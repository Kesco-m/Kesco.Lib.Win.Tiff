using System;
using System.Collections;
using System.Drawing;

namespace Kesco.Lib.Win.Tiff
{
	public struct CompressionInfo
	{
		private CompressionType compressionType;
		private Int32 jpegQuality;
		private SubSampling jpegXSubSampling;
		private SubSampling jpegYSubSampling;
		private PredictorType predictor;

		public CompressionType CompressionType
		{
			get { return compressionType; }
			set { compressionType = value; }
		}

		public Int32 JpegQuality
		{
			get { return jpegQuality; }
			set { jpegQuality = value; }
		}

		public SubSampling JpegXSubSampling
		{
			get { return jpegXSubSampling; }
			set { jpegXSubSampling = value; }
		}

		public SubSampling JpegYSubSampling
		{
			get { return jpegYSubSampling; }
			set { jpegYSubSampling = value; }
		}

		public PredictorType Predictor
		{
			get { return predictor; }
			set { predictor = value; }
		}
	}

	public class PageInfo
	{
		protected byte[] annotation;
		protected Bitmap image;
		private CompressionInfo compression;

		public byte[] Annotation
		{
			get { return annotation; }
			set { annotation = value; }
		}

		public virtual Bitmap Image
		{
			get { return image; }
			set { image = value; }
		}

		public CompressionInfo Compression
		{
			get { return compression; }
			set { compression = value; }
		}

		public PageInfo()
		{
		}

		public virtual void Clear()
		{
			if (image != null)
			{
				image.Dispose();
				image = null;
			}
		}
	}

	public enum CompressionType
	{
		COMPRESSION_NONE = 1,	/* dump mode */
		COMPRESSION_CCITTRLE = 2,	/* CCITT modified Huffman RLE */
		COMPRESSION_CCITTFAX3 = 3,	/* CCITT Group 3 fax encoding */
		COMPRESSION_CCITTFAX4 = 4,	/* CCITT Group 4 fax encoding */
		COMPRESSION_LZW = 5,       /* Lempel-Ziv  & Welch */
		COMPRESSION_OJPEG = 6,	/* !6.0 JPEG */
		COMPRESSION_JPEG = 7,	/* %JPEG DCT compression */
		COMPRESSION_NEXT = 32766,	/* NeXT 2-bit RLE */
		COMPRESSION_CCITTRLEW = 32771,	/* #1 w/ word alignment */
		COMPRESSION_PACKBITS = 32773,	/* Macintosh RLE */
		COMPRESSION_THUNDERSCAN = 32809,	/* ThunderScan RLE */
		/* codes 32895-32898 are reserved for ANSI IT8 TIFF/IT <dkelly@apago.com) */
		COMPRESSION_IT8CTPAD = 32895,   /* IT8 CT w/padding */
		COMPRESSION_IT8LW = 32896,  /* IT8 Linework RLE */
		COMPRESSION_IT8MP = 32897, /*  IT8 Monochrome picture */
		COMPRESSION_IT8BL = 32898, /*  IT8 Binary line art */
		/*  compression codes 32908-32911 are reserved for Pixar */
		COMPRESSION_PIXARFILM = 32908, /*  Pixar companded 10bit LZW */
		COMPRESSION_PIXARLOG = 32909, /*  Pixar companded 11bit ZIP */
		COMPRESSION_DEFLATE = 32946, /*  Deflate compression */
		COMPRESSION_ADOBE_DEFLATE = 8, /*  Deflate compression,
						   as recognized by Adobe */
		/*  compression code 32947 is reserved for Oceana Matrix <dev@oceana.com> */
		COMPRESSION_DCS = 32947, /*  Kodak DCS encoding */
		COMPRESSION_JBIG = 34661, /*  ISO JBIG */
		COMPRESSION_SGILOG = 34676, /*  SGI Log Luminance RLE */
		COMPRESSION_SGILOG24 = 34677, /*  SGI Log 24-bit packed */
		COMPRESSION_JP2000 = 34712 /*  Leadtools JPEG2000 */
	}

	public enum SubSampling : int
	{
		Low = 1,
		Middle = 2,
		High = 5
	}

	public enum PredictorType
	{
		NONE = 1,
		HORIZONATAL = 2
	}
}