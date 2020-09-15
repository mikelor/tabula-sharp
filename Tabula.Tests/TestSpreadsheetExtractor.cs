﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tabula.Csv;
using Tabula.Extractors;
using Tabula.Json;
using UglyToad.PdfPig.Core;
using Xunit;

namespace Tabula.Tests
{
    public class TestSpreadsheetExtractor
    {
        public static TableRectangle[] EXPECTED_RECTANGLES = new TableRectangle[]
        {
            //new TableRectangle(40.0f, 18.0f, 208.0f, 40.0f),
            //new TableRectangle(84.0f, 18.0f, 962.0f, 464.0f)
        };

        private static readonly Ruling[] VERTICAL_RULING_LINES = new[]
        {
            new Ruling(new PdfPoint(18.0, 40.0), new PdfPoint(18.0,   40.0 + 18.0)), // 40.0f, 18.0f, 0.0f, 40.0f),
            new Ruling(new PdfPoint(70.0, 44.0), new PdfPoint(70.0,   44.0 + 36.0)), // 44.0f, 70.0f, 0.0f, 36.0f),
            new Ruling(new PdfPoint(226.0, 40.0), new PdfPoint(226.0, 40.0 + 40.0)), // 40.0f, 226.0f, 0.0f, 40.0f)
        };

        private static readonly Ruling[] HORIZONTAL_RULING_LINES = new[]
        {
            new Ruling(new PdfPoint(18.0, 40.0), new PdfPoint(208.0 + 18.0, 40.0)), // 40.0f, 18.0f, 208.0f, 0.0f),
            new Ruling(new PdfPoint(18.0, 44.0), new PdfPoint(208.0 + 18.0, 44.0)), // 44.0f, 18.0f, 208.0f, 0.0f),
            new Ruling(new PdfPoint(18.0, 50.0), new PdfPoint(208.0 + 18.0, 50.0)), // 50.0f, 18.0f, 208.0f, 0.0f),
            new Ruling(new PdfPoint(18.0, 54.0), new PdfPoint(208.0 + 18.0, 54.0)), // 54.0f, 18.0f, 208.0f, 0.0f),
            new Ruling(new PdfPoint(18.0, 60.0), new PdfPoint(208.0 + 18.0, 60.0)), // 60.0f, 18.0f, 208.0f, 0.0f),
            new Ruling(new PdfPoint(18.0, 64.0), new PdfPoint(208.0 + 18.0, 64.0)), // 64.0f, 18.0f, 208.0f, 0.0f),
            new Ruling(new PdfPoint(18.0, 70.0), new PdfPoint(208.0 + 18.0, 70.0)), // 70.0f, 18.0f, 208.0f, 0.0f),
            new Ruling(new PdfPoint(18.0, 74.0), new PdfPoint(208.0 + 18.0, 74.0)), // 74.0f, 18.0f, 208.0f, 0.0f),
            new Ruling(new PdfPoint(18.0, 80.0), new PdfPoint(208.0 + 18.0, 80.0)), // 80.0f, 18.0f, 208.0f, 0.0f)
        };

        private static readonly Cell[] EXPECTED_CELLS = new Cell[]
        {
            new Cell(new PdfRectangle(18.0, 40.0 - 4.0, 18.0 + 208.0, 40.0)), // 40.0f, 18.0f, 208.0f, 4.0f),
            new Cell(new PdfRectangle(18.0, 44.0 - 6.0, 18.0 + 52.0, 44.0)), // 44.0f, 18.0f, 52.0f, 6.0f),
            new Cell(new PdfRectangle(18.0, 50.0 - 4.0, 18.0 + 52.0, 50.0)), // 50.0f, 18.0f, 52.0f, 4.0f),
            new Cell(new PdfRectangle(18.0, 54.0 - 6.0, 18.0 + 52.0, 54.0)), // 54.0f, 18.0f, 52.0f, 6.0f),
            new Cell(new PdfRectangle(18.0, 60.0 - 4.0, 18.0 + 52.0, 60.0)), // 60.0f, 18.0f, 52.0f, 4.0f),
            new Cell(new PdfRectangle(18.0, 64.0 - 6.0, 18.0 + 52.0, 64.0)), // 64.0f, 18.0f, 52.0f, 6.0f),
            new Cell(new PdfRectangle(18.0, 70.0 - 4.0, 18.0 + 52.0, 70.0)), // 70.0f, 18.0f, 52.0f, 4.0f),
            new Cell(new PdfRectangle(18.0, 74.0 - 6.0, 18.0 + 52.0, 74.0)), // 74.0f, 18.0f, 52.0f, 6.0f),
            new Cell(new PdfRectangle(70.0, 44.0 - 6.0, 70.0 + 156.0, 44.0)), // 44.0f, 70.0f, 156.0f, 6.0f),
            new Cell(new PdfRectangle(70.0, 50.0 - 4.0, 70.0 + 156.0, 50.0)), // 50.0f, 70.0f, 156.0f, 4.0f),
            new Cell(new PdfRectangle(70.0, 54.0 - 6.0, 70.0 + 156.0, 54.0)), // 54.0f, 70.0f, 156.0f, 6.0f),
            new Cell(new PdfRectangle(70.0, 60.0 - 4.0, 70.0 + 156.0, 60.0)), // 60.0f, 70.0f, 156.0f, 4.0f),
            new Cell(new PdfRectangle(70.0, 64.0 - 6.0, 70.0 + 156.0, 64.0)), // 64.0f, 70.0f, 156.0f, 6.0f),
            new Cell(new PdfRectangle(70.0, 70.0 - 4.0, 70.0 + 156.0, 70.0)), // 70.0f, 70.0f, 156.0f, 4.0f),
            new Cell(new PdfRectangle(70.0, 74.0 - 6.0, 70.0 + 156.0, 74.0)), // 74.0f, 70.0f, 156.0f, 6.0f)
        };

