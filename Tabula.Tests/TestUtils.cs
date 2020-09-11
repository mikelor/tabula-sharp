﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tabula;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Core;
using Xunit;

namespace Tabula.Tests
{
    public class TestUtils
    {
        public static Ruling[] RULINGS = new[]
        {
            new Ruling(new PdfPoint(0, 0), new PdfPoint(1,1)),
            new Ruling(new PdfPoint(2, 2), new PdfPoint(3,3))
        };

        public static TableRectangle[] RECTANGLES = new[]
        {
            new TableRectangle(),
            new TableRectangle(new PdfRectangle(0, 0, 2, 4))
        };

        [Fact]
        public void testBoundsOfTwoRulings()
        {
            TableRectangle r = new TableRectangle(Utils.bounds(RULINGS)); //RULINGS.ToList();
            Assert.Equal(0, r.getMinX(), 0);
            Assert.Equal(0, r.getMinY(), 0);
            Assert.Equal(3, r.getWidth(), 0);
            Assert.Equal(3, r.getHeight(), 0);
        }

        [Fact]
        public void testBoundsOfOneEmptyRectangleAndAnotherNonEmpty()
        {
            TableRectangle r = Utils.bounds(RECTANGLES.ToList());
            Assert.Equal(r, RECTANGLES[1]);
        }

        [Fact]
        public void testBoundsOfOneRectangle()
        {
            List<TableRectangle> shapes = new List<TableRectangle>();
            shapes.Add(new TableRectangle(new PdfRectangle(0, 0, 20, 40)));
            TableRectangle r = Utils.bounds(shapes);
            Assert.Equal(r, shapes[0]);
        }

        [Fact]
        public void testParsePagesOption()
        {
            List<int> rv = Utils.parsePagesOption("1");
            Assert.Equal(new int[] { 1 }, rv.ToArray());

            rv = Utils.parsePagesOption("1-4");
            Assert.Equal(new int[] { 1, 2, 3, 4 }, rv.ToArray());

            rv = Utils.parsePagesOption("1-4,20-24");
            Assert.Equal(new int[] { 1, 2, 3, 4, 20, 21, 22, 23, 24 }, rv.ToArray());

            rv = Utils.parsePagesOption("all");
            Assert.Null(rv);

        }

        [Fact]  //@Test(expected= ParseException.class)
        public void testExceptionInParsePages() //throws ParseException
        {
            Assert.Throws<FormatException>(() => Utils.parsePagesOption("1-4,24-22"));
        }

        [Fact] //@Test(expected= ParseException.class)
        public void testAnotherExceptionInParsePages()// throws ParseException
        {
            Assert.Throws<FormatException>(() => Utils.parsePagesOption("quuxor"));
        }

        /*
        [Fact] //@Test
        public void testQuickSortEmptyList()
        {
            List<int> numbers = new ArrayList<>();
            QuickSort.sort(numbers);

            Assert.Equal(Collections.emptyList(), numbers);
        }
        */

        /*
        [Fact] //@Test
        public void testQuickSortOneElementList()
        {
            List<int> numbers = Arrays.asList(5);
            QuickSort.sort(numbers);

            Assert.Equal(Arrays.asList(5), numbers);
        }
        */

        /*
        [Fact] //@Test
        public void testQuickSortShortList()
        {
            List<int> numbers = Arrays.asList(4, 5, 6, 8, 7, 1, 2, 3);
            QuickSort.sort(numbers);

            Assert.Equal(Arrays.asList(1, 2, 3, 4, 5, 6, 7, 8), numbers);
        }
        */

        /*
        [Fact] //@Test
        public void testQuickSortLongList()
        {
            List<int> numbers = new ArrayList<>();
            List<int> expectedNumbers = new ArrayList<>();

            for (int i = 0; i <= 12000; i++)
            {
                numbers.add(12000 - i);
                expectedNumbers.add(i);
            }

            QuickSort.sort(numbers);

            Assert.Equal(expectedNumbers, numbers);
        }
        */

        [Fact(Skip = "Image conversion not available")]
        public void testJPEG2000DoesNotRaise()
        {
            /*
            using (PdfDocument pdf_document = PdfDocument.Open("Resources/jpeg2000.pdf"))
            {
                var page = pdf_document.GetPage1);
                Utils.pageConvertToImage(pdf_document, page, 360, ImageType.RGB);
            }
            */
        }
    }
}