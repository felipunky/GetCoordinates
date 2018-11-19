using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using GH_IO;
using Acc = Accord.Math;

namespace GetCoordinates
{

    public class GetCoordinates : GH_Component
    {

        public GetCoordinates() : base("Calculate coordinates from file", "GetCoordinates", "This component creates coordinates from a file", "THR34D5Workshop", "ExtractData")
        {



        }

        public override Guid ComponentGuid
        {

            get { return new Guid("A4C6E387-F0CD-4AF0-A371-8F1ACBC6D54D"); }

        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {

            pManager.AddTextParameter("PathToFile", "Path", "Path to directory where the grib2 file resides", GH_ParamAccess.item);
            pManager.AddBooleanParameter( "SelectRange", "Range", "If true, it outputs the coordinates within the specified range", GH_ParamAccess.item );
            pManager.AddNumberParameter( "MinimumLatitude", "MinLat", "Specifies the minimum latitude to take into account", GH_ParamAccess.item );
            pManager.AddNumberParameter( "MaximumLatitude", "MaxLat", "Specifies the maximum latitude to take into account", GH_ParamAccess.item );
            pManager.AddNumberParameter( "MinimumLongitude", "MinLon", "Specifies the minimum longitude to take into account", GH_ParamAccess.item );
            pManager.AddNumberParameter( "MaximumLongitude", "MaxLon", "Specifies the maximum longitude to take into account", GH_ParamAccess.item );

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {

            pManager.AddTextParameter("CatchErrors", "ERR", "Tell if there is an error while loading the libraries", GH_ParamAccess.item);
            pManager.AddNumberParameter("Latitude", "Lat", "Creates the latitudes from the grib2 file", GH_ParamAccess.list);
            pManager.AddNumberParameter("Longitude", "Lon", "Creates the longitudes from the grib2 file", GH_ParamAccess.list);

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            string output = "";

            try
            {

                GdalConfiguration.ConfigureOgr();

                GdalConfiguration.ConfigureGdal();

                output = "It works!";

            }

            catch (Exception e)
            {

                output = "{0} Exception caught. " + e;

            }

            string input = "";
            DA.GetData(0, ref input);
            string file = input;

            bool flag = false;
            DA.GetData(1, ref flag);

            double minLat = 0.0;
            DA.GetData(2, ref minLat);

            double maxLat = 0.0;
            DA.GetData(3, ref maxLat);

            double minLon = 0.0;
            DA.GetData(4, ref minLon);

            double maxLon = 0.0;
            DA.GetData(5, ref maxLon);

            var boolListX = new List<bool>();
            var boolListY = new List<bool>();
            var boolOut = new List<bool>();

            OSGeo.GDAL.Dataset ds = OSGeo.GDAL.Gdal.Open(file, OSGeo.GDAL.Access.GA_ReadOnly);

            double[] gt = new double[6];

            ds.GetGeoTransform(gt);

            var xres = gt[1];
            var yres = gt[5];

            var xsize = ds.RasterXSize;
            var ysize = ds.RasterYSize;

            var xmin = gt[0] + xres * 0.5;
            var xmax = gt[0] + (xres * xsize) - xres * 0.5;
            var ymin = gt[3] + (yres * ysize) + yres * 0.5;
            var ymax = gt[3] - yres * 0.5;

            var xx = EnumerableUtilities.RangePython(xmin, xmax + xres, xres).ToArray();
            var yy = EnumerableUtilities.RangePython(ymax + yres, ymin, yres).ToArray();

            var inMX = new List<double>().ToArray();
            var inMY = new List<double>().ToArray();

            var xNew = new List<double>();
            var yNew = new List<double>();

            var M = Acc.Matrix.MeshGrid(inMX, inMY);

            bool xFirst = false;

            if (flag == false)
            {

                M = Acc.Matrix.MeshGrid(yy, xx);
                xFirst = false;

            }

            else if (flag == true)
            {

                for (int i = 0; i < xx.Length; ++i)
                {

                    if (xx[i] > minLat && xx[i] < maxLat)
                    {

                        xNew.Add(xx[i]);

                    }

                }

                for (int i = 0; i < yy.Length; ++i)
                {

                    if (yy[i] > minLon && yy[i] < maxLon)
                    {

                        yNew.Add(yy[i]);

                    }

                }

                if (yNew.Count() < xNew.Count())
                {

                    M = Acc.Matrix.MeshGrid(yNew.ToArray(), xNew.ToArray());
                    xFirst = false;

                }

                else if (yNew.Count() > xNew.Count() || yNew.Count() == xNew.Count())
                {

                    M = Acc.Matrix.MeshGrid(xNew.ToArray(), yNew.ToArray());
                    xFirst = true;

                }

            }

            int xSize = 0, ySize = 0;

            if (xFirst == true)
            {

                xSize = M.Item1.GetLength(0);
                ySize = M.Item2.GetLength(0);

            }

            else
            {

                xSize = M.Item2.GetLength(0);
                ySize = M.Item1.GetLength(0);

            }

            var x = new double[xSize, ySize];
            var y = new double[ySize, xSize];

            if (xFirst == true)
            {

                x = M.Item1;
                y = M.Item2;

            }

            else
            {

                x = M.Item2;
                y = M.Item1;

            }

            x = M.Item2;
            y = M.Item1;

            ds = null;

            DA.SetData(0, output);
            DA.SetDataList(1, x);
            DA.SetDataList(2, y);

        }

    }

}