        private static readonly Ruling[][] SINGLE_CELL_RULINGS = new[]
        {
            new Ruling[]
            {
                new Ruling(new PdfPoint(151.653545f, 185.66929f), new PdfPoint(380.73438f, 185.66929f)),
                new Ruling(new PdfPoint(151.653545f, 314.64567f), new PdfPoint(380.73438f, 314.64567f))
            },
            new Ruling[]
            {
                new Ruling(new PdfPoint(151.653545f, 185.66929f), new PdfPoint(151.653545f, 314.64567f)),
                new Ruling(new PdfPoint(380.73438f, 185.66929f), new PdfPoint(380.73438f, 314.64567f))
            }
        };

        private static readonly Ruling[][] TWO_SINGLE_CELL_RULINGS = new[]
        {
            new Ruling[]
            {
                new Ruling(new PdfPoint(151.653545f, 185.66929f), new PdfPoint(287.4074f, 185.66929f)),
                new Ruling(new PdfPoint(151.653545f, 262.101f), new PdfPoint(287.4074f, 262.101f)),
                new Ruling(new PdfPoint(232.44095f, 280.62992f), new PdfPoint(368.1948f, 280.62992f)),
                new Ruling(new PdfPoint(232.44095f, 357.06164f), new PdfPoint(368.1948f, 357.06164f))
            },
            new Ruling[]
            {
                new Ruling(new PdfPoint(151.653545f, 185.66929f), new PdfPoint(151.653545f, 262.101f)),
                new Ruling(new PdfPoint(287.4074f, 185.66929f), new PdfPoint(287.4074f, 262.101f)),
                new Ruling(new PdfPoint(232.44095f, 280.62992f), new PdfPoint(232.44095f, 357.06164f)),
                new Ruling(new PdfPoint(368.1948f, 280.62992f), new PdfPoint(368.1948f, 357.06164f))
            }
        };

        private static readonly Ruling[] EXTERNALLY_DEFINED_RULINGS = new Ruling[]
        {
            // height of page = 792
            // horizontal lines
            new Ruling(new PdfPoint(320.0,      792 - 285.0), new PdfPoint(564.4409,  792 - 285.0)),
            new Ruling(new PdfPoint(320.0,      792 - 457.0), new PdfPoint(564.4409,  792 - 457.0)),
            new Ruling(new PdfPoint(320.0,      792 - 331.0), new PdfPoint(564.4409,  792 - 331.0)),
            new Ruling(new PdfPoint(320.0,      792 - 315.0), new PdfPoint(564.4409,  792 - 315.0)),
            new Ruling(new PdfPoint(320.0,      792 - 347.0), new PdfPoint(564.4409,  792 - 347.0)),
            new Ruling(new PdfPoint(320.0,      792 - 363.0), new PdfPoint(564.44088, 792 - 363.0)),
            new Ruling(new PdfPoint(320.0,      792 - 379.0), new PdfPoint(564.44087, 792 - 379.0)),
            new Ruling(new PdfPoint(320.0,      792 - 395.5), new PdfPoint(564.44086, 792 - 395.5)),
            new Ruling(new PdfPoint(320.00006,  792 - 415.0), new PdfPoint(564.4409,  792 - 415.0)),
            new Ruling(new PdfPoint(320.00007,  792 - 431.0), new PdfPoint(564.4409,  792 - 431.0)),

            // vertical lines
            new Ruling(new PdfPoint(320.0,      792 - 457.0), new PdfPoint(320.0,     792 - 285.0)),
            new Ruling(new PdfPoint(565.0,      792 - 457.0), new PdfPoint(564.4409,  792 - 285.0)),
            new Ruling(new PdfPoint(470.5542,   792 - 457.0), new PdfPoint(470.36865, 792 - 285.0))
        };

        private static readonly Ruling[] EXTERNALLY_DEFINED_RULINGS2 = new Ruling[]
        {
            // height of page = 792
            // horizontal lines
            new Ruling(new PdfPoint(51.796964,  792 - 180.0), new PdfPoint(560.20312,       792 - 180.0)),
            new Ruling(new PdfPoint(51.797017,  792 - 219.0), new PdfPoint(560.2031,        792 - 219.0)),
            new Ruling(new PdfPoint(51.797,     792 - 239.0), new PdfPoint(560.2031,        792 - 239.0)),
            new Ruling(new PdfPoint(51.797,     792 - 262.0), new PdfPoint(560.20312,       792 - 262.0)),
            new Ruling(new PdfPoint(51.797,     792 - 283.50247), new PdfPoint(560.05024,   792 - 283.50247)),
            new Ruling(new PdfPoint(51.796964,  792 - 309.0), new PdfPoint(560.20312,       792 - 309.0)),
            new Ruling(new PdfPoint(51.796982,  792 - 333.0), new PdfPoint(560.20312,       792 - 333.0)),
            new Ruling(new PdfPoint(51.797,     792 - 366.0), new PdfPoint(560.20312,       792 - 366.0)),
            
            // vertical lines
            new Ruling(new PdfPoint(52.0,       792 - 366.0), new PdfPoint(51.797,      792 - 181.0)),
            new Ruling(new PdfPoint(208.62891,  792 - 366.0), new PdfPoint(208.62891,   792 - 181.0)),
            new Ruling(new PdfPoint(357.11328,  792 - 366.0), new PdfPoint(357.0,       792 - 181.0)),
            new Ruling(new PdfPoint(560.11328,  792 - 366.0), new PdfPoint(560.0,       792 - 181.0))
        };

        [Fact(Skip = "fails as of v0.7")]
        public void testLinesToCells()
        {
            List<Cell> cells = SpreadsheetExtractionAlgorithm.findCells(HORIZONTAL_RULING_LINES.ToList(), VERTICAL_RULING_LINES.ToList());
            Utils.sort(cells, new TableRectangle.ILL_DEFINED_ORDER());
            List<Cell> expected = EXPECTED_CELLS.ToList();
            Utils.sort(expected, new TableRectangle.ILL_DEFINED_ORDER());
            Assert.Equal(expected, cells);
        }

        [Fact]
        public void testDetectSingleCell()
        {
            List<Cell> cells = SpreadsheetExtractionAlgorithm.findCells(SINGLE_CELL_RULINGS[0].ToList(), SINGLE_CELL_RULINGS[1].ToList());
            Assert.Single(cells);
            Cell cell = cells[0];
            Assert.True(Utils.feq(151.65355, cell.getLeft()));
            Assert.True(Utils.feq(185.6693, cell.getBottom())); // .getTop()
            Assert.True(Utils.feq(229.08083, cell.getWidth()));
            Assert.True(Utils.feq(128.97636, cell.getHeight()));
        }

        [Fact]
        public void testDetectTwoSingleCells()
        {
            List<Cell> cells = SpreadsheetExtractionAlgorithm.findCells(TWO_SINGLE_CELL_RULINGS[0].ToList(), TWO_SINGLE_CELL_RULINGS[1].ToList());
            Assert.Equal(2, cells.Count);
            // should not overlap
            Assert.False(cells[0].intersects(cells[1]));
        }

        [Fact(Skip = "TODO")]
        public void testFindSpreadsheetsFromCells()
        {
            //CSVParser parse = org.apache.commons.csv.CSVParser.parse(new File("src/test/resources/technology/tabula/csv/TestSpreadsheetExtractor-CELLS.csv"),
            //    Charset.forName("utf-8"),
            //    CSVFormat.DEFAULT);

            //List<Cell> cells = new ArrayList<>();

            //for (CSVRecord record : parse) {
            //    cells.add(new Cell(Float.parseFloat(record[0]),
            //            Float.parseFloat(record[1]),
            //            Float.parseFloat(record[2]),
            //            Float.parseFloat(record[3])));
            //}

            //List<Rectangle> expected = Arrays.asList(EXPECTED_RECTANGLES);
            //Collections.sort(expected, Rectangle.ILL_DEFINED_ORDER);
            //List<Rectangle> foundRectangles = SpreadsheetExtractionAlgorithm.findSpreadsheetsFromCells(cells);
            //Collections.sort(foundRectangles, Rectangle.ILL_DEFINED_ORDER);
            //assertTrue(foundRectangles.equals(expected));
        }

        [Fact(Skip = "TODO Add assertions")]
        public void testSpreadsheetExtraction()
        {
            //PageArea page = UtilsForTesting.getAreaFromFirstPage("Resources/argentina_diputados_voting_record.pdf", 269.875f, 12.75f, 790.5f, 561f);
            //SpreadsheetExtractionAlgorithm.findCells(page.getHorizontalRulings(), page.getVerticalRulings());
        }

        [Fact(Skip = "fails as of v0.7")]
        public void testSpanningCells()
        {
            PageArea page = UtilsForTesting.getPage("Resources/spanning_cells.pdf", 1);
            string expectedJson = UtilsForTesting.loadJson("Resources/json/spanning_cells.json");
            SpreadsheetExtractionAlgorithm se = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = se.extract(page);
            Assert.Equal(2, tables.Count);

            StringBuilder sb = new StringBuilder();
            (new JSONWriter()).write(sb, tables);
            Assert.Equal(expectedJson, sb.ToString());

            /*
            using (var stream = new MemoryStream())
            using (var sb = new StreamWriter(stream) { AutoFlush = true })
            {
                (new JSONWriter()).write(sb, tables);

                var reader = new StreamReader(stream);
                stream.Position = 0;
                var s = reader.ReadToEnd().Trim(); // trim to remove last new line
                Assert.Equal(expectedJson, s); //.Replace("\r\n", "\n"));
            }
            */
        }

        [Fact]
        public void testSpanningCellsToCsv()
        {
            PageArea page = UtilsForTesting.getPage("Resources/spanning_cells.pdf", 1);
            string expectedCsv = UtilsForTesting.loadCsv("Resources/csv/spanning_cells.csv");
            SpreadsheetExtractionAlgorithm se = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = se.extract(page);
            Assert.Equal(2, tables.Count);

            StringBuilder sb = new StringBuilder();
            (new CSVWriter()).write(sb, tables);
            Assert.Equal(expectedCsv, sb.ToString().Replace("\r\n", "\n").Trim());

            /*
            using (var stream = new MemoryStream())
            using (var sb = new StreamWriter(stream) { AutoFlush = true })
            {
                (new CSVWriter()).write(sb, tables);

                var reader = new StreamReader(stream);
                stream.Position = 0;
                var s = reader.ReadToEnd().Trim(); // trim to remove last new line
                Assert.Equal(expectedCsv, s.Replace("\r\n", "\n")); //.Replace("\r\n", "\n"));
            }
            */
        }

        [Fact]
        public void testIncompleteGrid()
        {
            PageArea page = UtilsForTesting.getPage("Resources/china.pdf", 1);
            SpreadsheetExtractionAlgorithm se = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = se.extract(page);
            Assert.Equal(2, tables.Count);
        }

        [Fact]
        public void testNaturalOrderOfRectanglesDoesNotBreakContract()
        {
            PageArea page = UtilsForTesting.getPage("Resources/us-017.pdf", 2);
            SpreadsheetExtractionAlgorithm se = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = se.extract(page);

            string expected = "Project,Agency,Institution\r\nNanotechnology and its publics,NSF,Pennsylvania State University\r\n\"Public information and deliberation in nanoscience and\rnanotechnology policy (SGER)\",Interagency,\"North Carolina State\rUniversity\"\r\n\"Social and ethical research and education in agrifood\rnanotechnology (NIRT)\",NSF,Michigan State University\r\n\"From laboratory to society: developing an informed\rapproach to nanoscale science and engineering (NIRT)\",NSF,University of South Carolina\r\nDatabase and innovation timeline for nanotechnology,NSF,UCLA\r\nSocial and ethical dimensions of nanotechnology,NSF,University of Virginia\r\n\"Undergraduate exploration of nanoscience,\rapplications and societal implications (NUE)\",NSF,\"Michigan Technological\rUniversity\"\r\n\"Ethics and belief inside the development of\rnanotechnology (CAREER)\",NSF,University of Virginia\r\n\"All centers, NNIN and NCN have a societal\rimplications components\",\"NSF, DOE,\rDOD, and NIH\",\"All nanotechnology centers\rand networks\""; // \r\n

            StringBuilder sb = new StringBuilder();
            (new CSVWriter()).write(sb, tables[0]);
            string result = sb.ToString().Trim();
            Assert.Equal(expected.Replace("\r\n", "\r"), result.Replace("\r\n", "\r"));

            /*
            using (var stream = new MemoryStream())
            using (var sb = new StreamWriter(stream) { AutoFlush = true })
            {
                (new CSVWriter()).write(sb, tables);

                var reader = new StreamReader(stream);
                stream.Position = 0;
                var result = reader.ReadToEnd().Trim(); // trim to remove last new line
                Assert.Equal(expected, result); //.Replace("\r\n", "\n"));
            }
            */
        }

        [Fact]
        public void testMergeLinesCloseToEachOther()
        {
            PageArea page = UtilsForTesting.getPage("Resources/20.pdf", 1);
            List<Ruling> rulings = page.getVerticalRulings();
            Assert.Equal(6, rulings.Count);

            //float[] expectedRulings = new float[] { 105.549774, 107.52332, 160.58167, 377.1792, 434.95804, 488.21783 };
            double[] expectedRulings = new double[] { 105.554812, 107.522417, 160.568521, 377.172662, 434.963828, 488.229949 };

            var lefts = rulings.Select(x => x.getLeft()).ToArray();
            for (int i = 0; i < rulings.Count; i++)
            {
                Assert.Equal(expectedRulings[i], rulings[i].getLeft(), 2);
            }
        }

        [Fact(Skip = "fails as of v0.7")]
        public void testSpreadsheetWithNoBoundingFrameShouldBeSpreadsheet()
        {
            PageArea page = UtilsForTesting.getAreaFromPage("Resources/spreadsheet_no_bounding_frame.pdf", 1,
                new PdfRectangle(58.9, 842 - 654.7, 536.12, 698)); // 842 - 150.56)); // 150.56f, 58.9f, 654.7f, 536.12f);

            String expectedCsv = UtilsForTesting.loadCsv("Resources/csv/spreadsheet_no_bounding_frame.csv");

            SpreadsheetExtractionAlgorithm se = new SpreadsheetExtractionAlgorithm();
            bool isTabular = se.isTabular(page);
            Assert.True(isTabular);
            List<Table> tables = se.extract(page);

            StringBuilder sb = new StringBuilder();
            (new CSVWriter()).write(sb, tables[0]);
            Assert.Equal(expectedCsv, sb.ToString());

            /*
            using (var stream = new MemoryStream())
            using (var sb = new StreamWriter(stream) { AutoFlush = true })
            {
                (new CSVWriter()).write(sb, tables[0]);

                var reader = new StreamReader(stream);
                stream.Position = 0;
                var result = reader.ReadToEnd().Trim(); // trim to remove last new line

                Assert.Equal(expectedCsv, result.Replace("\r\n", "\n"));
            }
            */
        }

        [Fact(Skip = "TODO")]
        public void testExtractSpreadsheetWithinAnArea()
        {
            //PageArea page = UtilsForTesting.getAreaFromPage("Resources/puertos1.pdf", 1, 273.9035714285714f, 30.32142857142857f, 554.8821428571429f, 546.7964285714286f);
            //SpreadsheetExtractionAlgorithm se = new SpreadsheetExtractionAlgorithm();
            //List<Table> tables = se.extract(page);
            //Table table = tables[0];
            //Assert.Equal(15, table.getRows().Count);

            //String expected = "\"\",TM,M.U$S,TM,M.U$S,TM,M.U$S,TM,M.U$S,TM,M.U$S,TM,M.U$S,TM\n" +
            //        "Peces vivos,1,25,1,23,2,38,1,37,2,67,2,89,1\n" +
            //        "\"Pescado fresco\n" +
            //        "o refrigerado.\n" +
            //        "exc. filetes\",7.704,7.175,8.931,6.892,12.635,10.255,16.742,13.688,14.357,11.674,13.035,13.429,9.727\n" +
            //        "\"Pescado congelado\n" +
            //        "exc. filetes\",90.560,105.950,112.645,108.416,132.895,115.874,152.767,133.765,148.882,134.847,156.619,165.134,137.179\n" +
            //        "\"Filetes y demás car-\n" +
            //        "nes de pescado\",105.434,200.563,151.142,218.389,152.174,227.780,178.123,291.863,169.422,313.735,176.427,381.640,144.814\n" +
            //        "\"Pescado sec./sal./\n" +
            //        "en salm. har./pol./\n" +
            //        "pell. aptos\n" +
            //        "p/c humano\",6.837,14.493,6.660,9.167,14.630,17.579,18.150,21.302,18.197,25.739,13.460,23.549,11.709\n" +
            //        "Crustáceos,61.691,375.798,52.488,251.043,47.635,387.783,27.815,217.443,7.123,86.019,39.488,373.583,45.191\n" +
            //        "Moluscos,162.027,174.507,109.436,111.443,90.834,104.741,57.695,109.141,98.182,206.304,187.023,251.352,157.531\n" +
            //        "\"Prod. no exp. en\n" +
            //        "otros capítulos.\n" +
            //        "No apto p/c humano\",203,328,7,35,521,343,\"1,710\",\"1,568\",125,246,124,263,131\n" +
            //        "\"Grasas y aceites de\n" +
            //        "pescado y mamíferos\n" +
            //        "marinos\",913,297,\"1,250\",476,\"1,031\",521,\"1,019\",642,690,483,489,710,959\n" +
            //        "\"Extractos y jugos de\n" +
            //        "pescado y mariscos\",5,25,1,3,4,4,31,93,39,117,77,230,80\n" +
            //        "\"Preparaciones y con-\n" +
            //        "servas de pescado\",846,\"3,737\",\"1,688\",\"4,411\",\"1,556\",\"3,681\",\"2,292\",\"5,474\",\"2,167\",\"7,494\",\"2,591\",\"8,833\",\"2,795\"\n" +
            //        "\"Preparaciones y con-\n" +
            //        "servas de mariscos\",348,\"3,667\",345,\"1,771\",738,\"3,627\",561,\"2,620\",607,\"3,928\",314,\"2,819\",250\n" +
            //        "\"Harina, polvo y pe-\n" +
            //        "llets de pescado.No\n" +
            //        "aptos p/c humano\",\"16,947\",\"8,547\",\"11,867\",\"6,315\",\"32,528\",\"13,985\",\"37,313\",\"18,989\",\"35,787\",\"19,914\",\"37,821\",\"27,174\",\"30,000\"\n" +
            //        "TOTAL,\"453,515\",\"895,111\",\"456,431\",\"718,382\",\"487,183\",\"886,211\",\"494,220\",\"816,623\",\"495,580\",\"810,565\",\"627,469\",\"1,248,804\",\"540,367\"\n";


            //// TODO add better assertions
            //StringBuilder sb = new StringBuilder();
            //(new CSVWriter()).write(sb, tables[0]);
            //String result = sb.ToString();

            //List<CSVRecord> parsedExpected = org.apache.commons.csv.CSVParser.parse(expected, CSVFormat.EXCEL).getRecords();
            //List<CSVRecord> parsedResult = org.apache.commons.csv.CSVParser.parse(result, CSVFormat.EXCEL).getRecords();

            //Assert.Equal(parsedResult.Count, parsedExpected.Count);
            //for (int i = 0; i < parsedResult.Count; i++)
            //{
            //    Assert.Equal(parsedResult[i].size(), parsedExpected[i].size());
            //}
        }

        [Fact]
        public void testAlmostIntersectingRulingsShouldIntersect()
        {
            Ruling v = new Ruling(new PdfPoint(555.960876f, 271.569641f), new PdfPoint(555.960876f, 786.899902f));
            Ruling h = new Ruling(new PdfPoint(25.620499f, 786.899902f), new PdfPoint(555.960754f, 786.899902f));
            SortedDictionary<PdfPoint, Ruling[]> m = Ruling.findIntersections(new Ruling[] { h }.ToList(), new Ruling[] { v }.ToList());
            Assert.Single(m.Values);
        }

        /*
        [Fact(Skip = "TODO add assertions")]
        public void testDontRaiseSortException()
        {
            //PageArea page = UtilsForTesting.getAreaFromPage("Resources/us-017.pdf", 2, 446.0f, 97.0f, 685.0f, 520.0f);
            //page.getText();
            //SpreadsheetExtractionAlgorithm bea = new SpreadsheetExtractionAlgorithm();
            //bea.extract(page)[0];
        }
        */

        [Fact(Skip = "fails as of v0.7")]
        public void testShouldDetectASingleSpreadsheet()
        {
            PageArea page = UtilsForTesting.getAreaFromPage("Resources/offense.pdf", 1, new PdfRectangle(16.44, 792 - 680.85, 597.84, 792 - 16.44)); // 68.08f, 16.44f, 680.85f, 597.84f);
            SpreadsheetExtractionAlgorithm bea = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = bea.extract(page);
            Assert.Single(tables);
        }

        [Fact]
        public void testExtractTableWithExternallyDefinedRulings()
        {
            PageArea page = UtilsForTesting.getPage("Resources/us-007.pdf", 1);
            SpreadsheetExtractionAlgorithm bea = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = bea.extract(page, EXTERNALLY_DEFINED_RULINGS.ToList());
            Assert.Single(tables);
            Table table = tables[0];
            Assert.Equal(18, table.cells.Count);

            var rows = table.getRows();

            Assert.Equal("Payroll Period", rows[0][0].getText());
            Assert.Equal("One Withholding\rAllowance", rows[0][1].getText());
            Assert.Equal("Weekly", rows[1][0].getText());
            Assert.Equal("$71.15", rows[1][1].getText());
            Assert.Equal("Biweekly", rows[2][0].getText());
            Assert.Equal("142.31", rows[2][1].getText());
            Assert.Equal("Semimonthly", rows[3][0].getText());
            Assert.Equal("154.17", rows[3][1].getText());
            Assert.Equal("Monthly", rows[4][0].getText());
            Assert.Equal("308.33", rows[4][1].getText());
            Assert.Equal("Quarterly", rows[5][0].getText());
            Assert.Equal("925.00", rows[5][1].getText());
            Assert.Equal("Semiannually", rows[6][0].getText());
            Assert.Equal("1,850.00", rows[6][1].getText());
            Assert.Equal("Annually", rows[7][0].getText());
            Assert.Equal("3,700.00", rows[7][1].getText());
            Assert.Equal("Daily or Miscellaneous\r(each day of the payroll period)", rows[8][0].getText());
            Assert.Equal("14.23", rows[8][1].getText());
        }

        [Fact]
        public void testAnotherExtractTableWithExternallyDefinedRulings()
        {
            PageArea page = UtilsForTesting.getPage("Resources/us-024.pdf", 1);
            SpreadsheetExtractionAlgorithm bea = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = bea.extract(page, EXTERNALLY_DEFINED_RULINGS2.ToList());
            Assert.Single(tables);
            Table table = tables[0];

            Assert.Equal("Total Supply", table.getRows()[4][0].getText());
            Assert.Equal("6.6", table.getRows()[6][2].getText());
        }

        [Fact]
        public void testSpreadsheetsSortedByTopAndRight()
        {
            PageArea page = UtilsForTesting.getPage("Resources/sydney_disclosure_contract.pdf", 1);

            SpreadsheetExtractionAlgorithm sea = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = sea.extract(page);
            for (int i = 1; i < tables.Count; i++)
            {
                Assert.True(tables[i - 1].getTop() >= tables[i].getTop()); // Assert.True(tables[i - 1].getTop() <= tables[i].getTop());
            }
        }

        [Fact]
        public void testDontStackOverflowQuicksort()
        {
            PageArea page = UtilsForTesting.getPage("Resources/failing_sort.pdf", 1);

            SpreadsheetExtractionAlgorithm sea = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = sea.extract(page);
            for (int i = 1; i < tables.Count; i++)
            {
                Assert.True(tables[i - 1].getTop() >= tables[i].getTop()); //Assert.True(tables[i - 1].getTop() <= tables[i].getTop());
            }
        }

        [Fact(Skip = "RtL text to do later")]
        public void testRTL()
        {
            PageArea page = UtilsForTesting.getPage("Resources/arabic.pdf", 1);
            SpreadsheetExtractionAlgorithm sea = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = sea.extract(page);
            // Assert.Equal(1, tables.size());
            Table table = tables[0];

            Assert.Equal("اسمي سلطان", table.getRows()[1][1].getText());
            Assert.Equal("من اين انت؟", table.getRows()[2][1].getText());
            Assert.Equal("1234", table.getRows()[3][0].getText());
            Assert.Equal("هل انت شباك؟", table.getRows()[4][0].getText());
            Assert.Equal("انا من ولاية كارولينا الشمال", table.getRows()[2][0].getText()); // conjoined lam-alif gets missed
            Assert.Equal("اسمي Jeremy في الانجليزية", table.getRows()[4][1].getText()); // conjoined lam-alif gets missed
            Assert.Equal("عندي 47 قطط", table.getRows()[3][1].getText()); // the real right answer is 47.
            Assert.Equal("Jeremy is جرمي in Arabic", table.getRows()[5][0].getText()); // the real right answer is 47.
            Assert.Equal("مرحباً", table.getRows()[1][0].getText()); // really ought to be ً, but this is forgiveable for now

            // there is one remaining problems that are not yet addressed
            // - diacritics (e.g. Arabic's tanwinً and probably Hebrew nekudot) are put in the wrong place.
            // this should get fixed, but this is a good first stab at the problem.

            // these (commented-out) tests reflect the theoretical correct answer,
            // which is not currently possible because of the two problems listed above
            // Assert.Equal("مرحباً",                       table.getRows()[0][0].getText()); // really ought to be ً, but this is forgiveable for now
        }


        [Fact(Skip = "RtL text to do later")]
        public void testRealLifeRTL()
        {
            PageArea page = UtilsForTesting.getPage("Resources/mednine.pdf", 1);
            SpreadsheetExtractionAlgorithm sea = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = sea.extract(page);
            Assert.Single(tables);
            Table table = tables[0];
            var rows = table.getRows();

            Assert.Equal("الانتخابات التشريعية  2014", rows[0][0].getText()); // the doubled spaces might be a bug in my implementation. // bobld: missing space or worng words order
            Assert.Equal("ورقة كشف نتائج دائرة مدنين", rows[1][0].getText());
            Assert.Equal("426", rows[4][0].getText());
            Assert.Equal("63", rows[4][1].getText());
            Assert.Equal("43", rows[4][2].getText());
            Assert.Equal("56", rows[4][3].getText());
            Assert.Equal("58", rows[4][4].getText());
            Assert.Equal("49", rows[4][5].getText());
            Assert.Equal("55", rows[4][6].getText());
            Assert.Equal("33", rows[4][7].getText());
            Assert.Equal("32", rows[4][8].getText());
            Assert.Equal("37", rows[4][9].getText());
            Assert.Equal("قائمة من أجل تحقيق سلطة الشعب", rows[4][10].getText());

            // there is one remaining problems that are not yet addressed
            // - diacritics (e.g. Arabic's tanwinً and probably Hebrew nekudot) are put in the wrong place.
            // this should get fixed, but this is a good first stab at the problem.

            // these (commented-out) tests reflect the theoretical correct answer,
            // which is not currently possible because of the two problems listed above
            //Assert.Equal("مرحباً", rows[0][0].getText()); // really ought to be ً, but this is forgiveable for now
        }

        [Fact]
        public void testExtractColumnsCorrectly3()
        {
            // top,     left,   bottom,  right
            // 106.01f, 48.09f, 227.31f, 551.89f
            // bottom = 792 - 227.31 = 564.69
            // top =  792 - 106.01 = 685.99
            PageArea page = UtilsForTesting.getAreaFromFirstPage("Resources/frx_2012_disclosure.pdf", new PdfRectangle(48.09, 564.69, 551.89, 684.99)); // changed 685.99 to 684.99 because was adding an empty row at the top
            SpreadsheetExtractionAlgorithm sea = new SpreadsheetExtractionAlgorithm();
            Table table = sea.extract(page)[0];
            var rows = table.getRows();
            Assert.Equal("REGIONAL PULMONARY & SLEEP\rMEDICINE", table.getRows()[8][1].getText());
        }

        [Fact]
        public void testSpreadsheetExtractionIssue656()
        {
            // page height = 482, width 762.3 // 612
            // top,     left,    bottom,   right
            // 56.925f, 24.255f, 549.945f, 786.555f);
            PageArea page = UtilsForTesting.getAreaFromFirstPage("Resources/Publication_of_award_of_Bids_for_Transport_Sector__August_2016.pdf", new PdfRectangle(24.255, 71, 786.555, 553));
            string expectedCsv = UtilsForTesting.loadCsv("Resources/csv/Publication_of_award_of_Bids_for_Transport_Sector__August_2016.csv");

            SpreadsheetExtractionAlgorithm sea = new SpreadsheetExtractionAlgorithm();
            List<Table> tables = sea.extract(page);
            Assert.Single(tables);
            Table table = tables[0];

            StringBuilder sb = new StringBuilder();
            (new CSVWriter()).write(sb, table);
            String result = sb.ToString();
            Assert.Equal(expectedCsv.Replace("\n", "\r"), result.Replace("\r\n", "\n").Replace("\n", "\r").Trim());

            /*
            using (var stream = new MemoryStream())
            using (var sb = new StreamWriter(stream) { AutoFlush = true })
            {
                (new CSVWriter()).write(sb, table);

                var reader = new StreamReader(stream);
                stream.Position = 0;
                var s = reader.ReadToEnd().Trim(); // trim to remove last new line

                // is there an issue with \r and \n?
                Assert.Equal(expectedCsv.Replace("\n", "\r"), s.Replace("\r\n", "\n").Replace("\n", "\r"));
            }
            */
        }
    }
}
