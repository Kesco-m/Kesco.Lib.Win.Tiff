using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Media.Imaging;
using Microsoft.Win32.SafeHandles;

namespace Kesco.Lib.Win.Tiff
{
	public class LibTiffHelper : IDisposable
	{
		private bool first_time = true;
		private TIFFExtendProc _ParentExtender = null;
		private LibTiffHelperxxx extLT;
		private bool isUseLock = false;
		private object lockobject;

		public bool IsUseLock
		{
			get { return isUseLock; }
			set { isUseLock = value; }
		}

		public delegate int TIFFReadWriteProc(IntPtr thandle_t, IntPtr tdata_t, int tsize_t);
		public delegate uint TIFFSeekProc(IntPtr thandle_t, uint toff_t, int i);
		public delegate int TIFFCloseProc(IntPtr thandle_t);
		public delegate uint TIFFSizeProc(IntPtr thandle_t);
		public delegate int TIFFMapFileProc(IntPtr thandle_t, IntPtr tdata_t, ref uint toff_t);
		public delegate void TIFFUnmapFileProc(IntPtr thandle_t, byte[] tdata_t, uint toff_t);
		public delegate void TIFFExtendProc(IntPtr thandle_t);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl, SetLastError = true)]
		public delegate void TIFFErrorHandler(string module, string fmt, IntPtr ap);
		//public delegate void TIFFErrorHandler(string module, string fmt, string[] ap);

		public const int TIFFTAG_ANNOTATION = 32932;

		public const int TIFFTAG_SUBFILETYPE = 254;	/* subfile data descriptor */
		public const int FILETYPE_REDUCEDIMAGE = 0x1;	/* reduced resolution version */
		public const int FILETYPE_PAGE = 0x2;	/* one page of many */
		public const int FILETYPE_MASK = 0x4;	/* transparency mask */
		public const int TIFFTAG_OSUBFILETYPE = 255;	/* +kind of data in subfile */
		public const int OFILETYPE_IMAGE = 1;	/* full resolution image data */
		public const int OFILETYPE_REDUCEDIMAGE = 2;	/* reduced size image data */
		public const int OFILETYPE_PAGE = 3;	/* one page of many */
		public const int TIFFTAG_IMAGEWIDTH = 256;/* image width in pixels */
		public const int TIFFTAG_IMAGELENGTH = 257;	/* image height in pixels */
		public const int TIFFTAG_BITSPERSAMPLE = 258;	/* bits per channel (sample) */
		public const int TIFFTAG_COMPRESSION = 259;	/* data compression technique */
		public const int COMPRESSION_NONE = 1;	/* dump mode */
		public const int COMPRESSION_CCITTRLE = 2;	/* CCITT modified Huffman RLE */
		public const int COMPRESSION_CCITTFAX3 = 3;	/* CCITT Group 3 fax encoding */
		public const int COMPRESSION_CCITT_T4 = 3;       /* CCITT T.4 (TIFF 6 name) */
		public const int COMPRESSION_CCITTFAX4 = 4;	/* CCITT Group 4 fax encoding */
		public const int COMPRESSION_CCITT_T6 = 4;       /* CCITT T.6 (TIFF 6 name) */
		public const int COMPRESSION_LZW = 5;       /* Lempel-Ziv  & Welch */
		public const int COMPRESSION_OJPEG = 6;	/* !6.0 JPEG */
		public const int COMPRESSION_JPEG = 7;	/* %JPEG DCT compression */
		public const int COMPRESSION_NEXT = 32766;	/* NeXT 2-bit RLE */
		public const int COMPRESSION_CCITTRLEW = 32771;	/* #1 w/ word alignment */
		public const int COMPRESSION_PACKBITS = 32773;	/* Macintosh RLE */
		public const int COMPRESSION_THUNDERSCAN = 32809;	/* ThunderScan RLE */
		/* codes 32895-32898 are reserved for ANSI IT8 TIFF/IT <dkelly@apago.com) */
		public const int COMPRESSION_IT8CTPAD = 32895;   /* IT8 CT w/padding */
		public const int COMPRESSION_IT8LW = 32896;  /* IT8 Linework RLE */
		public const int COMPRESSION_IT8MP = 32897; /*  IT8 Monochrome picture */
		public const int COMPRESSION_IT8BL = 32898; /*  IT8 Binary line art */
		/*  compression codes 32908-32911 are reserved for Pixar */
		public const int COMPRESSION_PIXARFILM = 32908; /*  Pixar companded 10bit LZW */
		public const int COMPRESSION_PIXARLOG = 32909; /*  Pixar companded 11bit ZIP */
		public const int COMPRESSION_DEFLATE = 32946; /*  Deflate compression */
		public const int COMPRESSION_ADOBE_DEFLATE = 8; /*  Deflate compression,
						   as recognized by Adobe */
		/*  compression code 32947 is reserved for Oceana Matrix <dev@oceana.com> */
		public const int COMPRESSION_DCS = 32947; /*  Kodak DCS encoding */
		public const int COMPRESSION_JBIG = 34661; /*  ISO JBIG */
		public const int COMPRESSION_SGILOG = 34676; /*  SGI Log Luminance RLE */
		public const int COMPRESSION_SGILOG24 = 34677; /*  SGI Log 24-bit packed */
		public const int COMPRESSION_JP2000 = 34712; /*  Leadtools JPEG2000 */
		public const int TIFFTAG_PHOTOMETRIC = 262; /*  photometric interpretation */
		public const int PHOTOMETRIC_MINISWHITE = 0; /*  min value is white */
		public const int PHOTOMETRIC_MINISBLACK = 1; /*  min value is black */
		public const int PHOTOMETRIC_RGB = 2; /*  RGB color model */
		public const int PHOTOMETRIC_PALETTE = 3; /*  color map indexed */
		public const int PHOTOMETRIC_MASK = 4; /*  $holdout mask */
		public const int PHOTOMETRIC_SEPARATED = 5; /*  !color separations */
		public const int PHOTOMETRIC_YCBCR = 6; /*  !CCIR 601 */
		public const int PHOTOMETRIC_CIELAB = 8; /*  !1976 CIE L*a*b* */
		public const int PHOTOMETRIC_ICCLAB = 9; /*  ICC L*a*b* [Adobe TIFF Technote 4] */
		public const int PHOTOMETRIC_ITULAB = 10; /*  ITU L*a*b* */
		public const int PHOTOMETRIC_LOGL = 32844; /*  CIE Log2(L) */
		public const int PHOTOMETRIC_LOGLUV = 32845; /*  CIE Log2(L) (u',v') */
		public const int TIFFTAG_THRESHHOLDING = 263; /*  +thresholding used on data */
		public const int THRESHHOLD_BILEVEL = 1; /*  b&w art scan */
		public const int THRESHHOLD_HALFTONE = 2; /*  or dithered scan */
		public const int THRESHHOLD_ERRORDIFFUSE = 3; /*  usually floyd-steinberg */
		public const int TIFFTAG_CELLWIDTH = 264; /*  +dithering matrix width */
		public const int TIFFTAG_CELLLENGTH = 265; /*  +dithering matrix height */
		public const int TIFFTAG_FILLORDER = 266; /*  data order within a byte */
		public const int FILLORDER_MSB2LSB = 1; /*  most significant -> least */
		public const int FILLORDER_LSB2MSB = 2; /*  least significant -> most */
		public const int TIFFTAG_DOCUMENTNAME = 269; /*  name of doc. image is from */
		public const int TIFFTAG_IMAGEDESCRIPTION = 270; /*  info about image */
		public const int TIFFTAG_MAKE = 271; /*  scanner manufacturer name */
		public const int TIFFTAG_MODEL = 272; /*  scanner model name/number */
		public const int TIFFTAG_STRIPOFFSETS = 273; /*  offsets to data strips */
		public const int TIFFTAG_ORIENTATION = 274; /*  +image orientation */
		public const int ORIENTATION_TOPLEFT = 1; /*  row 0 top, col 0 lhs */
		public const int ORIENTATION_TOPRIGHT = 2; /*  row 0 top, col 0 rhs */
		public const int ORIENTATION_BOTRIGHT = 3; /*  row 0 bottom, col 0 rhs */
		public const int ORIENTATION_BOTLEFT = 4; /*  row 0 bottom, col 0 lhs */
		public const int ORIENTATION_LEFTTOP = 5; /*  row 0 lhs, col 0 top */
		public const int ORIENTATION_RIGHTTOP = 6; /*  row 0 rhs, col 0 top */
		public const int ORIENTATION_RIGHTBOT = 7; /*  row 0 rhs, col 0 bottom */
		public const int ORIENTATION_LEFTBOT = 8; /*  row 0 lhs, col 0 bottom */
		public const int TIFFTAG_SAMPLESPERPIXEL = 277; /*  samples per pixel */
		public const int TIFFTAG_ROWSPERSTRIP = 278; /*  rows per strip of data */
		public const int TIFFTAG_STRIPBYTECOUNTS = 279; /*  bytes counts for strips */
		public const int TIFFTAG_MINSAMPLEVALUE = 280; /*  +minimum sample value */
		public const int TIFFTAG_MAXSAMPLEVALUE = 281; /*  +maximum sample value */
		public const int TIFFTAG_XRESOLUTION = 282; /*  pixels/resolution in x */
		public const int TIFFTAG_YRESOLUTION = 283; /*  pixels/resolution in y */
		public const int TIFFTAG_PLANARCONFIG = 284; /*  storage organization */
		public const int PLANARCONFIG_CONTIG = 1; /*  single image plane */
		public const int PLANARCONFIG_SEPARATE = 2; /*  separate planes of data */
		public const int TIFFTAG_PAGENAME = 285; /*  page name image is from */
		public const int TIFFTAG_XPOSITION = 286; /*  x page offset of image lhs */
		public const int TIFFTAG_YPOSITION = 287; /*  y page offset of image lhs */
		public const int TIFFTAG_FREEOFFSETS = 288; /*  +byte offset to free block */
		public const int TIFFTAG_FREEBYTECOUNTS = 289; /*  +sizes of free blocks */
		public const int TIFFTAG_GRAYRESPONSEUNIT = 290; /*  $gray scale curve accuracy */
		public const int GRAYRESPONSEUNIT_10S = 1; /*  tenths of a unit */
		public const int GRAYRESPONSEUNIT_100S = 2; /*  hundredths of a unit */
		public const int GRAYRESPONSEUNIT_1000S = 3; /*  thousandths of a unit */
		public const int GRAYRESPONSEUNIT_10000S = 4; /*  ten-thousandths of a unit */
		public const int GRAYRESPONSEUNIT_100000S = 5; /*  hundred-thousandths */
		public const int TIFFTAG_GRAYRESPONSECURVE = 291; /*  $gray scale response curve */
		public const int TIFFTAG_GROUP3OPTIONS = 292; /*  32 flag bits */
		public const int TIFFTAG_T4OPTIONS = 292; /*  TIFF 6.0 proper name alias */
		public const int GROUP3OPT_2DENCODING = 0x1; /*  2-dimensional coding */
		public const int GROUP3OPT_UNCOMPRESSED = 0x2; /*  data not compressed */
		public const int GROUP3OPT_FILLBITS = 0x4; /*  fill to byte boundary */
		public const int TIFFTAG_GROUP4OPTIONS = 293; /*  32 flag bits */
		public const int TIFFTAG_T6OPTIONS = 293; /*  TIFF 6.0 proper name */
		public const int GROUP4OPT_UNCOMPRESSED = 0x2; /*  data not compressed */
		public const int TIFFTAG_RESOLUTIONUNIT = 296; /*  units of resolutions */
		public const int RESUNIT_NONE = 1; /*  no meaningful units */
		public const int RESUNIT_INCH = 2; /*  english */
		public const int RESUNIT_CENTIMETER = 3; /*  metric */
		public const int TIFFTAG_PAGENUMBER = 297; /*  page numbers of multi-page */
		public const int TIFFTAG_COLORRESPONSEUNIT = 300; /*  $color curve accuracy */
		public const int COLORRESPONSEUNIT_10S = 1; /*  tenths of a unit */
		public const int COLORRESPONSEUNIT_100S = 2; /*  hundredths of a unit */
		public const int COLORRESPONSEUNIT_1000S = 3; /*  thousandths of a unit */
		public const int COLORRESPONSEUNIT_10000S = 4; /*  ten-thousandths of a unit */
		public const int COLORRESPONSEUNIT_100000S = 5; /*  hundred-thousandths */
		public const int TIFFTAG_TRANSFERFUNCTION = 301; /*  !colorimetry info */
		public const int TIFFTAG_SOFTWARE = 305; /*  name & release */
		public const int TIFFTAG_DATETIME = 306; /*  creation date and time */
		public const int TIFFTAG_ARTIST = 315; /*  creator of image */
		public const int TIFFTAG_HOSTCOMPUTER = 316; /*  machine where created */
		public const int TIFFTAG_PREDICTOR = 317; /*  prediction scheme w/ LZW */
		public const int PREDICTOR_NONE = 1; /*  no prediction scheme used */
		public const int PREDICTOR_HORIZONTAL = 2; /*  horizontal differencing */
		public const int PREDICTOR_FLOATINGPOINT = 3; /*  floating point predictor */
		public const int TIFFTAG_WHITEPOINT = 318; /*  image white point */
		public const int TIFFTAG_PRIMARYCHROMATICITIES = 319; /*  !primary chromaticities */
		public const int TIFFTAG_COLORMAP = 320; /*  RGB map for pallette image */
		public const int TIFFTAG_HALFTONEHINTS = 321; /*  !highlight+shadow info */
		public const int TIFFTAG_TILEWIDTH = 322; /*  !tile width in pixels */
		public const int TIFFTAG_TILELENGTH = 323; /*  !tile height in pixels */
		public const int TIFFTAG_TILEOFFSETS = 324; /*  !offsets to data tiles */
		public const int TIFFTAG_TILEBYTECOUNTS = 325; /*  !byte counts for tiles */
		public const int TIFFTAG_BADFAXLINES = 326; /*  lines w/ wrong pixel count */
		public const int TIFFTAG_CLEANFAXDATA = 327; /*  regenerated line info */
		public const int CLEANFAXDATA_CLEAN = 0; /*  no errors detected */
		public const int CLEANFAXDATA_REGENERATED = 1; /*  receiver regenerated lines */
		public const int CLEANFAXDATA_UNCLEAN = 2; /*  uncorrected errors exist */
		public const int TIFFTAG_CONSECUTIVEBADFAXLINES = 328; /*  max consecutive bad lines */
		public const int TIFFTAG_SUBIFD = 330; /*  subimage descriptors */
		public const int TIFFTAG_INKSET = 332; /*  !inks in separated image */
		public const int INKSET_CMYK = 1; /*  !cyan-magenta-yellow-black color */
		public const int INKSET_MULTIINK = 2; /*  !multi-ink or hi-fi color */
		public const int TIFFTAG_INKNAMES = 333; /*  !ascii names of inks */
		public const int TIFFTAG_NUMBEROFINKS = 334; /*  !number of inks */
		public const int TIFFTAG_DOTRANGE = 336; /*  !0% and 100% dot codes */
		public const int TIFFTAG_TARGETPRINTER = 337; /*  !separation target */
		public const int TIFFTAG_EXTRASAMPLES = 338; /*  !info about extra samples */
		public const int EXTRASAMPLE_UNSPECIFIED = 0; /*  !unspecified data */
		public const int EXTRASAMPLE_ASSOCALPHA = 1; /*  !associated alpha data */
		public const int EXTRASAMPLE_UNASSALPHA = 2; /*  !unassociated alpha data */
		public const int TIFFTAG_SAMPLEFORMAT = 339; /*  !data sample format */
		public const int SAMPLEFORMAT_UINT = 1; /*  !unsigned integer data */
		public const int SAMPLEFORMAT_INT = 2; /*  !signed integer data */
		public const int SAMPLEFORMAT_IEEEFP = 3; /*  !IEEE floating point data */
		public const int SAMPLEFORMAT_VOID = 4; /*  !untyped data */
		public const int SAMPLEFORMAT_COMPLEXINT = 5; /*  !complex signed int */
		public const int SAMPLEFORMAT_COMPLEXIEEEFP = 6; /*  !complex ieee floating */
		public const int TIFFTAG_SMINSAMPLEVALUE = 340; /*  !variable MinSampleValue */
		public const int TIFFTAG_SMAXSAMPLEVALUE = 341; /*  !variable MaxSampleValue */
		public const int TIFFTAG_CLIPPATH = 343; /*  %ClipPath
						   [Adobe TIFF technote 2] */
		public const int TIFFTAG_XCLIPPATHUNITS = 344; /*  %XClipPathUnits
						   [Adobe TIFF technote 2] */
		public const int TIFFTAG_YCLIPPATHUNITS = 345; /*  %YClipPathUnits
						   [Adobe TIFF technote 2] */
		public const int TIFFTAG_INDEXED = 346; /*  %Indexed
						   [Adobe TIFF Technote 3] */
		public const int TIFFTAG_JPEGTABLES = 347; /*  %JPEG table stream */
		public const int TIFFTAG_OPIPROXY = 351; /*  %OPI Proxy [Adobe TIFF technote] */
		/*
		 * Tags 512-521 are obsoleted by Technical Note #2 which specifies a
		 * revised JPEG-in-TIFF scheme.
		 */
		public const int TIFFTAG_JPEGPROC = 512; /*  !JPEG processing algorithm */
		public const int JPEGPROC_BASELINE = 1; /*  !baseline sequential */
		public const int JPEGPROC_LOSSLESS = 14; /*  !Huffman coded lossless */
		public const int TIFFTAG_JPEGIFOFFSET = 513; /*  !pointer to SOI marker */
		public const int TIFFTAG_JPEGIFBYTECOUNT = 514; /*  !JFIF stream length */
		public const int TIFFTAG_JPEGRESTARTINTERVAL = 515; /*  !restart interval length */
		public const int TIFFTAG_JPEGLOSSLESSPREDICTORS = 517; /*  !lossless proc predictor */
		public const int TIFFTAG_JPEGPOINTTRANSFORM = 518; /*  !lossless point transform */
		public const int TIFFTAG_JPEGQTABLES = 519; /*  !Q matrice offsets */
		public const int TIFFTAG_JPEGDCTABLES = 520; /*  !DCT table offsets */
		public const int TIFFTAG_JPEGACTABLES = 521; /*  !AC coefficient offsets */
		public const int TIFFTAG_YCBCRCOEFFICIENTS = 529; /*  !RGB -> YCbCr transform */
		public const int TIFFTAG_YCBCRSUBSAMPLING = 530; /*  !YCbCr subsampling factors */
		public const int TIFFTAG_YCBCRPOSITIONING = 531; /*  !subsample positioning */
		public const int YCBCRPOSITION_CENTERED = 1; /*  !as in PostScript Level 2 */
		public const int YCBCRPOSITION_COSITED = 2; /*  !as in CCIR 601-1 */
		public const int TIFFTAG_REFERENCEBLACKWHITE = 532; /*  !colorimetry info */
		public const int TIFFTAG_XMLPACKET = 700; /*  %XML packet
						   [Adobe XMP Specification,
						   January 2004 */
		public const int TIFFTAG_OPIIMAGEID = 32781; /*  %OPI ImageID
						   [Adobe TIFF technote] */
		/* tags 32952-32956 are private tags registered to Island Graphics */
		public const int TIFFTAG_REFPTS = 32953; /*  image reference points */
		public const int TIFFTAG_REGIONTACKPOINT = 32954; /*  region-xform tack point */
		public const int TIFFTAG_REGIONWARPCORNERS = 32955; /*  warp quadrilateral */
		public const int TIFFTAG_REGIONAFFINE = 32956; /*  affine transformation mat */
		/*  tags 32995-32999 are private tags registered to SGI */
		public const int TIFFTAG_MATTEING = 32995; /*  $use ExtraSamples */
		public const int TIFFTAG_DATATYPE = 32996; /*  $use SampleFormat */
		public const int TIFFTAG_IMAGEDEPTH = 32997; /*  z depth of image */
		public const int TIFFTAG_TILEDEPTH = 32998; /*  z depth/data tile */
		/* tags 33300-33309 are private tags registered to Pixar */
		/*
		 * TIFFTAG_PIXAR_IMAGEFULLWIDTH and TIFFTAG_PIXAR_IMAGEFULLLENGTH
		 * are set when an image has been cropped out of a larger image.  
		 * They reflect the size of the original uncropped image.
		 * The TIFFTAG_XPOSITION and TIFFTAG_YPOSITION can be used
		 * to determine the position of the smaller image in the larger one.
		 */
		public const int TIFFTAG_PIXAR_IMAGEFULLWIDTH = 33300; /*  full image size in x */
		public const int TIFFTAG_PIXAR_IMAGEFULLLENGTH = 33301; /*  full image size in y */
		/*  Tags 33302-33306 are used to identify special image modes and data
		* used by Pixar's texture formats.
		*/
		public const int TIFFTAG_PIXAR_TEXTUREFORMAT = 33302; /*  texture map format */
		public const int TIFFTAG_PIXAR_WRAPMODES = 33303; /*  s & t wrap modes */
		public const int TIFFTAG_PIXAR_FOVCOT = 33304; /*  cotan(fov) for env. maps */
		public const int TIFFTAG_PIXAR_MATRIX_WORLDTOSCREEN = 33305;
		public const int TIFFTAG_PIXAR_MATRIX_WORLDTOCAMERA = 33306;
		/*  tag 33405 is a private tag registered to Eastman Kodak */
		public const int TIFFTAG_WRITERSERIALNUMBER = 33405; /*  device serial number */
		/*  tag 33432 is listed in the 6.0 spec w/ unknown ownership */
		public const int TIFFTAG_COPYRIGHT = 33432; /*  copyright string */
		/*  IPTC TAG from RichTIFF specifications */
		public const int TIFFTAG_RICHTIFFIPTC = 33723;
		/*  34016-34029 are reserved for ANSI IT8 TIFF/IT <dkelly@apago.com) */
		public const int TIFFTAG_IT8SITE = 34016; /*  site name */
		public const int TIFFTAG_IT8COLORSEQUENCE = 34017; /*  color seq. [RGB,CMYK,etc] */
		public const int TIFFTAG_IT8HEADER = 34018; /*  DDES Header */
		public const int TIFFTAG_IT8RASTERPADDING = 34019; /*  raster scanline padding */
		public const int TIFFTAG_IT8BITSPERRUNLENGTH = 34020; /*  # of bits in short run */
		public const int TIFFTAG_IT8BITSPEREXTENDEDRUNLENGTH = 34021; /*  # of bits in long run */
		public const int TIFFTAG_IT8COLORTABLE = 34022; /*  LW colortable */
		public const int TIFFTAG_IT8IMAGECOLORINDICATOR = 34023; /*  BP/BL image color switch */
		public const int TIFFTAG_IT8BKGCOLORINDICATOR = 34024; /*  BP/BL bg color switch */
		public const int TIFFTAG_IT8IMAGECOLORVALUE = 34025; /*  BP/BL image color value */
		public const int TIFFTAG_IT8BKGCOLORVALUE = 34026; /*  BP/BL bg color value */
		public const int TIFFTAG_IT8PIXELINTENSITYRANGE = 34027; /*  MP pixel intensity value */
		public const int TIFFTAG_IT8TRANSPARENCYINDICATOR = 34028; /*  HC transparency switch */
		public const int TIFFTAG_IT8COLORCHARACTERIZATION = 34029; /*  color character. table */
		public const int TIFFTAG_IT8HCUSAGE = 34030; /*  HC usage indicator */
		public const int TIFFTAG_IT8TRAPINDICATOR = 34031; /*  Trapping indicator
						   (untrapped=0, trapped=1) */
		public const int TIFFTAG_IT8CMYKEQUIVALENT = 34032; /*  CMYK color equivalents */
		/*  tags 34232-34236 are private tags registered to Texas Instruments */
		public const int TIFFTAG_FRAMECOUNT = 34232; /*  Sequence Frame Count */
		/*  tag 34377 is private tag registered to Adobe for PhotoShop */
		public const int TIFFTAG_PHOTOSHOP = 34377;
		/*  tags 34665, 34853 and 40965 are documented in EXIF specification */
		public const int TIFFTAG_EXIFIFD = 34665; /*  Pointer to EXIF private directory */
		/*  tag 34750 is a private tag registered to Adobe? */
		public const int TIFFTAG_ICCPROFILE = 34675; /*  ICC profile data */
		/*  tag 34750 is a private tag registered to Pixel Magic */
		public const int TIFFTAG_JBIGOPTIONS = 34750; /*  JBIG options */
		public const int TIFFTAG_GPSIFD = 34853; /*  Pointer to GPS private directory */
		/*  tags 34908-34914 are private tags registered to SGI */
		public const int TIFFTAG_FAXRECVPARAMS = 34908; /*  encoded Class 2 ses. parms */
		public const int TIFFTAG_FAXSUBADDRESS = 34909; /*  received SubAddr string */
		public const int TIFFTAG_FAXRECVTIME = 34910; /*  receive time (secs) */
		public const int TIFFTAG_FAXDCS = 34911; /*  encoded fax ses. params, Table 2/T.30 */
		/*  tags 37439-37443 are registered to SGI <gregl@sgi.com> */
		public const int TIFFTAG_STONITS = 37439; /*  Sample value to Nits */
		/*  tag 34929 is a private tag registered to FedEx */
		public const int TIFFTAG_FEDEX_EDR = 34929; /*  unknown use */
		public const int TIFFTAG_INTEROPERABILITYIFD = 40965; /*  Pointer to Interoperability private directory */
		/*  Adobe Digital Negative (DNG) format tags */
		public const int TIFFTAG_DNGVERSION = 50706; /*  &DNG version number */
		public const int TIFFTAG_DNGBACKWARDVERSION = 50707; /*  &DNG compatibility version */
		public const int TIFFTAG_UNIQUECAMERAMODEL = 50708; /*  &name for the camera model */
		public const int TIFFTAG_LOCALIZEDCAMERAMODEL = 50709; /*  &localized camera model
						   name */
		public const int TIFFTAG_CFAPLANECOLOR = 50710; /*  &CFAPattern->LinearRaw space
						   mapping */
		public const int TIFFTAG_CFALAYOUT = 50711; /*  &spatial layout of the CFA */
		public const int TIFFTAG_LINEARIZATIONTABLE = 50712; /*  &lookup table description */
		public const int TIFFTAG_BLACKLEVELREPEATDIM = 50713; /*  &repeat pattern size for
						   the BlackLevel tag */
		public const int TIFFTAG_BLACKLEVEL = 50714; /*  &zero light encoding level */
		public const int TIFFTAG_BLACKLEVELDELTAH = 50715; /*  &zero light encoding level
						   differences (columns) */
		public const int TIFFTAG_BLACKLEVELDELTAV = 50716; /*  &zero light encoding level
						   differences (rows) */
		public const int TIFFTAG_WHITELEVEL = 50717; /*  &fully saturated encoding
						   level */
		public const int TIFFTAG_DEFAULTSCALE = 50718; /*  &default scale factors */
		public const int TIFFTAG_DEFAULTCROPORIGIN = 50719; /*  &origin of the final image
						   area */
		public const int TIFFTAG_DEFAULTCROPSIZE = 50720; /*  &size of the final image 
						   area */
		public const int TIFFTAG_COLORMATRIX1 = 50721; /*  &XYZ->reference color space
						   transformation matrix 1 */
		public const int TIFFTAG_COLORMATRIX2 = 50722; /*  &XYZ->reference color space
						   transformation matrix 2 */
		public const int TIFFTAG_CAMERACALIBRATION1 = 50723; /*  &calibration matrix 1 */
		public const int TIFFTAG_CAMERACALIBRATION2 = 50724; /*  &calibration matrix 2 */
		public const int TIFFTAG_REDUCTIONMATRIX1 = 50725; /*  &dimensionality reduction
						   matrix 1 */
		public const int TIFFTAG_REDUCTIONMATRIX2 = 50726; /*  &dimensionality reduction
						   matrix 2 */
		public const int TIFFTAG_ANALOGBALANCE = 50727; /*  &gain applied the stored raw
						   values*/
		public const int TIFFTAG_ASSHOTNEUTRAL = 50728; /*  &selected white balance in
						   linear reference space */
		public const int TIFFTAG_ASSHOTWHITEXY = 50729; /*  &selected white balance in
						   x-y chromaticity
						   coordinates */
		public const int TIFFTAG_BASELINEEXPOSURE = 50730; /*  &how much to move the zero
						   point */
		public const int TIFFTAG_BASELINENOISE = 50731; /*  &relative noise level */
		public const int TIFFTAG_BASELINESHARPNESS = 50732; /*  &relative amount of
						   sharpening */
		public const int TIFFTAG_BAYERGREENSPLIT = 50733; /*  &how closely the values of
						   the green pixels in the
						   blue/green rows track the
						   values of the green pixels
						   in the red/green rows */
		public const int TIFFTAG_LINEARRESPONSELIMIT = 50734; /*  &non-linear encoding range */
		public const int TIFFTAG_CAMERASERIALNUMBER = 50735; /*  &camera's serial number */
		public const int TIFFTAG_LENSINFO = 50736; /*  info about the lens */
		public const int TIFFTAG_CHROMABLURRADIUS = 50737; /*  &chroma blur radius */
		public const int TIFFTAG_ANTIALIASSTRENGTH = 50738; /*  &relative strength of the
						   camera's anti-alias filter */
		public const int TIFFTAG_SHADOWSCALE = 50739; /*  &used by Adobe Camera Raw */
		public const int TIFFTAG_DNGPRIVATEDATA = 50740; /*  &manufacturer's private data */
		public const int TIFFTAG_MAKERNOTESAFETY = 50741; /*  &whether the EXIF MakerNote
						   tag is safe to preserve
						   along with the rest of the
						   EXIF data */
		public const int TIFFTAG_CALIBRATIONILLUMINANT1 = 50778; /*  &illuminant 1 */
		public const int TIFFTAG_CALIBRATIONILLUMINANT2 = 50779; /*  &illuminant 2 */
		public const int TIFFTAG_BESTQUALITYSCALE = 50780; /*  &best quality multiplier */
		public const int TIFFTAG_RAWDATAUNIQUEID = 50781; /*  &unique identifier for
						   the raw image data */
		public const int TIFFTAG_ORIGINALRAWFILENAME = 50827; /*  &file name of the original
						   raw file */
		public const int TIFFTAG_ORIGINALRAWFILEDATA = 50828; /*  &contents of the original
						   raw file */
		public const int TIFFTAG_ACTIVEAREA = 50829; /*  &active (non-masked) pixels
						   of the sensor */
		public const int TIFFTAG_MASKEDAREAS = 50830; /*  &list of coordinates
						   of fully masked pixels */
		public const int TIFFTAG_ASSHOTICCPROFILE = 50831; /*  &these two tags used to */
		public const int TIFFTAG_ASSHOTPREPROFILEMATRIX = 50832; /*  map cameras's color space
						   into ICC profile space */
		public const int TIFFTAG_CURRENTICCPROFILE = 50833; /*  & */
		public const int TIFFTAG_CURRENTPREPROFILEMATRIX = 50834; /*  & */
		/*  tag 65535 is an undefined tag used by Eastman Kodak */
		public const int TIFFTAG_DCSHUESHIFTVALUES = 65535; /*  hue shift correction data */

		/* 
		* The following are ``pseudo tags'' that can be used to control
		* codec-specific functionality.  These tags are not written to file.
		* Note that these values start at 0xffff+1 so that they'll never
		* collide with Aldus-assigned tags.
		*
		* If you want your private pseudo tags ``registered'' (i.e. added to
		* this file), please post a bug report via the tracking system at
		* http://www.remotesensing.org/libtiff/bugs.html with the appropriate
		* C definitions to add.
		*/
		public const int TIFFTAG_FAXMODE = 65536; /*  Group 3/4 format control */
		public const int FAXMODE_CLASSIC = 0x0000; /*  default, include RTC */
		public const int FAXMODE_NORTC = 0x0001; /*  no RTC at end of data */
		public const int FAXMODE_NOEOL = 0x0002; /*  no EOL code at end of row */
		public const int FAXMODE_BYTEALIGN = 0x0004; /*  byte align row */
		public const int FAXMODE_WORDALIGN = 0x0008; /*  word align row */
		public const int FAXMODE_CLASSF = FAXMODE_NORTC; /*  TIFF Class F */
		/// <summary>
		/// Note: quality level is on the IJG 0-100 scale.  Default value is 75 
		/// </summary>
		public const int TIFFTAG_JPEGQUALITY = 65537; /*  Compression quality level */
		public const int TIFFTAG_JPEGCOLORMODE = 65538; /*  Auto RGB<=>YCbCr convert? */
		public const int JPEGCOLORMODE_RAW = 0x0000; /*  no conversion (default) */
		public const int JPEGCOLORMODE_RGB = 0x0001; /*  do auto conversion */
		/// <summary>
		/// Note: default is JPEGTABLESMODE_QUANT | JPEGTABLESMODE_HUFF 
		/// </summary>
		public const int TIFFTAG_JPEGTABLESMODE = 65539; /*  What to put in JPEGTables */
		public const int JPEGTABLESMODE_QUANT = 0x0001; /*  include quantization tbls */
		public const int JPEGTABLESMODE_HUFF = 0x0002; /*  include Huffman tbls */
		public const int TIFFTAG_FAXFILLFUNC = 65540; /*  G3/G4 fill function */
		public const int TIFFTAG_PIXARLOGDATAFMT = 65549; /*  PixarLogCodec I/O data sz */
		public const int PIXARLOGDATAFMT_8BIT = 0; /*  regular u_char samples */
		public const int PIXARLOGDATAFMT_8BITABGR = 1; /*  ABGR-order u_chars */
		public const int PIXARLOGDATAFMT_11BITLOG = 2; /*  11-bit log-encoded (raw) */
		public const int PIXARLOGDATAFMT_12BITPICIO = 3; /*  as per PICIO (1.0==2048) */
		public const int PIXARLOGDATAFMT_16BIT = 4; /*  signed short samples */
		public const int PIXARLOGDATAFMT_FLOAT = 5; /*  IEEE float samples */
		/*  65550-65556 are allocated to Oceana Matrix <dev@oceana.com> */
		public const int TIFFTAG_DCSIMAGERTYPE = 65550; /*  imager model & filter */
		public const int DCSIMAGERMODEL_M3 = 0; /*  M3 chip (1280 x 1024) */
		public const int DCSIMAGERMODEL_M5 = 1; /*  M5 chip (1536 x 1024) */
		public const int DCSIMAGERMODEL_M6 = 2; /*  M6 chip (3072 x 2048) */
		public const int DCSIMAGERFILTER_IR = 0; /*  infrared filter */
		public const int DCSIMAGERFILTER_MONO = 1; /*  monochrome filter */
		public const int DCSIMAGERFILTER_CFA = 2; /*  color filter array */
		public const int DCSIMAGERFILTER_OTHER = 3; /*  other filter */
		public const int TIFFTAG_DCSINTERPMODE = 65551; /*  interpolation mode */
		public const int DCSINTERPMODE_NORMAL = 0x0; /*  whole image, default */
		public const int DCSINTERPMODE_PREVIEW = 0x1; /*  preview of image (384x256) */
		public const int TIFFTAG_DCSBALANCEARRAY = 65552; /*  color balance values */
		public const int TIFFTAG_DCSCORRECTMATRIX = 65553; /*  color correction values */
		public const int TIFFTAG_DCSGAMMA = 65554; /*  gamma value */
		public const int TIFFTAG_DCSTOESHOULDERPTS = 65555; /*  toe & shoulder points */
		public const int TIFFTAG_DCSCALIBRATIONFD = 65556; /*  calibration file desc */
		/*  Note: quality level is on the ZLIB 1-9 scale. Default value is -1 */
		public const int TIFFTAG_ZIPQUALITY = 65557; /*  compression quality level */
		public const int TIFFTAG_PIXARLOGQUALITY = 65558; /*  PixarLog uses same scale */
		/*  65559 is allocated to Oceana Matrix <dev@oceana.com> */
		public const int TIFFTAG_DCSCLIPRECTANGLE = 65559; /*  area of image to acquire */
		public const int TIFFTAG_SGILOGDATAFMT = 65560; /*  SGILog user data format */
		public const int SGILOGDATAFMT_FLOAT = 0; /*  IEEE float samples */
		public const int SGILOGDATAFMT_16BIT = 1; /*  16-bit samples */
		public const int SGILOGDATAFMT_RAW = 2; /*  uninterpreted data */
		public const int SGILOGDATAFMT_8BIT = 3; /*  8-bit RGB monitor values */
		public const int TIFFTAG_SGILOGENCODE = 65561; /*  SGILog data encoding control*/

		public static LibTiffHelper.TIFFErrorHandler errDelegate;

		public static object GetRegistryValue(string name)
		{
			Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey("Software\\Kesco");
			return key.GetValue(name);
		}

		public LibTiffHelper()
		{
			if(IntPtr.Size == 8)
				extLT = new LibTiffHelperx64();
			else
				extLT = new LibTiffHelperx32();
			lockobject = new object();
			//try
			//{
			//    errDelegate = new LibTiffHelper.TIFFErrorHandler(TIFFErrorHandlerCallback);
			//    LibTiffHelper.TIFFErrorHandler ed = extLT.TIFSetErrorHandler(errDelegate);
			//}
			//catch { }
		}

		public void _XTIFFInitialize()
		{
			if(!first_time)
				return; /* Been there. Done that. */
			first_time = false;

			/* Grab the inherited method and install */
			TIFFExtendProc _XTIFFDirectory = new TIFFExtendProc(_XTIFFDefaultDirectory);
			_ParentExtender = extLT.TIFSetTagExtender(_XTIFFDirectory);

		}

		public void _XTIFFDefaultDirectory(IntPtr thandle_t)
		{
			/* Install the extended Tag field info */
			IntPtr ptrTag = extLT.TIFFindField(thandle_t, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
			if(ptrTag == IntPtr.Zero)
				AddAnnotationTag(thandle_t);


			/* Since an XTIFF client module may have overridden
			 * the default directory method, we call it now to
			 * allow it to set up the rest of its own methods.
			 */

			if(_ParentExtender != null)
				_ParentExtender(thandle_t);
		}


		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct TIFFFieldInfo
		{
			public int field_tag;		/* field's tag */
			public short field_readcount;	/* read count/TIFF_VARIABLE/TIFF_SPP */
			public short field_writecount;	/* write count/TIFF_VARIABLE */
			[MarshalAs(UnmanagedType.I4)]
			public TIFFDataType field_type;	/* type of associated data */
			public ushort field_bit;	/* bit in fieldsset bit vector */
			public byte field_oktochange;	/* if true, can change while writing */
			public byte field_passcount;	/* if true, pass dir count on set */
			[MarshalAs(UnmanagedType.BStr, SizeConst = 32)]
			public String field_name;
		}

		public enum TIFFDataType : int
		{
			TIFF_NOTYPE = 0,	/* placeholder */
			TIFF_BYTE = 1,	/* 8-bit unsigned integer */
			TIFF_ASCII = 2,	/* 8-bit bytes w/ last byte null */
			TIFF_SHORT = 3,	/* 16-bit unsigned integer */
			TIFF_LONG = 4,	/* 32-bit unsigned integer */
			TIFF_RATIONAL = 5,	/* 64-bit unsigned fraction */
			TIFF_SBYTE = 6,	/* !8-bit signed integer */
			TIFF_UNDEFINED = 7,	/* !8-bit untyped data */
			TIFF_SSHORT = 8,	/* !16-bit signed integer */
			TIFF_SLONG = 9,	/* !32-bit signed integer */
			TIFF_SRATIONAL = 10,	/* !64-bit signed fraction */
			TIFF_FLOAT = 11,	/* !32-bit IEEE floating point */
			TIFF_DOUBLE = 12,	/* !64-bit IEEE floating point */
			TIFF_IFD = 13	/* %32-bit unsigned integer (offset) */
		}

		/// <summary>
		/// Добваление тега заметок в текущую директорию
		/// </summary>
		public void AddAnnotationTag(IntPtr thandle_t)
		{
			TIFFFieldInfo tiffAnn = new TIFFFieldInfo();
			tiffAnn.field_tag = TIFFTAG_ANNOTATION;
			tiffAnn.field_readcount = -3;
			tiffAnn.field_writecount = -3;
			tiffAnn.field_type = TIFFDataType.TIFF_BYTE;
			tiffAnn.field_bit = 65;
			tiffAnn.field_oktochange = 1;
			tiffAnn.field_passcount = 1;
			//tiffAnn.field_name = "                      Tag 32932" + '\0'; 
			//tiffAnn.field_name = "Tag 32932" + '\0'; 
			///для записи лучше так
			//IntPtr ptrStr = Marshal.StringToHGlobalAnsi("Tag 32932" + '\0');
			tiffAnn.field_name = "Tag 32932" + '\0';

			TIFFFieldInfo[] xtiffFieldInfo = new TIFFFieldInfo[1] { tiffAnn };
			extLT.TIFMergeFieldInfo(thandle_t, xtiffFieldInfo, 1);
			//Наверное освобождать память тут не стоит
			//Marshal.FreeHGlobal(ptrStr);

		}


		public static void TIFFErrorHandlerCallback(string module, string fmt, IntPtr ap)
		{
			string r = null;
			bool isUni = false;
			if(fmt != null)
				isUni = fmt.Contains("%S");
			IntPtr[] ar = new IntPtr[1];
			Marshal.Copy(ap, ar, 0, 1);
			if(isUni)
				r = Marshal.PtrToStringUni(ar[0]);
			else
				r = Marshal.PtrToStringAnsi(ar[0]);

			WriteToLog(module + "\n" + fmt + (!string.IsNullOrEmpty(r) ? System.Environment.NewLine + r : ""));
		}

		/// <summary>
		/// Хендл открытого файла. За закрытием должен следить владелец экземпляра
		/// </summary>
		private IntPtr openTif = IntPtr.Zero;

		public IntPtr TiffHandle
		{
			get { return openTif; }
			set { openTif = value; }
		}

		public LibTiffHelperxxx ExtLT
		{
			get
			{
				if(extLT == null)
					if(IntPtr.Size == 8)
						extLT = new LibTiffHelperx64();
					else
						extLT = new LibTiffHelperx32();
				return extLT;
			}
		}

		public int GetCountPages(string fileName)
		{
			int n = 0;
			Bitmap bmp = null;
			lock(lockobject)
			{
				IntPtr tif = TiffOpenRead(ref fileName, out bmp, false);
				try
				{
					if(tif != IntPtr.Zero)
					{
						n = extLT.TIFNumberOfDirectories(tif);
						if(n == 0)
							n = 1;
						extLT.TIFSetDirectory(tif, (ushort)(n - 1));
						int i = extLT.TIFLastDirectory(tif);
						if(i == 0)
							++n;
					}
					else if(tif == IntPtr.Zero && bmp != null)
					{
						bmp.Dispose();
						n = -1;
					}
				}
				finally
				{
					if(tif != IntPtr.Zero)
						extLT.TIFClose(tif);
					tif = IntPtr.Zero;
				}
			}
			return n;
		}

		/// <summary>
		/// Закрытие тифа для TiffOpenRead
		/// </summary>
		public void TiffCloseRead(ref IntPtr tif)
		{
			try
			{
				if(tif != IntPtr.Zero)
					extLT.TIFClose(tif);

				tif = IntPtr.Zero;
				if(openTif != IntPtr.Zero)
				{
					extLT.TIFClose(openTif);
					openTif = IntPtr.Zero;
				}
			}
			catch { openTif = IntPtr.Zero; }
		}

		/// <summary>
		/// Закрытие тифа для TiffOpenRead
		/// </summary>
		public void TiffCloseWrite(ref IntPtr tif)
		{
			if(tif != IntPtr.Zero)
				extLT.TIFClose(tif);
			tif = IntPtr.Zero;
		}

		public Bitmap ConvertToFullColorImage(Bitmap bmp)
		{
			int w = bmp.Width;
			int h = bmp.Height;
			Bitmap output;
			BitmapData bmpData, outputData;

			output = new Bitmap(w, h, PixelFormat.Format32bppArgb);
			bmpData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			outputData = output.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
			int size = (h * w) * 4;

			byte[] buf = new byte[size];
			Marshal.Copy(bmpData.Scan0, buf, 0, buf.Length);
			Marshal.Copy(buf, 0, outputData.Scan0, buf.Length);
			bmp.UnlockBits(bmpData);
			output.UnlockBits(outputData);
			return output;
		}

		/// <summary>
		/// Конвертер в 8bpp
		/// </summary>
		public Bitmap ConvertToGrayscale(Bitmap bmp)
		{
			int w = bmp.Width,
				h = bmp.Height,
				r, ic, oc, bmpStride, outputStride, bytesPerPixel;
			PixelFormat pfIn = bmp.PixelFormat;
			ColorPalette palette;
			Bitmap output;
			BitmapData bmpData, outputData;

			output = new Bitmap(w, h, PixelFormat.Format8bppIndexed);

			palette = output.Palette;
			for(int i = 0; i < 256; i++)
			{
				Color tmp = Color.FromArgb(255, i, i, i);
				palette.Entries[i] = Color.FromArgb(255, i, i, i);
			}
			output.Palette = palette;
			byte[] outputBytes;
			if(pfIn == PixelFormat.Format8bppIndexed)
			{
				Hashtable ht = new Hashtable();
				for(int p = 0; p < bmp.Palette.Entries.Length; ++p)
				{
					ht.Add((byte)p, (byte)((bmp.Palette.Entries[p].R + bmp.Palette.Entries[p].B + bmp.Palette.Entries[p].G) / 3));
				}
				output = (Bitmap)bmp.Clone();
				outputData = output.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
				outputStride = outputData.Stride;
				outputBytes = new byte[outputStride * h];
				Marshal.Copy(outputData.Scan0, outputBytes, 0, outputBytes.Length);
				for(int k = 0; k < outputBytes.Length; k++)
					outputBytes[k] = (byte)ht[outputBytes[k]];

				Marshal.Copy(outputBytes, 0, outputData.Scan0, outputBytes.Length);
				output.UnlockBits(outputData);
				output.Palette = palette;
				return output;
			}
			switch(pfIn)
			{
				case PixelFormat.Format1bppIndexed:
					bytesPerPixel = 1;
					break;
				case PixelFormat.Format24bppRgb:
					bytesPerPixel = 3;
					break;
				case PixelFormat.Format32bppArgb:
					bytesPerPixel = 4;
					break;
				case PixelFormat.Format32bppRgb:
					bytesPerPixel = 4;
					break;
				default:
					return null;
			}

			bmpData = bmp.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.ReadOnly,
				pfIn);
			outputData = output.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly,
				PixelFormat.Format8bppIndexed);
			bmpStride = bmpData.Stride;
			outputStride = outputData.Stride;

			IntPtr bmpPtr = bmpData.Scan0;
			IntPtr outputPtr = outputData.Scan0;
			byte[] bmpBytes = new byte[bmpStride * h];
			outputBytes = new byte[outputStride * h];
			Marshal.Copy(bmpPtr, bmpBytes, 0, bmpBytes.Length);

			if(bytesPerPixel == 3)
			{

				// L = .299*R + .587*G + .114*B
				for(r = 0; r < h; r++)
					for(ic = oc = 0; oc < w; ic += 3, ++oc)
						outputBytes[r * outputStride + oc] = (byte)(int)
							(0.299f * bmpBytes[r * bmpStride + ic] +
							0.587f * bmpBytes[r * bmpStride + ic + 1] +
							0.114f * bmpBytes[r * bmpStride + ic + 2]);
			}
			else if(bytesPerPixel == 1)
			{
				for(r = 0; r < h; r++)
					for(ic = oc = 0; oc < w; ++oc)
					{
						outputBytes[r * outputStride + oc] = (byte)(((bmpBytes[r * bmpStride + oc / 8] >> (7 - oc % 8)) & 1) * 255);
					}
			}
			else //bytesPerPixel == 4
			{
				// L = alpha * (.299*R + .587*G + .114*B)
				for(r = 0; r < h; r++)
					for(ic = oc = 0; oc < w; ic += 4, ++oc)
						outputBytes[r * outputStride + oc] = (byte)(int)
							((bmpBytes[r * bmpStride + ic] / 255.0f) *
							(0.299f * bmpBytes[r * bmpStride + ic + 1] +
							0.587f * bmpBytes[r * bmpStride + ic + 2] +
							0.114f * bmpBytes[r * bmpStride + ic + 3]));
			}
			Marshal.Copy(outputBytes, 0, outputPtr, outputBytes.Length);

			bmp.UnlockBits(bmpData);
			output.UnlockBits(outputData);

			return output;
		}

		/// <summary>
		/// Конвертер в 1bpp
		/// </summary>
		//public Bitmap ConvertToBitonalCalc(Bitmap original)
		//{
		//    BitmapSource bs = CreateBitmapSourceFromBitmap(original);
		//    FormatConvertedBitmap fcb = new FormatConvertedBitmap(bs, System.Windows.Media.PixelFormats.Indexed1, BitmapPalettes.BlackAndWhite, 1.0);
		//    byte[] ret = BitSourceToArray(fcb);
		//    using(MemoryStream ms = new MemoryStream(ret))
		//    {
		//        Bitmap destination = new Bitmap(ms);
		//        destination.SetResolution(original.HorizontalResolution, original.VerticalResolution);
		//        //BitmapData destinationData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);
		//        //Marshal.Copy(ret, 0, destinationData.Scan0, ret.Length);

		//        //destination.UnlockBits(destinationData);
		//        return destination;
		//    }

		//}

		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(IntPtr hObject);

		public static BitmapSource CreateBitmapSourceFromBitmap(Bitmap bitmap)
		{
			if(bitmap == null)
				throw new ArgumentNullException("bitmap");

			IntPtr hBitmap = bitmap.GetHbitmap();

			try
			{
				return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
					hBitmap,
					IntPtr.Zero,
					System.Windows.Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				DeleteObject(hBitmap);
			}
		}

		//public static byte[] BitSourceToArray(BitmapSource bitmapSource)
		//{
		//    BitmapEncoder encoder = new BmpBitmapEncoder();
		//    using(MemoryStream stream = new MemoryStream())
		//    {
		//        encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
		//        encoder.Save(stream);
		//        return stream.ToArray();
		//    }
		//}

		/// <summary>
		/// Конвертер в 1bpp
		/// </summary>
		public Bitmap ConvertToBitonal(Bitmap original)
		{
			return ConvertToBitonal(original, true);
		}

		public Bitmap ConvertToBitonal(Bitmap original, bool dispose)
		{

			//return original;
			Bitmap source = null;

			if(original.PixelFormat == PixelFormat.Format1bppIndexed)
				return original;
			if(original.PixelFormat != PixelFormat.Format32bppArgb)
			{
				source = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
				source.SetResolution(original.HorizontalResolution, original.VerticalResolution);
				using(Graphics g = Graphics.FromImage(source))
				{
					g.DrawImageUnscaled(original, 0, 0);
				}
			}
			else
			{
				source = original;
			}

			BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			int imageSize = sourceData.Stride * sourceData.Height;
			byte[] sourceBuffer = new byte[imageSize];
			Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, imageSize);

			source.UnlockBits(sourceData);

			Bitmap destination = new Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed);
			destination.SetResolution(source.HorizontalResolution, source.VerticalResolution);

			BitmapData destinationData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

			imageSize = destinationData.Stride * destinationData.Height;
			byte[] destinationBuffer = new byte[imageSize];

			int sourceIndex = 0;
			int destinationIndex = 0;
			int pixelTotal = 0;
			byte destinationValue = 0;
			int pixelValue = 128;
			int height = source.Height;
			int width = source.Width;
			int threshold = 500;

			for(int y = 0; y < height; y++)
			{
				sourceIndex = y * sourceData.Stride;
				destinationIndex = y * destinationData.Stride;
				destinationValue = 0;
				pixelValue = 128;

				for(int x = 0; x < width; x++)
				{
					pixelTotal = sourceBuffer[sourceIndex + 1] + sourceBuffer[sourceIndex + 2] + sourceBuffer[sourceIndex + 3];
					if(pixelTotal > threshold)
					{
						destinationValue += (byte)pixelValue;
					}
					if(pixelValue == 1)
					{
						destinationBuffer[destinationIndex] = destinationValue;
						destinationIndex++;
						destinationValue = 0;
						pixelValue = 128;
					}
					else
					{
						pixelValue >>= 1;
					}
					sourceIndex += 4;
				}
				if(pixelValue != 128)
				{
					destinationBuffer[destinationIndex] = destinationValue;
				}
			}

			Marshal.Copy(destinationBuffer, 0, destinationData.Scan0, imageSize);

			destination.UnlockBits(destinationData);

			if(source != original)
			{
				source.Dispose();
			}
			if(dispose)
			{
				original.Dispose();
				original = null;
			}
			if(destination.Height == 0 || destination.Width == 0)
			{
				WriteToLog("некорректые размеры при конвертации");
			}
			// Return
			return destination;
		}


		/// <summary>
		/// Конвертер в 1bpp
		/// </summary>
		public Bitmap ConvertrgbaToBitonal(Bitmap original)
		{
			//return original;
			Bitmap source = null;

			bool rgba = original.PixelFormat == PixelFormat.Format32bppArgb;
			if(original.PixelFormat != PixelFormat.Format32bppArgb)
			{
				source = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
				source.SetResolution(original.HorizontalResolution, original.VerticalResolution);
				using(Graphics g = Graphics.FromImage(source))
				{
					g.DrawImageUnscaled(original, 0, 0);
				}
			}
			else
			{
				source = original;
			}

			BitmapData sourceData = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);

			int imageSize = sourceData.Stride * sourceData.Height;
			byte[] sourceBuffer = new byte[imageSize];
			Marshal.Copy(sourceData.Scan0, sourceBuffer, 0, imageSize);

			source.UnlockBits(sourceData);

			Bitmap destination = new Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed);
			destination.SetResolution(source.HorizontalResolution, source.VerticalResolution);

			BitmapData destinationData = destination.LockBits(new Rectangle(0, 0, destination.Width, destination.Height), ImageLockMode.WriteOnly, PixelFormat.Format1bppIndexed);

			imageSize = destinationData.Stride * destinationData.Height;
			byte[] destinationBuffer = new byte[imageSize];

			int sourceIndex = 0;
			int destinationIndex = 0;
			int pixelTotal = 0;
			byte destinationValue = 0;
			int pixelValue = 128;
			int height = source.Height;
			int width = source.Width;
			int threshold = 500;

			for(int y = 0; y < height; y++)
			{
				sourceIndex = y * sourceData.Stride;
				destinationIndex = y * destinationData.Stride;
				destinationValue = 0;
				pixelValue = 128;

				for(int x = 0; x < width; x++)
				{
					if(rgba)
						pixelTotal = sourceBuffer[sourceIndex] + sourceBuffer[sourceIndex + 1] + sourceBuffer[sourceIndex + 2];
					else
						pixelTotal = sourceBuffer[sourceIndex + 1] + sourceBuffer[sourceIndex + 2] + sourceBuffer[sourceIndex + 3];
					if(pixelTotal > threshold)
					{
						destinationValue += (byte)pixelValue;
					}
					if(pixelValue == 1)
					{
						destinationBuffer[destinationIndex] = destinationValue;
						destinationIndex++;
						destinationValue = 0;
						pixelValue = 128;
					}
					else
					{
						pixelValue >>= 1;
					}
					sourceIndex += 4;
				}
				if(pixelValue != 128)
				{
					destinationBuffer[destinationIndex] = destinationValue;
				}
			}

			Marshal.Copy(destinationBuffer, 0, destinationData.Scan0, imageSize);

			destination.UnlockBits(destinationData);

			if(source != original)
			{
				source.Dispose();
			}

			original.Dispose();
			if(destination.Height == 0 || destination.Width == 0)
			{
				WriteToLog("некорректые размеры при конвертации");
			}
			// Return
			return destination;
		}

		public Bitmap ConvertGrayScaleToBitonal(Bitmap original)
		{
			return original;
		}

		/// <summary>
		/// Открывает файл с рисунком
		/// </summary>
		public Bitmap TryOpenFile(string fileName)
		{
			Bitmap bitmap = null;
			FileStream fs = null;
			MemoryStream ms = null;
			try
			{
				if(isUseLock)
				{
					bitmap = new Bitmap(fileName);
				}
				else
				{
					fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

					if(fs != null)
					{
						ms = new MemoryStream();

						int i;
						byte[] buf = new byte[4096];
						while((i = fs.Read(buf, 0, 4096)) > -1)
						{
							ms.Write(buf, 0, i);
							if(i < 4096)
								break;
						}
						ms.Position = 0;
                        fs.Close();
						bitmap = new Bitmap(ms);
					}
				}
			}
            catch (ArgumentException)
            {
                // Ошибка возникает вследствии открытия поврежденного файла.
                WriteToLog("Файл " + fileName + " поврежден или не является файлом изображения. (LibTiffHelper.TryOpenFile)");
                if (bitmap != null)
                {
                    bitmap.Dispose();
                    bitmap = null;
                }
            }
			catch(Exception ex)
			{
				WriteToLog(ex);
				if(bitmap != null)
				{
					bitmap.Dispose();
					bitmap = null;
				}
			}
			finally
			{
				if(ms != null)
				{
					ms.Close();
					ms.Dispose();
				}
				ms = null;
				if(fs != null)
				{
					fs.Close();
					fs.Dispose();
				}
				fs = null;
			}
			return bitmap;
		}

		/// <summary>
		/// Открывает файл с рисунком, конвертит и создает новый файл
		/// </summary>
		public bool TryOpenFileWithConvertAndCopy(ref string fileName)
		{
			bool res = false;
			if(!File.Exists(fileName))
				return false;
			Bitmap bitmap = TryOpenFile(fileName);
			if(bitmap != null)
			{
				bitmap = ConvertToBitonal(bitmap);
				if(bitmap != null)
				{
					String newFileName = fileName.Substring(0, fileName.Length - (fileName.Length - fileName.LastIndexOf('.'))) + ".tif";
					IntPtr tifw = extLT.TIFOpenW(newFileName, "w");
					if(IntPtr.Zero != tifw)
					{
						SetImageToTiff(tifw, bitmap, null);
						TiffCloseWrite(ref tifw);
					}
					fileName = newFileName;
					res = true;
				}
			}
			return res;
		}


		/// <summary>
		/// Открывает тиф для чтения. Если файл не открывается, следует вторая попытка открыть его, подразумевая,
		/// что это не тиф, а файл с неизвестным форматом. В этом случае, в завсисомсти от флага isCreateFileSecondOpen, 
		/// файл будет переконверчен(если удастся), сохранен как тиф, открыт, изменено имя файла и передан его хэндл, или открыт в битмап bitmapAnotherFormat.
		/// В сучае, если файл не рисунок, bitmapAnotherFormat равен null и возвращается нулевой поинтер(это сигнализирует о том, что с этим
		/// файлом пускай работают другие, а нам он не нужен)
		/// </summary>
		public IntPtr TiffOpenRead(ref string fileName, out Bitmap bitmapAnotherFormat, bool isCreateFileSecondOpen)
		{
			IntPtr tifr = IntPtr.Zero;
			bool secondAttempt = false;
			bitmapAnotherFormat = null;
			if(fileName == null || fileName == "")
				return IntPtr.Zero;
			try
			{
				tifr = extLT.TIFOpenW(fileName, "r" + (isUseLock ? "k" : ""));
				if(tifr == IntPtr.Zero)
				{
					secondAttempt = true;
				}

			}
			catch(Exception ex)//вторая попытка
			{
				WriteToLog(ex);
				secondAttempt = true;
			}
			if(secondAttempt)
			{
				if(tifr != IntPtr.Zero)
					TiffCloseRead(ref tifr);
				if(isCreateFileSecondOpen)
				{
					if(TryOpenFileWithConvertAndCopy(ref fileName))
						tifr = extLT.TIFOpenW(fileName, "r" + (isUseLock ? "k" : ""));
				}
				else
					if(File.Exists(fileName))
						bitmapAnotherFormat = TryOpenFile(fileName);
			}
			return tifr;
		}

		public event EventHandler FileChanged;

		private void OnFileChanged()
		{
			if(FileChanged != null)
				FileChanged(this, EventArgs.Empty);
		}

		public List<PageInfo> GetPagesCollectionFromFile(string fileName, int beginPage, int length, bool isUseInternalPtr)
		{
			FileInfo fi = new FileInfo(fileName);
			if(!fi.Exists)
			{
				WriteToLog("Отсутствует файл " + fileName);
				if(isUseLock && openTif != IntPtr.Zero)
				{
					extLT.TIFClose(openTif);
					openTif = IntPtr.Zero;
				}
				return null;
			}
			if(fi.Length < 21)
			{
				WriteToLog("длина не корректна " + fi.FullName + " " + fi.Length.ToString());
			}

			Bitmap bitmapAnotherFormat = null;
			IntPtr tifr;
			if(isUseLock && openTif != IntPtr.Zero)
				tifr = openTif;
			else
				tifr = TiffOpenRead(ref fileName, out bitmapAnotherFormat, false);

			List<PageInfo> list = new List<PageInfo>();
			try
			{
				if(bitmapAnotherFormat != null)
				{
					bool isIndexed = bitmapAnotherFormat.PixelFormat == PixelFormat.Format1bppIndexed || bitmapAnotherFormat.PixelFormat == PixelFormat.Format4bppIndexed || bitmapAnotherFormat.PixelFormat == PixelFormat.Format8bppIndexed;

					list.Add(new PageInfo() { Image = bitmapAnotherFormat, Compression = new CompressionInfo() });
					return list;
				}
				if(IntPtr.Zero != tifr)
				{
					if(isUseLock && openTif == IntPtr.Zero)
						openTif = tifr;
					if(length <= 0)//считаем, что нужно получить весь файл
					{
						int numPages = extLT.TIFNumberOfDirectories(tifr);
						beginPage = 0;//на всяки случай
						length = numPages;
					}
					for(int i = beginPage; i < beginPage + length; i++)
					{

						try
						{
							PageInfo pageInfo = GetImageFromTiff(tifr, i);
							list.Add(pageInfo);
						}
						catch(Exception ex)
						{
							String errorInfo = "BeginPage = " + beginPage.ToString() + Environment.NewLine +
												" length = " + length.ToString() + Environment.NewLine +
												" i = " + i.ToString() + Environment.NewLine +
												" file name = " + fileName + Environment.NewLine;
							WriteToLog(ex, errorInfo);
							FileInfo fiNew = new FileInfo(fileName);
							if(fi.LastWriteTimeUtc != fiNew.LastWriteTimeUtc || fi.Length != fiNew.Length)
							{
								OnFileChanged();
								return null;
							}
						}
					}
				}
			}
			finally
			{
				if(!isUseLock)
					TiffCloseRead(ref tifr);
			}
			return list;
		}

		/// <summary>
		/// Сохраняет список битмапов и заметок в файл(рекомендуемый метод сохранения любых изменений).
		/// </summary>
		public void SaveBitmapsCollectionToFile(string fileName, List<PageInfo> list, bool isColorSave)
		{
			IntPtr tifw = extLT.TIFOpenW(fileName, "w");
			try
			{
				if(IntPtr.Zero != tifw)
				{
					for(int i = 0; i < list.Count; i++)
					{
						VerifyColorSetImageToTiff(tifw, list[i].Image, list[i].Annotation, isColorSave);
						extLT.TIFWriteDirectory(tifw);
					}
				}
				else
				{
					int er = Marshal.GetLastWin32Error();
					if(er > 0)
						throw new System.ComponentModel.Win32Exception(er);
				}
			}
			catch(Exception ex)
			{
				WriteToLog(ex);
			}
			finally
			{
				if(IntPtr.Zero != tifw)
					TiffCloseWrite(ref tifw);
			}
		}

		/// <summary>
		/// Сохраняет список битмапов и заметок в файл(рекомендуемый метод сохранения любых изменений).
		/// </summary>
		public void SaveBitmapsCollectionToFile(string fileName, List<Bitmap> list, List<byte[]> anns, bool isColorSave)
		{
			IntPtr tifw = extLT.TIFOpenW(fileName, "w");
			try
			{
				if(IntPtr.Zero != tifw)
				{
					for(int i = 0; i < list.Count; i++)
					{
						VerifyColorSetImageToTiff(tifw, list[i], anns[i], isColorSave);
						extLT.TIFWriteDirectory(tifw);
					}
				}
				else
				{
					int er = Marshal.GetLastWin32Error();
					if(er > 0)
						throw new System.ComponentModel.Win32Exception(er);
				}
			}
			catch(Exception ex)
			{
				WriteToLog(ex);
			}
			finally
			{
				if(IntPtr.Zero != tifw)
					TiffCloseWrite(ref tifw);
			}
		}

		public bool QuickColorCheck(string fileName, int startPage, int endPage)
		{
			if(string.IsNullOrEmpty(fileName))
				return false;
			Bitmap bmp = null;
			IntPtr ptr = TiffOpenRead(ref fileName, out bmp, false);
			if(bmp != null)
			{
				bool ret = bmp.PixelFormat != PixelFormat.Format1bppIndexed;
				bmp.Dispose();
				return ret;
			}
			if(ptr == IntPtr.Zero)
				return false;
			int pageCount = extLT.TIFNumberOfDirectories(ptr);
			if(pageCount == 0)
				pageCount = 1;
			extLT.TIFSetDirectory(ptr, (ushort)(pageCount - 1));
			int i = extLT.TIFLastDirectory(ptr);
			if(i == 0)
				++pageCount;
			if(startPage < 0)
				startPage = 0;
			if(startPage >= pageCount)
				startPage = pageCount - 1;
			if(endPage < 0 || endPage >= pageCount)
			{
				endPage = pageCount - 1;
			}
			if(startPage > endPage)
				endPage = startPage;
			int bits = 0, compression = 0;
			for(ushort k = (ushort)startPage; k <= endPage; k++)
			{
				extLT.TIFSetDirectory(ptr, k);
				extLT.TIFGetField(ptr, TIFFTAG_COMPRESSION, ref compression);
				if(compression < 5 && compression > 1)
					continue;
				extLT.TIFGetField(ptr, TIFFTAG_BITSPERSAMPLE, ref bits);
				if(bits > 1)
				{
					TiffCloseRead(ref ptr);
					return true;
				}
			}
			TiffCloseRead(ref ptr);
			return false;
		}

		/// <summary>
		/// Удаление из файла
		/// </summary>
		public string DeletePart(string fileName, int startPage, int length, string newFileName)
		{
			Bitmap bitmapAnotherFormat = null;
			lock(lockobject)
			{
				IntPtr tifr = TiffOpenRead(ref fileName, out bitmapAnotherFormat, true);
				if(bitmapAnotherFormat != null)
				{
					throw new Exception("Incorrect file type");
				}
				if(tifr == null)
					throw new Exception("Can't open file " + fileName);
				else
					TiffCloseRead(ref tifr);
				if(string.IsNullOrEmpty(newFileName) || fileName.Equals(newFileName, StringComparison.CurrentCultureIgnoreCase))
				{
					//if(!File.Exists(fileName + ".tmp"))
					//    newFileName = fileName + ".tmp";
					//else
					//    return null;
					newFileName = fileName + ".tmp";
					if(File.Exists(newFileName))
						try { File.Delete(newFileName); }
						catch { return null; }
				}
				TiffCloseRead(ref tifr);
				if(extLT.TIFDeletePart(fileName, newFileName, (ushort)startPage, (ushort)length))
					return newFileName;
				else
					return null;
			}
		}

		/// <summary>
		/// Вставка страницы в тиф, предварительнос считывая страницы в буфер. Если страница не 1bpp, не 8bpp не 32argb, конвертит в 1bpp
		/// Вслучае, если страница добавляется в конец, - рекомендуемый метод сохрананения(самый быстрый)
		/// </summary>
		public void InsertAfterPage(string fileName, int page, List<PageInfo> newCacheImageList)
		{
			Bitmap bitmapAnotherFormat = null;
			IntPtr tifr = TiffOpenRead(ref fileName, out bitmapAnotherFormat, true);

			if(IntPtr.Zero != tifr)
			{
				int numPages = extLT.TIFNumberOfDirectories(tifr);
				IntPtr tifw = extLT.TIFOpenW(fileName + ".tmp", "w");
				for(int i = 0; i < numPages; i++)
				{
					PageInfo pageInfo = GetImageFromTiff(tifr, i);
					SetImageToTiff(tifw, pageInfo.Image, pageInfo.Annotation);
					extLT.TIFWriteDirectory(tifw);
					if(page == i)
					{
						for(int k = 0; k < newCacheImageList.Count; k++)
						{
							PageInfo newImg = newCacheImageList[k];
							if(newImg.Image.PixelFormat != PixelFormat.Format1bppIndexed && newImg.Image.PixelFormat != PixelFormat.Format8bppIndexed && newImg.Image.PixelFormat != PixelFormat.Format32bppArgb)
							{
								newImg.Image = ConvertToBitonal(newImg.Image);
							}
							SetImageToTiff(tifw, newImg.Image, newImg.Annotation);
							extLT.TIFWriteDirectory(tifw);
						}
					}
				}
				if(IntPtr.Zero != tifw)
					TiffCloseWrite(ref tifw);
			}
			TiffCloseRead(ref tifr);
		}

		/// <summary>
		/// Вставка страницы в тиф, предварительнос считывая страницы в буфер. Если страница не 1bpp, не 8bpp не 32argb, конвертит в 1bpp
		/// </summary>
		public void InsertBeforePage(string fileName, int page, List<PageInfo> newCacheImageList)
		{
			Bitmap bitmapAnotherFormat = null;
			IntPtr tifr = TiffOpenRead(ref fileName, out bitmapAnotherFormat, true);

			if(IntPtr.Zero != tifr)
			{
				int numPages = extLT.TIFNumberOfDirectories(tifr);
				IntPtr tifw = extLT.TIFOpenW(fileName + ".tmp", "w");
				for(int i = 0; i < numPages; i++)
				{
					if(page == i)
					{
						for(int k = 0; k < newCacheImageList.Count; k++)
						{
							PageInfo newImg = newCacheImageList[k];
							if(newImg.Image.PixelFormat != PixelFormat.Format1bppIndexed && newImg.Image.PixelFormat != PixelFormat.Format8bppIndexed && newImg.Image.PixelFormat != PixelFormat.Format32bppArgb)
							{
								newImg.Image = ConvertToBitonal(newImg.Image);
							}
							SetImageToTiff(tifw, newImg.Image, newImg.Annotation);
							extLT.TIFWriteDirectory(tifw);
						}
					}
					PageInfo pageInfo = GetImageFromTiff(tifr, i);
					SetImageToTiff(tifw, pageInfo.Image, pageInfo.Annotation);
					extLT.TIFWriteDirectory(tifw);
				}
				if(IntPtr.Zero != tifw)
					TiffCloseWrite(ref tifw);
			}
			TiffCloseRead(ref tifr);
		}

		/// <summary>
		/// Добавляет одну картинку в тиф, работает с цветом
		/// </summary>
		/// <param name="tif"></param>
		/// <param name="img"></param>
		/// <param name="tiffAnnotation"></param>
		/// <param name="isVerifyColor">true - проверять на цвет, если цветное изображение, сохранять в цвете, иначе конверт в 1bpp, false - конверт в 1bpp</param>
		internal void VerifyColorSetImageToTiff(IntPtr tifr, Bitmap img, byte[] tiffAnnotation, bool isVerifyColor)
		{
			if(isVerifyColor)//если есть цветная картинка, то проверяем что точно цвет, иначе конверт
			{
				if(!IsColorPixelFormat(img.PixelFormat) || !IsColorYCbCr(img))
				{
					if(img.PixelFormat != PixelFormat.Format1bppIndexed)
						img = this.ConvertToBitonal(img);
				}
			}
			else
				if(img.PixelFormat != PixelFormat.Format1bppIndexed)
					img = this.ConvertToBitonal(img);

			SetImageToTiff(tifr, img, tiffAnnotation);
		}

		/// <summary>
		/// Добавляет одну картинку в тиф(всегда append директории). Cледить за порядком директории должен вызывающий метод
		/// </summary>
		public void SetImageToTiff(IntPtr tifr, byte[] img, byte[] tiffAnnotation, PixelFormat pf, int width, int height, bool isgray, double hres, double vres, ColorPalette palette)
		{
			int Samples = pf == PixelFormat.Format1bppIndexed || pf == PixelFormat.Format8bppIndexed ? 1 : 3;
			int bitsSample = pf == PixelFormat.Format1bppIndexed ? 1 : 8;
			int photometric = Samples == 3 ? PHOTOMETRIC_RGB : PHOTOMETRIC_MINISWHITE;
			int compression = photometric == PHOTOMETRIC_RGB ? COMPRESSION_JPEG : COMPRESSION_CCITTFAX4;
			int defaultStripSize;
			if(pf == PixelFormat.Format8bppIndexed)
			{
				compression = COMPRESSION_DEFLATE;
				photometric = (isgray ? PHOTOMETRIC_MINISBLACK : PHOTOMETRIC_PALETTE);
			}
			//compression = COMPRESSION_DEFLATE;
			//compression = COMPRESSION_JPEG;
			//compression = COMPRESSION_PACKBITS;
			//заметки
			byte[] bannotation = tiffAnnotation;
			IntPtr ptrTag = extLT.TIFFindField(tifr, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
			if(ptrTag == IntPtr.Zero && bannotation != null && bannotation.Length > 0)
			{

				AddAnnotationTag(tifr);
				//ptrTag = extLT.TIFFFindFieldInfo(tifr, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
				//LibTiffHelper.TIFFFieldInfo sdtr = (LibTiffHelper.TIFFFieldInfo)Marshal.PtrToStructure(ptrTag, typeof(LibTiffHelper.TIFFFieldInfo));
				//string name = Marshal.PtrToStringAnsi(new IntPtr(ptrTag.ToInt32() + 16));
			}
			else if(ptrTag != IntPtr.Zero && bannotation != null && bannotation.Length > 0)
			{
				//изменяем размеры тага
				Marshal.WriteInt16(ptrTag, 4, -3);
				Marshal.WriteInt16(ptrTag, 6, -3);

			}
			extLT.TIFSetField(tifr, TIFFTAG_IMAGEWIDTH, width);
			extLT.TIFSetField(tifr, TIFFTAG_IMAGELENGTH, height);
			extLT.TIFSetField(tifr, TIFFTAG_BITSPERSAMPLE, bitsSample);
			extLT.TIFSetField(tifr, TIFFTAG_COMPRESSION, compression);
			extLT.TIFSetField(tifr, TIFFTAG_PHOTOMETRIC, photometric);
			extLT.TIFSetField(tifr, TIFFTAG_SAMPLESPERPIXEL, Samples);
			extLT.TIFSetField(tifr, TIFFTAG_FILLORDER, FILLORDER_MSB2LSB);
			defaultStripSize = extLT.TIFDefaultStripSize(tifr, uint.MaxValue);
			if(defaultStripSize < 1)
				defaultStripSize = height;
			extLT.TIFSetField(tifr, TIFFTAG_ROWSPERSTRIP, defaultStripSize);
			extLT.TIFSetField(tifr, TIFFTAG_PLANARCONFIG, PLANARCONFIG_CONTIG);
			extLT.TIFSetField(tifr, TIFFTAG_XRESOLUTION, hres);//только double
			extLT.TIFSetField(tifr, TIFFTAG_YRESOLUTION, vres);
			extLT.TIFSetField(tifr, TIFFTAG_ORIENTATION, ORIENTATION_TOPLEFT);
			extLT.TIFSetField(tifr, TIFFTAG_RESOLUTIONUNIT, RESUNIT_INCH);
			if(compression == COMPRESSION_LZW || compression == COMPRESSION_DEFLATE || compression == COMPRESSION_ADOBE_DEFLATE)
				extLT.TIFSetField(tifr, TIFFTAG_PREDICTOR, 2);
			else if(compression == COMPRESSION_JPEG || compression == COMPRESSION_OJPEG)
			{
				extLT.TIFSetField(tifr, TIFFTAG_YCBCRSUBSAMPLING, 4, 2);
				extLT.TIFSetField(tifr, TIFFTAG_PHOTOMETRIC, PHOTOMETRIC_YCBCR);
				extLT.TIFSetField(tifr, TIFFTAG_JPEGCOLORMODE, JPEGCOLORMODE_RGB);
				extLT.TIFSetField(tifr, TIFFTAG_JPEGQUALITY, 75); //JPEGCOLORMODE_RGB
				////extLT.TIFFSetField(tifr, TIFFTAG_JPEGPROC, JPEGPROC_LOSSLESS);
				////extLT.TIFFSetField(tifr, TIFFTAG_YCBCRPOSITIONING, YCBCRPOSITION_COSITED);
				////float[] ycbcrCoeffs = new float[3] { .299F, .587F, .114F };
				////extLT.TIFFSetField(tifr, TIFFTAG_YCBCRCOEFFICIENTS, ycbcrCoeffs);
			}

			//extLT.TIFFSetField(tifr, TIFFTAG_SUBIFD, (long)3);
			//extLT.TIFFSetField(tifr, TIFFTAG_SUBFILETYPE, FILETYPE_PAGE);
			//extLT.TIFFSetField(tifr, TIFFTAG_PAGENUMBER, (ushort)(TIFFNumberOfDirectories(tifr) + 1), (ushort)(TIFFNumberOfDirectories(tifr) + 1));


			IntPtr ptrba = IntPtr.Zero;
			GCHandle hb = new GCHandle();
			try
			{
				//сохранение аннотации
				if(bannotation != null && bannotation.Length > 0)
				{

					hb = GCHandle.Alloc(bannotation, GCHandleType.Pinned);
					ptrba = extLT._TIFmalloc(bannotation.Length);
					IntPtr ptrBufferAnnotationTiff = hb.AddrOfPinnedObject();
					extLT._TIFmemcpy(ptrba, ptrBufferAnnotationTiff, bannotation.Length);
					IntPtr ptrTagV = extLT.TIFFindField(tifr, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
					if(ptrTagV == IntPtr.Zero)
						AddAnnotationTag(tifr);

					int annRet1 = extLT.TIFSetField(tifr, TIFFTAG_ANNOTATION, (uint)bannotation.Length, ptrba);
				}
			}
			catch(Exception ex) { WriteToLog(ex); }
			finally
			{
				if(ptrba != IntPtr.Zero)
					extLT._TIFfree(ptrba);
				if(hb.IsAllocated)
					hb.Free();
			}
			//сохранение рисунков
			if(pf == PixelFormat.Format1bppIndexed)
			{
				int stripSize = extLT.TIFStripSize(tifr);
				int stripMax = extLT.TIFNumberOfStrips(tifr);
				int cnt = height * stripSize;
				int scanLineSize = extLT.TIFScanlineSize(tifr);
				byte[] destinationBuffer = new byte[stripSize];
				int lastSize = (height % (stripSize / scanLineSize)) * scanLineSize;
				if(lastSize < 1)
					lastSize = stripSize;
				int sourceMoveIndex = 0;
				GCHandle bHandle;
				bHandle = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
				for(int y = 0; y < stripMax; y++)
				{
					sourceMoveIndex = defaultStripSize * stripSize * y;
					for(int x = 0; x < ((y < stripMax - 1) ? stripSize : lastSize); x++)
					{
						destinationBuffer[x] = (byte)(img[(x / scanLineSize) * stripSize + x % scanLineSize + sourceMoveIndex] ^ 255);
					}
					int ret = 0;
					try
					{
						ret = extLT.TIFWriteEncodedStrip(tifr, (uint)y, bHandle.AddrOfPinnedObject(), ((y == stripMax - 1) ? lastSize : destinationBuffer.Length));
					}
					catch(Exception ex)
					{
						destinationBuffer = null;
						throw new Exception("Неудачное сохранение рисунка", ex);
					}

					if(ret <= 0)
					{
						if(bHandle.IsAllocated)
							bHandle.Free();

						destinationBuffer = null;
						throw new Exception("Неудачное сохранение рисунка");
					}
				}
				if(bHandle.IsAllocated)
					bHandle.Free();
				destinationBuffer = null;
			}
			else if(pf == PixelFormat.Format8bppIndexed)
			{
				if(photometric != PHOTOMETRIC_MINISBLACK)
				{
					ushort[] red = new ushort[256];
					ushort[] green = new ushort[256];
					ushort[] blue = new ushort[256];

					for(int c = 0, n = 0; c < palette.Entries.Length; c++, n++)
					{
						red[n] = (ushort)(palette.Entries[c].R << 8);
						green[n] = (ushort)(palette.Entries[c].G << 8);
						blue[n] = (ushort)(palette.Entries[c].B << 8);
					}

					extLT.TIFSetField(tifr, TIFFTAG_COLORMAP, red, green, blue);
				}

				int scanLineSize = extLT.TIFScanlineSize(tifr);
				int cnt = height * scanLineSize;

				byte[] destinationBuffer = new byte[scanLineSize];
				int sourceIndex = 0;

				GCHandle bHandle;
				IntPtr bufPtrWrite;
				for(int y = 0; y < height; y++)
				{
					sourceIndex = y * scanLineSize;
					Buffer.BlockCopy(img, sourceIndex, destinationBuffer, 0, scanLineSize);
					bHandle = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
					bufPtrWrite = bHandle.AddrOfPinnedObject();
					int result = 0;
					try
					{
						result = extLT.TIFWriteScanline(tifr, bufPtrWrite, (uint)y, 0);
					}
					catch(Exception ex)
					{
						throw new Exception("Неудачное сохранение рисунка", ex);
					}
					finally
					{
						if(bHandle.IsAllocated)
							bHandle.Free();
					}
					if(result == -1)
						throw new Exception("Неудачное сохранение рисунка");
				}
			}
			else if(pf == PixelFormat.Format32bppArgb || pf == PixelFormat.Format24bppRgb)
			{
				int sampleSource = pf == PixelFormat.Format32bppArgb ? 4 : 3;

				int stripSize = extLT.TIFStripSize(tifr);
				int stripMax = extLT.TIFNumberOfStrips(tifr);
				int scanLineSize = extLT.TIFScanlineSize(tifr);
				int rowsperstrip = stripSize / scanLineSize;
				int strideSize = scanLineSize;
				byte[] buffer = new byte[stripSize];
				int lastrows = height % defaultStripSize;
				int lastSize = defaultStripSize * scanLineSize;
				if(lastrows == 0)
					lastrows = defaultStripSize;
				else
					lastSize = lastrows * scanLineSize;
				int pos, ptr, ret, max = 0;
				GCHandle bHandle;
				for(int y = 0; y < stripMax; y++)
				{
					pos = 0;
					ptr = 0;
					Buffer.BlockCopy(img, ptr, buffer, 0, defaultStripSize);
					bHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
					try
					{
						ret = extLT.TIFWriteRawStrip(tifr, (uint)y, bHandle.AddrOfPinnedObject(), ((y == stripMax - 1) ? lastSize : buffer.Length));
					}
					catch(Exception ex)
					{
						buffer = null;
						throw new Exception("Неудачное сохранение рисунка", ex);
					}
					finally
					{
						if(bHandle.IsAllocated)
							bHandle.Free();
					}
					if(ret <= 0)
					{
						buffer = null;
						throw new Exception("Неудачное сохранение рисунка");
					}
				}
				buffer = null;
			}
		}

		/// <summary>
		/// Добавляет одну картинку в тиф(всегда append директории). Cледить за порядком директории должен вызывающий метод
		/// </summary>
		public void SetImageToTiffA(IntPtr tifr, Bitmap img, byte[] tiffAnnotation)
		{
			int Samples = img.PixelFormat == PixelFormat.Format1bppIndexed || img.PixelFormat == PixelFormat.Format8bppIndexed ? 1 : 3;
			int bitsSample = img.PixelFormat == PixelFormat.Format1bppIndexed ? 1 : 8;
			int photometric = Samples == 3 ? PHOTOMETRIC_RGB : PHOTOMETRIC_MINISWHITE;
			int compression = photometric == PHOTOMETRIC_RGB ? COMPRESSION_JPEG : COMPRESSION_CCITTFAX4;
			int defaultStripSize;
			if(img.PixelFormat == PixelFormat.Format8bppIndexed)
			{
				compression = COMPRESSION_DEFLATE;
				photometric = (IsGrayScale(img) ? PHOTOMETRIC_MINISBLACK : PHOTOMETRIC_PALETTE);
			}
			//compression = COMPRESSION_DEFLATE;
			//compression = COMPRESSION_JPEG;
			//compression = COMPRESSION_PACKBITS;
			//заметки
			byte[] bannotation = tiffAnnotation;
			IntPtr ptrTag = extLT.TIFFindField(tifr, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
			if(ptrTag == IntPtr.Zero && bannotation != null && bannotation.Length > 0)
			{

				AddAnnotationTag(tifr);
			}
			else if(ptrTag != IntPtr.Zero && bannotation != null && bannotation.Length > 0)
			{
				//изменяем размеры тага
				Marshal.WriteInt16(ptrTag, 4, -3);
				Marshal.WriteInt16(ptrTag, 6, -3);

			}
			extLT.TIFSetField(tifr, TIFFTAG_IMAGEWIDTH, img.Width);
			extLT.TIFSetField(tifr, TIFFTAG_IMAGELENGTH, img.Height);
			extLT.TIFSetField(tifr, TIFFTAG_BITSPERSAMPLE, bitsSample);
			extLT.TIFSetField(tifr, TIFFTAG_COMPRESSION, compression);
			extLT.TIFSetField(tifr, TIFFTAG_PHOTOMETRIC, photometric);
			extLT.TIFSetField(tifr, TIFFTAG_SAMPLESPERPIXEL, Samples);
			extLT.TIFSetField(tifr, TIFFTAG_FILLORDER, FILLORDER_MSB2LSB);
			defaultStripSize = extLT.TIFDefaultStripSize(tifr, uint.MaxValue);
			if(defaultStripSize < 1)
				defaultStripSize = img.Height;
			extLT.TIFSetField(tifr, TIFFTAG_ROWSPERSTRIP, defaultStripSize);
			extLT.TIFSetField(tifr, TIFFTAG_PLANARCONFIG, PLANARCONFIG_CONTIG);
			extLT.TIFSetField(tifr, TIFFTAG_XRESOLUTION, (double)img.HorizontalResolution);//только double
			extLT.TIFSetField(tifr, TIFFTAG_YRESOLUTION, (double)img.VerticalResolution);
			extLT.TIFSetField(tifr, TIFFTAG_ORIENTATION, ORIENTATION_TOPLEFT);
			extLT.TIFSetField(tifr, TIFFTAG_RESOLUTIONUNIT, RESUNIT_INCH);
			if(compression == COMPRESSION_LZW || compression == COMPRESSION_DEFLATE || compression == COMPRESSION_ADOBE_DEFLATE)
				extLT.TIFSetField(tifr, TIFFTAG_PREDICTOR, 2);
			else if(compression == COMPRESSION_JPEG || compression == COMPRESSION_OJPEG)
			{
				extLT.TIFSetField(tifr, TIFFTAG_YCBCRSUBSAMPLING, 4, 2);
				extLT.TIFSetField(tifr, TIFFTAG_PHOTOMETRIC, PHOTOMETRIC_YCBCR);
				extLT.TIFSetField(tifr, TIFFTAG_JPEGCOLORMODE, JPEGCOLORMODE_RGB);
				extLT.TIFSetField(tifr, TIFFTAG_JPEGQUALITY, 75); //JPEGCOLORMODE_RGB
			}

			IntPtr ptrba = IntPtr.Zero;
			GCHandle hb = new GCHandle();
			try
			{
				//сохранение аннотации
				if(bannotation != null && bannotation.Length > 0)
				{

					hb = GCHandle.Alloc(bannotation, GCHandleType.Pinned);
					ptrba = extLT._TIFmalloc(bannotation.Length);
					IntPtr ptrBufferAnnotationTiff = hb.AddrOfPinnedObject();
					extLT._TIFmemcpy(ptrba, ptrBufferAnnotationTiff, bannotation.Length);
					IntPtr ptrTagV = extLT.TIFFindField(tifr, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
					if(ptrTagV == IntPtr.Zero)
						AddAnnotationTag(tifr);

					int annRet1 = extLT.TIFSetField(tifr, TIFFTAG_ANNOTATION, (uint)bannotation.Length, ptrba);
				}
			}
			catch(Exception ex) { WriteToLog(ex); }
			finally
			{
				if(ptrba != IntPtr.Zero)
					extLT._TIFfree(ptrba);
				if(hb.IsAllocated)
					hb.Free();
			}
			//сохранение рисунков
			if(img.PixelFormat == PixelFormat.Format1bppIndexed)
			{

				BitmapData bd = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
				int cnt = img.Height * bd.Stride;
				byte[] sourceBuffer = new byte[cnt];
				Marshal.Copy(bd.Scan0, sourceBuffer, 0, cnt);

				int stripSize = extLT.TIFStripSize(tifr);
				int stripMax = extLT.TIFNumberOfStrips(tifr);
				int scanLineSize = extLT.TIFScanlineSize(tifr);
				byte[] destinationBuffer = new byte[stripSize];
				int height = bd.Height;
				int width = bd.Width;
				int strideSize = bd.Stride;
				int lastSize = (height % (stripSize / scanLineSize)) * scanLineSize;
				if(lastSize < 1)
					lastSize = stripSize;
				int sourceMoveIndex = 0;
				GCHandle bHandle;
				bHandle = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
				for(int y = 0; y < stripMax; y++)
				{
					sourceMoveIndex = defaultStripSize * strideSize * y;
					for(int x = 0; x < ((y < stripMax - 1) ? stripSize : lastSize); x++)
					{
						destinationBuffer[x] = (byte)(sourceBuffer[(x / scanLineSize) * strideSize + x % scanLineSize + sourceMoveIndex] ^ 255);
					}
					int ret = 0;
					try
					{
						ret = extLT.TIFWriteEncodedStrip(tifr, (uint)y, bHandle.AddrOfPinnedObject(), ((y == stripMax - 1) ? lastSize : destinationBuffer.Length));
					}
					catch(Exception ex)
					{
						sourceBuffer = null;
						destinationBuffer = null;
						throw new Exception("Неудачное сохранение рисунка", ex);
					}

					if(ret <= 0)
					{
						if(bHandle.IsAllocated)
							bHandle.Free();
						img.UnlockBits(bd);
						sourceBuffer = null;
						destinationBuffer = null;
						throw new Exception("Неудачное сохранение рисунка");
					}
				}
				if(bHandle.IsAllocated)
					bHandle.Free();
				if(bd != null)
				{
					img.UnlockBits(bd);
					bd = null;
				}
				sourceBuffer = null;
				destinationBuffer = null;
			}
			else if(img.PixelFormat == PixelFormat.Format8bppIndexed)
			{
				if(photometric != PHOTOMETRIC_MINISBLACK)
				{
					ushort[] red = new ushort[256];
					ushort[] green = new ushort[256];
					ushort[] blue = new ushort[256];

					for(int c = 0, n = 0; c < img.Palette.Entries.Length; c++, n++)
					{
						red[n] = (ushort)(img.Palette.Entries[c].R << 8);
						green[n] = (ushort)(img.Palette.Entries[c].G << 8);
						blue[n] = (ushort)(img.Palette.Entries[c].B << 8);
					}

					extLT.TIFSetField(tifr, TIFFTAG_COLORMAP, red, green, blue);
				}
				BitmapData bd = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
				int cnt = img.Height * bd.Stride;
				byte[] sourceBuffer = new byte[cnt];
				Marshal.Copy(bd.Scan0, sourceBuffer, 0, cnt);

				int scanLineSize = extLT.TIFScanlineSize(tifr);

				byte[] destinationBuffer = new byte[scanLineSize];
				int sourceIndex = 0;
				int height = bd.Height;
				int width = bd.Width;

				GCHandle bHandle;
				IntPtr bufPtrWrite;
				for(int y = 0; y < height; y++)
				{
					sourceIndex = y * bd.Stride;
					Buffer.BlockCopy(sourceBuffer, sourceIndex, destinationBuffer, 0, scanLineSize);
					bHandle = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
					bufPtrWrite = bHandle.AddrOfPinnedObject();
					int result = 0;
					try
					{
						result = extLT.TIFWriteScanline(tifr, bufPtrWrite, (uint)y, 0);
					}
					catch(Exception ex)
					{
						throw new Exception("Неудачное сохранение рисунка", ex);
					}
					finally
					{
						if(bd != null)
							img.UnlockBits(bd);
						if(bHandle.IsAllocated)
							bHandle.Free();
					}
					if(result == -1)
						throw new Exception("Неудачное сохранение рисунка");
				}
			}
			else if(img.PixelFormat == PixelFormat.Format32bppArgb || img.PixelFormat == PixelFormat.Format24bppRgb)
			{
				int sampleSource = img.PixelFormat == PixelFormat.Format32bppArgb ? 4 : 3;

				int stripSize = extLT.TIFStripSize(tifr);
				int stripMax = extLT.TIFNumberOfStrips(tifr);
				int scanLineSize = extLT.TIFScanlineSize(tifr);
				int rowsperstrip = stripSize / scanLineSize;
				int height = img.Height;
				int width = img.Width;
				BitmapData bd = null;
				if(img.PixelFormat == PixelFormat.Format32bppArgb)
					bd = img.LockBits(new Rectangle(0, 0, img.Width, defaultStripSize), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
				else
					bd = img.LockBits(new Rectangle(0, 0, img.Width, defaultStripSize), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
				int strideSize = bd.Stride;
				byte[] b = new byte[strideSize * defaultStripSize];
				byte[] buffer = new byte[stripSize];
				int lastrows = height % defaultStripSize;
				int lastSize = defaultStripSize * scanLineSize;
				if(lastrows == 0)
					lastrows = defaultStripSize;
				else
					lastSize = lastrows * scanLineSize;
				int i, pos, ptr, ret, max = 0;
				GCHandle bHandle;
				for(int y = 0; y < stripMax; y++)
				{
					if(y < stripMax - 1)
					{
						if(y > 0)
							if(img.PixelFormat == PixelFormat.Format32bppArgb)
								bd = img.LockBits(new Rectangle(0, y * defaultStripSize, img.Width, defaultStripSize), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
							else
								bd = img.LockBits(new Rectangle(0, y * defaultStripSize, img.Width, defaultStripSize), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
						Marshal.Copy(bd.Scan0, b, 0, b.Length);
						max = width * defaultStripSize;
					}
					else
					{
						if(y > 0)
							if(img.PixelFormat == PixelFormat.Format32bppArgb)
								bd = img.LockBits(new Rectangle(0, y * defaultStripSize, img.Width, lastrows), ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
							else
								bd = img.LockBits(new Rectangle(0, y * defaultStripSize, img.Width, lastrows), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
						int s = lastrows * strideSize;
						Marshal.Copy(bd.Scan0, b, 0, s);
						max = lastrows * width;
					}
					pos = 0;
					ptr = 0;
					for(i = 0; i < max; i++, pos += 3)
					{
						ptr = (i * 3 / scanLineSize) * (strideSize) + (i % (scanLineSize / 3)) * sampleSource;
						buffer[pos] = b[ptr + 2];
						buffer[pos + 1] = b[ptr + 1];
						buffer[pos + 2] = b[ptr];
					}
					bHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
					try
					{
						ret = extLT.TIFWriteEncodedStrip(tifr, (uint)y, bHandle.AddrOfPinnedObject(), max * 3);
					}
					catch(Exception ex)
					{
						b = null;
						buffer = null;
						throw new Exception("Неудачное сохранение рисунка", ex);
					}
					finally
					{
						if(bHandle.IsAllocated)
							bHandle.Free();
						if(bd != null)
						{
							img.UnlockBits(bd);
							bd = null;
						}
					}
					if(ret <= 0)
					{
						b = null;
						buffer = null;
						throw new Exception("Неудачное сохранение рисунка");
					}
				}
				b = null;
				buffer = null;
			}
		}


		/// <summary>
		/// Добавляет одну картинку в тиф(всегда append директории). Cледить за порядком директории должен вызывающий метод
		/// </summary>
		public bool SetImageToTiff(IntPtr tifr, Bitmap img, byte[] tiffAnnotation)
		{
			int Samples = img.PixelFormat == PixelFormat.Format1bppIndexed || img.PixelFormat == PixelFormat.Format8bppIndexed ? 1 : 3;
			int bitsSample = img.PixelFormat == PixelFormat.Format1bppIndexed ? 1 : 8;
			int photometric = Samples == 3 ? PHOTOMETRIC_RGB : PHOTOMETRIC_MINISWHITE;
			int compression = photometric == PHOTOMETRIC_RGB ? COMPRESSION_JPEG : COMPRESSION_CCITTFAX4;
			int defaultStripSize;
			if(img.PixelFormat == PixelFormat.Format8bppIndexed)
			{
				compression = COMPRESSION_DEFLATE;
				photometric = (IsGrayScale(img) ? PHOTOMETRIC_MINISBLACK : PHOTOMETRIC_PALETTE);
			}
			//compression = COMPRESSION_DEFLATE;
			//compression = COMPRESSION_JPEG;
			//compression = COMPRESSION_PACKBITS;
			//заметки
			byte[] bannotation = tiffAnnotation;
			IntPtr ptrTag = extLT.TIFFindField(tifr, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
			if(ptrTag == IntPtr.Zero && bannotation != null && bannotation.Length > 0)
			{

				AddAnnotationTag(tifr);
				//ptrTag = extLT.TIFFFindFieldInfo(tifr, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
				//LibTiffHelper.TIFFFieldInfo sdtr = (LibTiffHelper.TIFFFieldInfo)Marshal.PtrToStructure(ptrTag, typeof(LibTiffHelper.TIFFFieldInfo));
				//string name = Marshal.PtrToStringAnsi(new IntPtr(ptrTag.ToInt32() + 16));
			}
			else if(ptrTag != IntPtr.Zero && bannotation != null && bannotation.Length > 0)
			{
				//изменяем размеры тага
				Marshal.WriteInt16(ptrTag, 4, -3);
				Marshal.WriteInt16(ptrTag, 6, -3);

			}
			//IntPtr ptrTag1 = extLT.TIFFFindFieldInfo(tifr, TIFFTAG_IMAGEWIDTH, TIFFDataType.TIFF_LONG);
			//TIFFFieldInfo fi = (TIFFFieldInfo)Marshal.PtrToStructure(ptrTag1, typeof(TIFFFieldInfo));
			extLT.TIFSetField(tifr, TIFFTAG_IMAGEWIDTH, img.Width);
			extLT.TIFSetField(tifr, TIFFTAG_IMAGELENGTH, img.Height);
			extLT.TIFSetField(tifr, TIFFTAG_BITSPERSAMPLE, bitsSample);
			extLT.TIFSetField(tifr, TIFFTAG_COMPRESSION, compression);
			extLT.TIFSetField(tifr, TIFFTAG_PHOTOMETRIC, photometric);
			extLT.TIFSetField(tifr, TIFFTAG_SAMPLESPERPIXEL, Samples);
			//extLT.TIFSetField(tifr, TIFFTAG_FILLORDER, FILLORDER_MSB2LSB);
			defaultStripSize = extLT.TIFDefaultStripSize(tifr, uint.MaxValue);
			if(defaultStripSize < 1)
				defaultStripSize = img.Height;
			extLT.TIFSetField(tifr, TIFFTAG_ROWSPERSTRIP, defaultStripSize);
			extLT.TIFSetField(tifr, TIFFTAG_PLANARCONFIG, PLANARCONFIG_CONTIG);
			extLT.TIFSetField(tifr, TIFFTAG_XRESOLUTION, (double)img.HorizontalResolution);//только double
			extLT.TIFSetField(tifr, TIFFTAG_YRESOLUTION, (double)img.VerticalResolution);
			extLT.TIFSetField(tifr, TIFFTAG_ORIENTATION, ORIENTATION_TOPLEFT);
			//extLT.TIFSetField(tifr, TIFFTAG_RESOLUTIONUNIT, RESUNIT_INCH);
			if(compression == COMPRESSION_LZW || compression == COMPRESSION_DEFLATE || compression == COMPRESSION_ADOBE_DEFLATE)
				extLT.TIFSetField(tifr, TIFFTAG_PREDICTOR, 2);
			else if(compression == COMPRESSION_JPEG || compression == COMPRESSION_OJPEG)
			{
				extLT.TIFSetField(tifr, TIFFTAG_YCBCRSUBSAMPLING, 4, 2);
				extLT.TIFSetField(tifr, TIFFTAG_PHOTOMETRIC, PHOTOMETRIC_YCBCR);
				extLT.TIFSetField(tifr, TIFFTAG_JPEGCOLORMODE, JPEGCOLORMODE_RGB);
				extLT.TIFSetField(tifr, TIFFTAG_JPEGQUALITY, 75); //JPEGCOLORMODE_RGB
				////extLT.TIFFSetField(tifr, TIFFTAG_JPEGPROC, JPEGPROC_LOSSLESS);
				////extLT.TIFFSetField(tifr, TIFFTAG_YCBCRPOSITIONING, YCBCRPOSITION_COSITED);
				////float[] ycbcrCoeffs = new float[3] { .299F, .587F, .114F };
				////extLT.TIFFSetField(tifr, TIFFTAG_YCBCRCOEFFICIENTS, ycbcrCoeffs);
			}

			//extLT.TIFFSetField(tifr, TIFFTAG_SUBIFD, (long)3);
			//extLT.TIFFSetField(tifr, TIFFTAG_SUBFILETYPE, FILETYPE_PAGE);
			//extLT.TIFFSetField(tifr, TIFFTAG_PAGENUMBER, (ushort)(TIFFNumberOfDirectories(tifr) + 1), (ushort)(TIFFNumberOfDirectories(tifr) + 1));


			IntPtr ptrba = IntPtr.Zero;
			GCHandle hb = new GCHandle();
			try
			{
				//сохранение аннотации
				if(bannotation != null && bannotation.Length > 0)
				{

					hb = GCHandle.Alloc(bannotation, GCHandleType.Pinned);
					ptrba = extLT._TIFmalloc(bannotation.Length);
					IntPtr ptrBufferAnnotationTiff = hb.AddrOfPinnedObject();
					extLT._TIFmemcpy(ptrba, ptrBufferAnnotationTiff, bannotation.Length);
					IntPtr ptrTagV = extLT.TIFFindField(tifr, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
					if(ptrTagV == IntPtr.Zero)
						AddAnnotationTag(tifr);

					int annRet1 = extLT.TIFSetField(tifr, TIFFTAG_ANNOTATION, (uint)bannotation.Length, ptrba);
				}
			}
			catch(Exception ex) { WriteToLog(ex); return false; }
			finally
			{
				if(ptrba != IntPtr.Zero)
					extLT._TIFfree(ptrba);
				if(hb.IsAllocated)
					hb.Free();
			}
			//сохранение рисунков
			if(img.PixelFormat == PixelFormat.Format1bppIndexed)
			{
				BitmapData bd = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format1bppIndexed);
				int cnt = img.Height * bd.Stride;
				//IntPtr bufPtr = Marshal.AllocCoTaskMem(cnt);
				//extLT._TIFmemcpy(bufPtr, bd.Scan0, cnt);
				byte[] sourceBuffer = new byte[cnt];
				Marshal.Copy(bd.Scan0, sourceBuffer, 0, cnt);
				//Marshal.FreeCoTaskMem(bufPtr);

				int stripSize = extLT.TIFStripSize(tifr);
				int stripMax = extLT.TIFNumberOfStrips(tifr);
				int scanLineSize = extLT.TIFScanlineSize(tifr);
				byte[] destinationBuffer = new byte[stripSize];
				int height = bd.Height;
				int width = bd.Width;
				int strideSize = bd.Stride;
				int lastSize = (height % (stripSize / scanLineSize)) * scanLineSize;
				if(lastSize < 1)
					lastSize = stripSize;
				int sourceMoveIndex = 0;
				GCHandle bHandle;
				bHandle = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
				for(int y = 0; y < stripMax; y++)
				{
					sourceMoveIndex = defaultStripSize * strideSize * y;
					for(int x = 0; x < ((y < stripMax - 1) ? stripSize : lastSize); x++)
					{
						destinationBuffer[x] = (byte)(sourceBuffer[(x / scanLineSize) * strideSize + x % scanLineSize + sourceMoveIndex] ^ 255);
						//destinationBuffer[destinationIndex] = (byte)(sourceBuffer[sourceIndex] ^ 255);
						//						destinationBuffer[destinationIndex] = (byte)(destinationBuffer[destinationIndex] ^ ((sourceBuffer[sourceIndex]) ^ 255));
					}
					int ret = 0;
					try
					{
						ret = extLT.TIFWriteEncodedStrip(tifr, (uint)y, bHandle.AddrOfPinnedObject(), ((y == stripMax - 1) ? lastSize : destinationBuffer.Length));
					}
					catch(Exception ex)
					{
						sourceBuffer = null;
						destinationBuffer = null;
						throw new Exception("Неудачное сохранение рисунка", ex);
					}

					if(ret <= 0)
					{
						if(bHandle.IsAllocated)
							bHandle.Free();
						if(bd != null)
						{
							img.UnlockBits(bd);
							bd = null;
						}
						sourceBuffer = null;
						destinationBuffer = null;
						throw new Exception("Неудачное сохранение рисунка");
					}
				}
				if(bHandle.IsAllocated)
					bHandle.Free();
				if(bd != null)
				{
					img.UnlockBits(bd);
					bd = null;
				}
				sourceBuffer = null;
				destinationBuffer = null;
			}
			else if(img.PixelFormat == PixelFormat.Format8bppIndexed)
			{
				if(photometric != PHOTOMETRIC_MINISBLACK)
				{
					ushort[] red = new ushort[256];
					ushort[] green = new ushort[256];
					ushort[] blue = new ushort[256];

					for(int c = 0, n = 0; c < img.Palette.Entries.Length; c++, n++)
					{
						red[n] = (ushort)(img.Palette.Entries[c].R << 8);
						green[n] = (ushort)(img.Palette.Entries[c].G << 8);
						blue[n] = (ushort)(img.Palette.Entries[c].B << 8);
					}

					extLT.TIFSetField(tifr, TIFFTAG_COLORMAP, red, green, blue);
				}
				BitmapData bd = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
				int cnt = img.Height * bd.Stride;
				//IntPtr bufPtr = Marshal.AllocCoTaskMem(cnt);
				//extLT._TIFmemcpy(bufPtr, bd.Scan0, cnt);
				byte[] sourceBuffer = new byte[cnt];
				Marshal.Copy(bd.Scan0, sourceBuffer, 0, cnt);
				//Marshal.FreeCoTaskMem(bufPtr);

				int scanLineSize = extLT.TIFScanlineSize(tifr);

				byte[] destinationBuffer = new byte[scanLineSize];
				int sourceIndex = 0;
				int height = bd.Height;
				int width = bd.Width;

				GCHandle bHandle;
				IntPtr bufPtrWrite;
				for(int y = 0; y < height; y++)
				{
					sourceIndex = y * bd.Stride;
					Buffer.BlockCopy(sourceBuffer, sourceIndex, destinationBuffer, 0, scanLineSize);

					bHandle = GCHandle.Alloc(destinationBuffer, GCHandleType.Pinned);
					bufPtrWrite = bHandle.AddrOfPinnedObject();
					int result = 0;
					try
					{
						result = extLT.TIFWriteScanline(tifr, bufPtrWrite, (uint)y, 0);
					}
					catch(Exception ex)
					{
						throw new Exception("Неудачное сохранение рисунка", ex);
					}
					finally
					{
						if(bHandle.IsAllocated)
							bHandle.Free();
					}
					if(result == -1)
						throw new Exception("Неудачное сохранение рисунка");
				}
				if(bd != null)
				{
					img.UnlockBits(bd);
					bd = null;
				}
			}
			else if(img.PixelFormat == PixelFormat.Format32bppArgb || img.PixelFormat == PixelFormat.Format24bppRgb)
			{
				int sampleSource = img.PixelFormat == PixelFormat.Format32bppArgb ? 4 : 3;

				int stripSize = extLT.TIFStripSize(tifr);
				int stripMax = extLT.TIFNumberOfStrips(tifr);
				int scanLineSize = extLT.TIFScanlineSize(tifr);
				int rowsperstrip = stripSize / scanLineSize;
				int height = img.Height;
				int width = img.Width;
				BitmapData bd = null;
				//if (img.PixelFormat == PixelFormat.Format32bppArgb)
				bd = img.LockBits(new Rectangle(0, 0, img.Width, defaultStripSize), ImageLockMode.ReadWrite, img.PixelFormat);
				//else
				//    bd = img.LockBits(new Rectangle(0, 0, img.Width, defaultStripSize), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
				int strideSize = bd.Stride;
				byte[] b = new byte[strideSize * defaultStripSize];
				byte[] buffer = new byte[stripSize];
				int lastrows = height % defaultStripSize;
				int lastSize = defaultStripSize * scanLineSize;
				if(lastrows == 0)
					lastrows = defaultStripSize;
				else
					lastSize = lastrows * scanLineSize;
				int i, pos, ptr, ret, max = 0;
				GCHandle bHandle;
				for(int y = 0; y < stripMax; y++)
				{
					if(y < stripMax - 1)
					{
						if(y > 0)
							//if (img.PixelFormat == PixelFormat.Format32bppArgb)
							bd = img.LockBits(new Rectangle(0, y * defaultStripSize, img.Width, defaultStripSize), ImageLockMode.ReadWrite, img.PixelFormat);
						//else
						//    bd = img.LockBits(new Rectangle(0, y * defaultStripSize, img.Width, defaultStripSize), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
						Marshal.Copy(bd.Scan0, b, 0, b.Length);
						max = width * defaultStripSize;
					}
					else
					{
						if(y > 0)
							//if (img.PixelFormat == PixelFormat.Format32bppArgb)
							bd = img.LockBits(new Rectangle(0, y * defaultStripSize, img.Width, lastrows), ImageLockMode.ReadWrite, img.PixelFormat);
						//else
						//    bd = img.LockBits(new Rectangle(0, y * defaultStripSize, img.Width, lastrows), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
						int s = lastrows * strideSize;
						Marshal.Copy(bd.Scan0, b, 0, s);
						max = lastrows * width;
					}
					pos = 0;
					ptr = 0;
					for(i = 0; i < max; i++, pos += 3)
					{
						ptr = (i * 3 / scanLineSize) * (strideSize) + (i % (scanLineSize / 3)) * sampleSource;
						buffer[pos] = b[ptr + 2];
						buffer[pos + 1] = b[ptr + 1];
						buffer[pos + 2] = b[ptr];
					}
					bHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
					try
					{
						ret = extLT.TIFWriteEncodedStrip(tifr, (uint)y, bHandle.AddrOfPinnedObject(), ((y == stripMax - 1) ? lastSize : buffer.Length));
					}
					catch(Exception ex)
					{
						b = null;
						buffer = null;
						throw new Exception("Неудачное сохранение рисунка", ex);
					}
					finally
					{
						if(bHandle.IsAllocated)
							bHandle.Free();
						if(bd != null)
						{
							img.UnlockBits(bd);
							bd = null;
						}
					}
					if(ret <= 0)
					{
						b = null;
						buffer = null;
						throw new Exception("Неудачное сохранение рисунка");
					}
				}
				b = null;
				buffer = null;
			}
			return true;
		}

		/// <summary>
		/// Получает массив из трех элементов: картинки и заметки, из текущей директории.
		/// </summary>
		public PageInfo GetImageFromTiff(IntPtr tifr, int page)
		{

			if(tifr == IntPtr.Zero)
				return new PageInfo();
			Bitmap arr = null;
			Int32 w = 0;
			Int32 h = 0;
			int samples = 3;
			int bits = 8;
			int planar = 0;
			int photometric = 0;
			int compression = 0;
			int rowsperstrip = 0;
			float resx = 0;
			float resy = 0;
			int resunit = 0;
			int jpegQuality = 0;
			uint jpegXQuality = 0;
			uint jpegYQuality = 0;
			int predictor = 1;
			byte[] bufferAnnotationTiff = null;

			lock(lockobject)
			{
				extLT.TIFSetDirectory(tifr, (ushort)page);
				extLT.TIFGetField(tifr, TIFFTAG_IMAGEWIDTH, ref w);
				extLT.TIFGetField(tifr, TIFFTAG_IMAGELENGTH, ref h);
				if(w <= 0 || h <= 0)
				{
					WriteToLog("некорректные значения ширины и высоты\n" + w.ToString() + " " + h.ToString() + " " + page.ToString());
					return new PageInfo();
				}
				extLT.TIFGetField(tifr, TIFFTAG_COMPRESSION, ref compression);
				extLT.TIFGetField(tifr, TIFFTAG_PLANARCONFIG, ref planar);
				extLT.TIFGetField(tifr, TIFFTAG_PHOTOMETRIC, ref photometric);
				extLT.TIFGetField(tifr, TIFFTAG_BITSPERSAMPLE, ref bits);
				extLT.TIFGetField(tifr, TIFFTAG_SAMPLESPERPIXEL, ref samples);
				extLT.TIFGetField(tifr, TIFFTAG_ROWSPERSTRIP, ref rowsperstrip);
				extLT.TIFGetField(tifr, TIFFTAG_XRESOLUTION, ref resx);
				extLT.TIFGetField(tifr, TIFFTAG_YRESOLUTION, ref resy);
				extLT.TIFGetField(tifr, TIFFTAG_RESOLUTIONUNIT, ref resunit);
				if(compression == COMPRESSION_JPEG || compression == COMPRESSION_OJPEG)
				{

					extLT.TIFGetField(tifr, TIFFTAG_JPEGQUALITY, ref jpegQuality);
					extLT.TIFGetField(tifr, TIFFTAG_YCBCRSUBSAMPLING, ref jpegXQuality, ref jpegYQuality);
				}
				else if(compression == COMPRESSION_LZW || compression == COMPRESSION_ADOBE_DEFLATE || compression == COMPRESSION_DEFLATE)
				{
					extLT.TIFGetField(tifr, TIFFTAG_PREDICTOR, ref predictor);
				}

				/////Проверить таг аннотации 

				IntPtr ptrTag = extLT.TIFFindField(tifr, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
				if(ptrTag != IntPtr.Zero)
				{
					Marshal.WriteInt16(ptrTag, 4, -3);
					Marshal.WriteInt16(ptrTag, 6, -3);
					uint sizeAnnotationTiff = 0;
					IntPtr ptrBufferAnnotationTif = IntPtr.Zero;
					if(extLT.TIFGetField(tifr, TIFFTAG_ANNOTATION, ref sizeAnnotationTiff, ref ptrBufferAnnotationTif) > 0)
					{
						bufferAnnotationTiff = new byte[sizeAnnotationTiff];
						GCHandle hb = GCHandle.Alloc(bufferAnnotationTiff, GCHandleType.Pinned);
						Marshal.Copy(ptrBufferAnnotationTif, bufferAnnotationTiff, 0, (int)sizeAnnotationTiff);
						hb.Free();
					}
				}


				if(compression == 2 | compression == 3 | compression == 4 | (bits == 1 && samples == 1))//1bpp
				{
					arr = new Bitmap(w, h, PixelFormat.Format1bppIndexed);
					if(resx <= 0)
						resx = arr.HorizontalResolution;
					if(resy <= 0)
						resy = arr.VerticalResolution;
					arr.SetResolution(resx, resy);
					if(extLT.TIFIsTiled(tifr) > 0)
					{
					}
					else
					{
						int stripSize = extLT.TIFStripSize(tifr);
						int stripMax = extLT.TIFNumberOfStrips(tifr);
						int scanlineSize = extLT.TIFScanlineSize(tifr);
						if(rowsperstrip < 1 || rowsperstrip > h)
							rowsperstrip = stripSize / scanlineSize;
						if(stripSize < scanlineSize * rowsperstrip)
							stripSize = scanlineSize * rowsperstrip;
						byte[] buf = new byte[stripSize];
						GCHandle handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
						IntPtr bufPtr = handle.AddrOfPinnedObject();
						int result = 0;
						int minisColor = photometric == PHOTOMETRIC_MINISBLACK ? 0 : 255;
						BitmapData bd = arr.LockBits(new Rectangle(0, 0, w, rowsperstrip), ImageLockMode.WriteOnly, arr.PixelFormat);
						byte[] destinationBuffer = new byte[bd.Stride * rowsperstrip];
						int lastSize = bd.Stride * (h % rowsperstrip);
						if(lastSize == 0)
							lastSize = destinationBuffer.Length;
						for(int stripNum = 0; stripNum < stripMax; stripNum++)
						{
							try
							{
								if(stripNum > 0)
									if(stripNum < stripMax - 1 || (h % rowsperstrip) == 0)
										bd = arr.LockBits(new Rectangle(0, rowsperstrip * stripNum, w, rowsperstrip), ImageLockMode.WriteOnly, arr.PixelFormat);
									else
										bd = arr.LockBits(new Rectangle(0, rowsperstrip * stripNum, w, h % rowsperstrip), ImageLockMode.WriteOnly, arr.PixelFormat);
								result = extLT.TIFReadEncodedStrip(tifr, (uint)stripNum, bufPtr, stripSize);
								if(result == -1)
									break;
								for(int z = 0; z < rowsperstrip; z++)
									for(int x = 0; x < scanlineSize; x++)
									{
										try
										{
											destinationBuffer[x + bd.Stride * z] = (byte)(buf[x + scanlineSize * z] ^ minisColor);
											buf[x + scanlineSize * z] = 0;
										}
										catch(Exception ex)
										{
											buf = null;
											destinationBuffer = null;
											throw new Exception("Ошибка чтения", ex);
										}
									}
								Marshal.Copy(destinationBuffer, 0, bd.Scan0, (stripNum == stripMax - 1) ? lastSize : destinationBuffer.Length);
								if(bd != null)
								{
									arr.UnlockBits(bd);
									bd = null;
								}
							}
							catch(Exception ex)
							{
								if(bd != null)
								{
									arr.UnlockBits(bd);
									bd = null;
								}
								if(handle.IsAllocated)
									handle.Free();
								buf = null;
								destinationBuffer = null;
								throw new Exception("Ошибка чтения", ex);
							}
						}
						if(bd != null)
							arr.UnlockBits(bd);
						if(handle.IsAllocated)
							handle.Free();
						buf = null;
						destinationBuffer = null;
					}
				}
				else if(bits == 8 && samples == 1)
				{
					arr = new Bitmap(w, h, PixelFormat.Format8bppIndexed);
					if(resx <= 0)
						resx = arr.HorizontalResolution;
					if(resy <= 0)
						resy = arr.VerticalResolution;
					arr.SetResolution(resx, resy);

					ColorPalette palette = arr.Palette;
					if(photometric != PHOTOMETRIC_PALETTE)
					{
						for(int p = 0; p < 256; p++)
						{
							palette.Entries[p] = Color.FromArgb(255, p, p, p);
						}
						arr.Palette = palette;
					}
					else
					{
						ushort[] redg = new ushort[256];
						ushort[] greeng = new ushort[256];
						ushort[] blueg = new ushort[256];
						short[] tmparr = new short[256];
						IntPtr rptr = IntPtr.Zero;
						IntPtr gptr = IntPtr.Zero;
						IntPtr bptr = IntPtr.Zero;
						int res = extLT.TIFGetField(tifr, TIFFTAG_COLORMAP, ref rptr, ref  gptr, ref bptr);
						if(res == 1)
						{
							Marshal.Copy(rptr, tmparr, 0, 256);
							redg = Array.ConvertAll<short, ushort>(tmparr, new Converter<short, ushort>(ShortToUshort));
							Marshal.Copy(gptr, tmparr, 0, 256);
							greeng = Array.ConvertAll<short, ushort>(tmparr, new Converter<short, ushort>(ShortToUshort));
							Marshal.Copy(bptr, tmparr, 0, 256);
							blueg = Array.ConvertAll<short, ushort>(tmparr, new Converter<short, ushort>(ShortToUshort));
							for(int p = 0; p < 256; p++)
							{
								palette.Entries[p] = Color.FromArgb(redg[p] >> 8, greeng[p] >> 8, blueg[p] >> 8);
							}
							arr.Palette = palette;
						}
					}
					int stripSize = extLT.TIFStripSize(tifr);
					int stripMax = extLT.TIFNumberOfStrips(tifr);
					int scanlineSize = extLT.TIFScanlineSize(tifr);
					int bufferSize = h * scanlineSize;
					if(rowsperstrip < 1 || rowsperstrip > h)
						rowsperstrip = stripSize / scanlineSize;
					if(stripSize < scanlineSize * rowsperstrip)
						stripSize = scanlineSize * rowsperstrip;
					byte[] sourceBuffer = new byte[stripSize];
					GCHandle handle = GCHandle.Alloc(sourceBuffer, GCHandleType.Pinned);
					IntPtr bufPtr = handle.AddrOfPinnedObject();
					int result = 0;
					int minisColor = photometric == PHOTOMETRIC_MINISBLACK ? 0 : 255;
					BitmapData bd = arr.LockBits(new Rectangle(0, 0, w, rowsperstrip), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
					byte[] destinationBuffer = new byte[bd.Stride * rowsperstrip];
					int lastrows = h % rowsperstrip;
					int lastSize = destinationBuffer.Length;
					if(lastrows == 0)
						lastrows = rowsperstrip;
					else
						lastSize = bd.Stride * lastrows;
					for(int y = 0; y < stripMax; y++)
					{
						try
						{
							if(y > 0)
								if(y < stripMax - 1)
									bd = arr.LockBits(new Rectangle(0, y * rowsperstrip, w, rowsperstrip), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
								else
									bd = arr.LockBits(new Rectangle(0, y * rowsperstrip, w, lastrows), ImageLockMode.WriteOnly, PixelFormat.Format8bppIndexed);
							result = extLT.TIFReadEncodedStrip(tifr, (uint)y, bufPtr, stripSize);
							if(result == -1)
							{
								if(bd != null)
								{
									arr.UnlockBits(bd);
									bd = null;
								}
								if(handle.IsAllocated)
									handle.Free();
								sourceBuffer = null;
								destinationBuffer = null;
								throw new Exception("Ошибка чтения");
							}
							for(int z = 0; z < stripSize / scanlineSize; z++)
								for(int x = 0; x < scanlineSize; x++)
								{
									if(photometric == PHOTOMETRIC_PALETTE)
									{
										destinationBuffer[x + bd.Stride * z] = sourceBuffer[x + scanlineSize * z];
									}
									else
										destinationBuffer[x + bd.Stride * z] = (byte)(sourceBuffer[x + scanlineSize * z] ^ minisColor);
									sourceBuffer[x + scanlineSize * z] = 0;
								}
							Marshal.Copy(destinationBuffer, 0, bd.Scan0, (y == stripMax - 1) ? lastSize : destinationBuffer.Length);
							if(bd != null)
							{
								arr.UnlockBits(bd);
								bd = null;
							}
						}
						catch(Exception ex)
						{
							if(bd != null)
							{
								arr.UnlockBits(bd);
								bd = null;
							}
							if(handle.IsAllocated)
								handle.Free();
							sourceBuffer = null;
							destinationBuffer = null;
							throw new Exception("Ошибка чтения", ex);
						}
					}
					if(bd != null)
						arr.UnlockBits(bd);
					if(handle.IsAllocated)
						handle.Free();
					bd = null;
					destinationBuffer = null;
					sourceBuffer = null;
				}
				else
				{
					if(samples == 3)
						arr = new Bitmap(w, h, PixelFormat.Format24bppRgb);
					else
						arr = new Bitmap(w, h);
					RotateFlipType flip = RotateFlipType.RotateNoneFlipNone;
					bool backorder = false;
					bool compressionjpeg = compression == COMPRESSION_JBIG || compression == COMPRESSION_JPEG || compression == COMPRESSION_JP2000 || compression == COMPRESSION_OJPEG;
					if(resx <= 0)
						resx = arr.HorizontalResolution;
					if(resy <= 0)
						resy = arr.VerticalResolution;
					arr.SetResolution(resx, resy);
					int orientation = (int)ORIENTATION_TOPLEFT;
					extLT.TIFGetField(tifr, TIFFTAG_ORIENTATION, ref orientation);
					//if (orientation != ORIENTATION_TOPLEFT)
					//    throw new Exception("Не стандартная ориентация\nВывод не поддерживается");
					switch(orientation)
					{
						case ORIENTATION_BOTRIGHT:
							flip = RotateFlipType.RotateNoneFlipY;
							backorder = true;
							break;
						case ORIENTATION_BOTLEFT:
							backorder = true;
							flip = RotateFlipType.RotateNoneFlipY;
							break;
						case ORIENTATION_LEFTBOT:
							backorder = true;
							flip = RotateFlipType.Rotate90FlipNone;
							break;
						case ORIENTATION_LEFTTOP:
							flip = RotateFlipType.Rotate90FlipNone;
							break;
						case ORIENTATION_RIGHTTOP:
							flip = RotateFlipType.Rotate90FlipNone;
							break;
						case ORIENTATION_RIGHTBOT:
							backorder = true;
							flip = RotateFlipType.Rotate90FlipNone;
							break;
						case ORIENTATION_TOPRIGHT:
						case ORIENTATION_TOPLEFT:
						default:
							break;
					}
					int scanlineSize = w * 3;
					if(compressionjpeg || samples == 4)
						scanlineSize += w;
					if(rowsperstrip < 1 || rowsperstrip > h)
						rowsperstrip = h;
					int stripSize = scanlineSize * rowsperstrip;//extLT.TIFFStripSize(tifr);
					int stripMax = extLT.TIFNumberOfStrips(tifr);
					int samp = (arr.PixelFormat == PixelFormat.Format32bppArgb) ? 4 : 3;
					byte[] buf = new byte[stripSize];
					GCHandle handle = GCHandle.Alloc(buf, GCHandleType.Pinned);
					IntPtr bufPtr = handle.AddrOfPinnedObject();
					int result = 0;
					BitmapData bd = arr.LockBits(new Rectangle(0, 0, w, rowsperstrip), ImageLockMode.WriteOnly, arr.PixelFormat);
					byte[] destinationBuffer = new byte[bd.Stride * rowsperstrip];
					int lastrows = h % rowsperstrip;
					int lastSize = destinationBuffer.Length;
					int lastStripSize = stripSize;
					if(lastrows == 0)
						lastrows = rowsperstrip;
					else
					{
						lastSize = bd.Stride * lastrows;
						lastStripSize = lastrows * scanlineSize;
					}
					int pos, ptra, x, z, max = 0;
					for(int stripCount = 0; stripCount < stripMax; stripCount++)
					{
						if(stripCount > 0)
							if(stripCount < stripMax - 1)
								bd = arr.LockBits(new Rectangle(0, stripCount * rowsperstrip, w, rowsperstrip), ImageLockMode.WriteOnly, arr.PixelFormat);
							else
								bd = arr.LockBits(new Rectangle(0, stripCount * rowsperstrip, w, lastrows), ImageLockMode.WriteOnly, arr.PixelFormat);
						try
						{
							if(compressionjpeg)
							{
								result = extLT.TIFReadRGBAStrip(tifr, (uint)(stripCount * rowsperstrip), bufPtr);
								if(result == -1)
								{
									break;
								}
								if(stripCount == stripMax - 1)
								{
									max = h % rowsperstrip;
									if(max < 1)
										max = rowsperstrip;
								}
								else
									max = rowsperstrip;
								for(z = 0; z < max; z++)
								{
									if(backorder)
										pos = scanlineSize * z;
									else
										pos = scanlineSize * (max - z - 1);
									ptra = bd.Stride * z;
									for(x = 0; x < w; x++, pos += 4, ptra += samples)
									{
										destinationBuffer[ptra] = buf[pos + 2];
										destinationBuffer[ptra + 1] = buf[pos + 1];
										destinationBuffer[ptra + 2] = buf[pos];
										if(samples != 3)
											destinationBuffer[ptra + 3] = buf[pos + 3];
									}
								}
								Marshal.Copy(destinationBuffer, 0, bd.Scan0, (stripCount == stripMax - 1) ? lastSize : destinationBuffer.Length);
							}
							else
							{
								if(stripCount == stripMax - 1)
								{
									result = extLT.TIFReadEncodedStrip(tifr, (uint)stripCount, bufPtr, lastStripSize);

									max = h % rowsperstrip;
									if(max < 1)
										max = rowsperstrip;
								}
								else
								{
									result = extLT.TIFReadEncodedStrip(tifr, (uint)stripCount, bufPtr, stripSize);
									max = rowsperstrip;
								}
								if(result == -1)
								{
									break;
								}

								for(z = 0; z < max; z++)
								{
									if(backorder)
										pos = scanlineSize * (max - z - 1);
									else
										pos = scanlineSize * z;
									ptra = bd.Stride * z;
									for(x = 0; x < w; x++, pos += samples, ptra += samples)
									{

										destinationBuffer[ptra] = buf[pos + 2];
										destinationBuffer[ptra + 1] = buf[pos + 1];
										destinationBuffer[ptra + 2] = buf[pos];
										if(samples != 3)
											destinationBuffer[ptra + 3] = buf[pos+3];
									}
								}
								Marshal.Copy(destinationBuffer, 0, bd.Scan0, (stripCount == stripMax - 1) ? lastSize : destinationBuffer.Length);
							}
							if(bd != null)
							{
								arr.UnlockBits(bd);
								bd = null;
							}
						}
						catch(Exception ex)
						{
							if(bd != null)
								arr.UnlockBits(bd);
							if(handle.IsAllocated)
								handle.Free();
							buf = null;
							destinationBuffer = null;
							throw new Exception("Ошибка чтения", ex);
						}
					}
					if(bd != null)
						arr.UnlockBits(bd);
					if(handle.IsAllocated)
						handle.Free();
					buf = null;
					destinationBuffer = null;
					if(flip != RotateFlipType.RotateNoneFlipNone)
						arr.RotateFlip(flip);
				}
			}
			PageInfo pageInfo = new PageInfo();
			pageInfo.Image = arr;
			pageInfo.Annotation = bufferAnnotationTiff;
			CompressionInfo comp = new CompressionInfo();
			comp.CompressionType = (CompressionType)compression;
			if(comp.CompressionType == CompressionType.COMPRESSION_JPEG)
			{
				comp.JpegQuality = jpegQuality;
				comp.JpegXSubSampling = (SubSampling)jpegXQuality;
				comp.JpegYSubSampling = (SubSampling)jpegYQuality;
			}
			pageInfo.Compression = comp;

			return pageInfo;
		}

		/// <summary>
		/// Получаем данные из Tiff не декодируя их.
		/// </summary>
		/// <param name="tifr"></param>
		/// <param name="i"></param>
		/// <returns></returns>
		private List<byte[]> GetPageFromTiff(IntPtr tifr, int page)
		{

			byte[] arr = null;
			Int32 w = 0;
			Int32 h = 0;
			int samples = 3;
			int bits = 8;
			int planar = 0;
			int photometric = 0;
			int compression = 0;
			int rowsperstrip = 0;
			float resx = 0;
			float resy = 0;
			int resunit = 0;
			int jpegQuality = 0;
			uint jpegXQuality = 0;
			uint jpegYQuality = 0;
			int predictor = 1;
			extLT.TIFSetDirectory(tifr, (ushort)page);
			extLT.TIFGetField(tifr, TIFFTAG_IMAGEWIDTH, ref w);
			extLT.TIFGetField(tifr, TIFFTAG_IMAGELENGTH, ref h);
			if(w <= 0 || h <= 0)
			{
				WriteToLog("некорректные значения ширины и высоты\n" + w.ToString() + " " + h.ToString() + " " + page.ToString());
				return null;
			}
			extLT.TIFGetField(tifr, TIFFTAG_COMPRESSION, ref compression);
			extLT.TIFGetField(tifr, TIFFTAG_PLANARCONFIG, ref planar);
			extLT.TIFGetField(tifr, TIFFTAG_PHOTOMETRIC, ref photometric);
			extLT.TIFGetField(tifr, TIFFTAG_BITSPERSAMPLE, ref bits);
			extLT.TIFGetField(tifr, TIFFTAG_SAMPLESPERPIXEL, ref samples);
			extLT.TIFGetField(tifr, TIFFTAG_ROWSPERSTRIP, ref rowsperstrip);
			extLT.TIFGetField(tifr, TIFFTAG_XRESOLUTION, ref resx);
			extLT.TIFGetField(tifr, TIFFTAG_YRESOLUTION, ref resy);
			extLT.TIFGetField(tifr, TIFFTAG_RESOLUTIONUNIT, ref resunit);
			if(compression == COMPRESSION_JPEG || compression == COMPRESSION_OJPEG)
			{

				extLT.TIFGetField(tifr, TIFFTAG_JPEGQUALITY, ref jpegQuality);
				extLT.TIFGetField(tifr, TIFFTAG_YCBCRSUBSAMPLING, ref jpegXQuality, ref jpegYQuality);
			}
			else if(compression == COMPRESSION_LZW || compression == COMPRESSION_ADOBE_DEFLATE || compression == COMPRESSION_DEFLATE)
			{
				extLT.TIFGetField(tifr, TIFFTAG_PREDICTOR, ref predictor);
			}
			byte[] bufferAnnotationTiff = null;

			/////Проверить таг аннотации 

			IntPtr ptrTag = extLT.TIFFindField(tifr, TIFFTAG_ANNOTATION, TIFFDataType.TIFF_BYTE);
			if(ptrTag != IntPtr.Zero)
			{
				//изменяем размеры тага
				Marshal.WriteInt16(ptrTag, 4, -3);
				Marshal.WriteInt16(ptrTag, 6, -3);

				uint sizeAnnotationTiff = 0;
				IntPtr ptrBufferAnnotationTif = IntPtr.Zero;
				if(extLT.TIFGetField(tifr, TIFFTAG_ANNOTATION, ref sizeAnnotationTiff, ref ptrBufferAnnotationTif) > 0)
				{
					bufferAnnotationTiff = new byte[sizeAnnotationTiff];
					GCHandle hb = GCHandle.Alloc(bufferAnnotationTiff, GCHandleType.Pinned);
					Marshal.Copy(ptrBufferAnnotationTif, bufferAnnotationTiff, 0, (int)sizeAnnotationTiff);
					hb.Free();
				}
			}


			int stripSize = extLT.TIFStripSize(tifr);
			int stripMax = extLT.TIFNumberOfStrips(tifr);
			int scanlineSize = extLT.TIFScanlineSize(tifr);
			if(rowsperstrip < 1 || rowsperstrip > h)
				rowsperstrip = stripSize / scanlineSize;
			if(stripSize < scanlineSize * rowsperstrip)
				stripSize = scanlineSize * rowsperstrip;
			arr = new byte[scanlineSize * h];
			GCHandle handle = GCHandle.Alloc(arr, GCHandleType.Pinned);
			IntPtr bufPtr = handle.AddrOfPinnedObject();
			int result = 0;
			int minisColor = photometric == PHOTOMETRIC_MINISBLACK ? 0 : 255;
			for(int stripCount = 0; stripCount < stripMax; stripCount++)
			{
				try
				{
					result = extLT.TIFReadRawStrip(tifr, (uint)stripCount, bufPtr, stripSize);
					//result = extLT.TIFReadRawTile(tifr, (uint)stripCount, bufPtr, stripSize);
					if(result == -1)
						break;
				}
				catch(Exception ex)
				{
					if(handle.IsAllocated)
						handle.Free();
					throw new Exception("Ошибка чтения", ex);
				}
			}
			if(handle.IsAllocated)
				handle.Free();
			CompressionInfo comp = new CompressionInfo();
			comp.CompressionType = (CompressionType)compression;
			if(comp.CompressionType == CompressionType.COMPRESSION_JPEG)
			{
				comp.JpegQuality = jpegQuality;
				comp.JpegXSubSampling = (SubSampling)jpegXQuality;
				comp.JpegYSubSampling = (SubSampling)jpegYQuality;
			}
			CompressionInfo[] cc = new CompressionInfo[1] { comp };
			byte[] cb = new byte[20];
			Buffer.BlockCopy(cc, 0, cb, 0, cb.Length);
			List<byte[]> lst = new List<byte[]>() { arr, bufferAnnotationTiff, cb };
			return lst;
		}

		static ushort ShortToUshort(short input)
		{
			if(input < 0)
				return (ushort)(ushort.MaxValue + input + 1);
			else
				return (ushort)input;
		}

		public static bool IsColorPixelFormat(PixelFormat pixelFormap)
		{
			return pixelFormap == PixelFormat.Format24bppRgb || pixelFormap == PixelFormat.Format32bppArgb || pixelFormap == PixelFormat.Format8bppIndexed || pixelFormap == PixelFormat.Format4bppIndexed;
		}

		public static bool IsGrayScale(Image image)
		{
			if(image.PixelFormat != PixelFormat.Format4bppIndexed && image.PixelFormat != PixelFormat.Format8bppIndexed)
				return false;

			for(int i = 0; i < image.Palette.Entries.Length; i++)
				if(image.Palette.Entries[i].B != image.Palette.Entries[i].R || image.Palette.Entries[i].G != image.Palette.Entries[i].R || image.Palette.Entries[i].B != image.Palette.Entries[i].G)
					return false;

			return true;
		}

		public bool VerifyIsOneColorPageInFile(string fileName)
		{
			if(fileName == null || fileName == "")
				return false;
			Bitmap bitmapAnotherFormat = null;
			IntPtr tifr = TiffOpenRead(ref fileName, out bitmapAnotherFormat, false);
			Boolean isColor = false;

			try
			{
				if(bitmapAnotherFormat != null)
					return IsColorPixelFormat(bitmapAnotherFormat.PixelFormat);
				if(IntPtr.Zero != tifr)
				{
					int numPages = extLT.TIFNumberOfDirectories(tifr);
					for(int i = 0; i < numPages; i++)
					{
						try
						{
							int samples = 0;
							extLT.TIFGetField(tifr, TIFFTAG_SAMPLESPERPIXEL, ref samples);
							if(samples >= 3)
								return true;
						}
						catch(Exception ex)
						{
							WriteToLog(ex);
						}
					}
				}
			}
			finally
			{
				if(bitmapAnotherFormat != null)
					bitmapAnotherFormat.Dispose();
				if(tifr != IntPtr.Zero)
					TiffCloseRead(ref tifr);
			}
			return isColor;
		}

		public static bool IsColorYCbCr(Bitmap bmp)
		{
			int i, j, ptr;
			BitmapData bd = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
			try
			{
				Int32 size = (bmp.Width * bmp.Height) * 4;
				byte[] buf = new byte[size];
				Marshal.Copy(bd.Scan0, buf, 0, buf.Length);

				for(j = 0, ptr = 0; j < bmp.Height; j++)
				{
					for(i = 0; i < bmp.Width; i++, ptr += 4)
						if((buf[ptr] != 0 && buf[ptr] != 255) || (buf[ptr + 1] != 0 && buf[ptr + 1] != 255) || (buf[ptr + 2] != 0 && buf[ptr + 2] != 255))
							return true;
				}

			}
			finally
			{
				bmp.UnlockBits(bd);
			}
			return false;
		}

		/// <summary>
		/// Удаление страницы из тифа, предварительно тиф считывается в буфер
		/// </summary>
		public void DeletePage(string fileName, int page)
		{
			Bitmap bitmapAnotherFormat = null;
			lock(lockobject)
			{
				IntPtr tifr = TiffOpenRead(ref fileName, out bitmapAnotherFormat, true);
				ArrayList list = new ArrayList();
				if(IntPtr.Zero != tifr)
				{

					int numPages = extLT.TIFNumberOfDirectories(tifr);

					for(int i = 0; i < numPages; i++)
					{
						if(page == i)
							continue;
						list.Add(GetImageFromTiff(tifr, i));
					}
				}
				TiffCloseRead(ref tifr);

				IntPtr tifw = extLT.TIFOpenW(fileName, "w");
				if(IntPtr.Zero != tifw && list.Count > 0)
				{
					for(int i = 0; i < list.Count; i++)
					{
						VerifyColorSetImageToTiff(tifw, (Bitmap)((object[])list[i])[0], (byte[])((object[])list[i])[1], false);
						extLT.TIFWriteDirectory(tifw);
					}
					TiffCloseWrite(ref tifw);
				}
			}
		}

		#region IDisposable Members

		public void Dispose()
		{
			if(openTif != IntPtr.Zero)
			{
				IntPtr i = IntPtr.Zero;
				TiffCloseRead(ref i);
			}
		}

		#endregion

		/// <summary>
		/// Функция для отправки сообщений и ошибок в лог
		/// </summary>
		/// <param name="ex">Ошибка</param>
		/// <param name="ad">Дополнительная информация</param>
		public static void WriteToLog(Exception ex, string ad)
		{
			if(string.IsNullOrEmpty(ad))
				Log.Logger.WriteEx(ex);
			else
				if(ex == null)
					Log.Logger.WriteEx(new Log.LogicalException(ad, null, System.Reflection.Assembly.GetCallingAssembly().GetName()));
				else
					Log.Logger.WriteEx(new Exception(ad, ex));
		}

		/// <summary>
		/// Запись данных в лог
		/// </summary>
		/// <param name="ex">Ошибка для записи</param>
		public static void WriteToLog(Exception ex)
		{
			WriteToLog(ex, null);
		}

		/// <summary>
		/// Запись данных в лог
		/// </summary>
		/// <param name="ad">Дополнительная информация</param>
		public static void WriteToLog(string ad)
		{
			WriteToLog(null, ad);
		}

		public void AppendPages(string filename, List<PageInfo> images)
		{
			IntPtr tifw = extLT.TIFOpenW(filename, "a");
			if(IntPtr.Zero != tifw && images.Count > 0)
			{
				for(int i = 0; i < images.Count; i++)
				{
					SetImageToTiff(tifw, images[i].Image, images[i].Annotation);
					extLT.TIFWriteDirectory(tifw);
				}
				TiffCloseWrite(ref tifw);
			}
		}

		public void SavePagesWithAdd(string filename, List<PageInfo> images, int page)
		{
			Bitmap bitmapAnotherFormat = null;
			lock(lockobject)
			{
				IntPtr tifr = TiffOpenRead(ref filename, out bitmapAnotherFormat, true);
				List<PageInfo> list = new List<PageInfo>();
				if(IntPtr.Zero != tifr)
				{

					int numPages = extLT.TIFNumberOfDirectories(tifr);

					if(page == -1)
						list.AddRange(images);
					for(int i = 0; i < numPages; i++)
					{
						list.Add(GetImageFromTiff(tifr, i));
						if(page == i)
							list.AddRange(images);
					}
				}
				TiffCloseRead(ref tifr);

				IntPtr tifw = extLT.TIFOpenW(filename, "w");
				if(IntPtr.Zero != tifw && list.Count > 0)
				{
					for(int i = 0; i < list.Count; i++)
					{
						SetImageToTiff(tifw, list[i].Image, list[i].Annotation);
						extLT.TIFWriteDirectory(tifw);
					}
					TiffCloseWrite(ref tifw);
				}
			}
		}

		public PageInfo GetImageFromTiff(string fileName, int page)
		{
			FileInfo fi = new FileInfo(fileName);
			if(!fi.Exists)
			{
				WriteToLog("Отсутствует файл " + fileName);
				if(isUseLock && openTif != IntPtr.Zero)
				{
					extLT.TIFClose(openTif);
					openTif = IntPtr.Zero;
				}
				return new PageInfo();
			}
			if(fi.Length < 21)
			{
				WriteToLog("длина не корректна " + fi.FullName + " " + fi.Length.ToString());
                return new PageInfo();
			}

			Bitmap bitmapAnotherFormat = null;
			IntPtr tifr;
			tifr = TiffOpenRead(ref fileName, out bitmapAnotherFormat, false);
			if(tifr == IntPtr.Zero)
			{
				if(bitmapAnotherFormat != null && page == 0)
					return new PageInfo() { Image = bitmapAnotherFormat };
				else
					return new PageInfo();
			}
			else
				try
				{
					return GetImageFromTiff(tifr, page);
				}
				finally
				{
					TiffCloseWrite(ref tifr);
				}
		}

		public void SaveBitmapToFile(string fileName, Bitmap image, bool IsColorSave)
		{
			IntPtr tifw = extLT.TIFOpenW(fileName, "w");
			if(!IsColorSave)
				image = ConvertToBitonal(image);
			SetImageToTiff(tifw, image, null);
			extLT.TIFWriteDirectory(tifw);
			TiffCloseWrite(ref tifw);
		}

        /// <summary>
        /// Сохранение изображения из старого файла в новый файл с сохранением страницы изображения (Bitmap) под индексом pageNum
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="newFilename"></param>
        /// <param name="pageNum"></param>
        /// <param name="image"></param>
        /// <param name="IsColorSave"></param>
        /// <returns></returns>
		public bool SaveBitmapToFile(string fileName, string newFilename, int pageNum, PageInfo image, bool isColorSave, Dictionary<int,Tuple<int,int,bool>> movPages)
		{
			var dic =new Dictionary<int, PageInfo>();
			dic.Add(pageNum, image);
			return SaveBitmapToFile(fileName, newFilename, dic, isColorSave, movPages);
		}

	    /// <summary>
	    /// Сохранение изображения из старого файла в новый файл с сохранением страниц изображения (Bitmaps)
	    /// </summary>
	    /// <param name="fileName"></param>
	    /// <param name="newFilename"></param>
	    /// <param name="changedImages">Страницы для сохранения</param>
	    /// <param name="isColorSave"></param>
	    /// <returns></returns>
		public bool SaveBitmapToFile(string fileName, string newFilename, Dictionary<int, PageInfo> changedImages, bool isColorSave, Dictionary<int, Tuple<int,int,bool>> movPages)
		{
			Bitmap bitmapAnotherFormat = null;
			IntPtr tifr = TiffOpenRead(ref fileName, out bitmapAnotherFormat, false);
			int page = -1;
			if(movPages == null)
				movPages = new Dictionary<int,Tuple<int,int,bool>>();
			if(bitmapAnotherFormat != null)
			{
				bitmapAnotherFormat.Dispose();
				return false;
			}
			if(IntPtr.Zero != tifr)
			{
				IntPtr tifw = IntPtr.Zero;
				try
				{
					int numPages = extLT.TIFNumberOfDirectories(tifr);
					tifw = extLT.TIFOpenW(newFilename, "w");

					for(int i = 0; i < numPages; i++)
					{
						if(movPages.ContainsKey(i))
							page = movPages[i].Item1;
						else
							page = i;
						if(changedImages.ContainsKey(i))
							SetImageToTiff(tifw, changedImages[i].Image, changedImages[i].Annotation);
						else
						{
							PageInfo pi = GetImageFromTiff(tifr, page);
							SetImageToTiff(tifw, pi.Image, pi.Annotation);
							pi.Clear();
						}
						extLT.TIFWriteDirectory(tifw);
					}
				}
				finally
				{
					TiffCloseWrite(ref tifw);
					TiffCloseRead(ref tifr);
				}
			}
			return true;
		}

		public void SavePage(IntPtr tifw)
		{
			extLT.TIFWriteDirectory(tifw);
		}

		public int[] GetColorPages(string fileName, int startPage, int endPage)
		{
			if(String.IsNullOrEmpty(fileName))
				return null;
			Bitmap bitmapAnotherFormat = null;
			IntPtr tifr = TiffOpenRead(ref fileName, out bitmapAnotherFormat, false);
			Boolean isColor = false;

			try
			{
				if(bitmapAnotherFormat != null)
					if(startPage == 0)
						return new int[1] { 0 };
					else
						return null;
				if(IntPtr.Zero != tifr)
				{
					int numPages = extLT.TIFNumberOfDirectories(tifr);
					if(endPage < 0 || endPage > numPages)
					{
						extLT.TIFSetDirectory(tifr, (ushort)(numPages - 1));
						int i = extLT.TIFLastDirectory(tifr);
						if(i == 0)
							++numPages;
						if(endPage < 0 || endPage > numPages - 1)
							endPage = numPages - 1;
					}
					if(startPage < 0)
						startPage = 0;
					List<int> pages = new List<int>(endPage - startPage + 1);
					for(int i = startPage; i <= endPage; i++)
					{
						try
						{
							int samples = 0;
							extLT.TIFSetDirectory(tifr, (ushort)i);
							extLT.TIFGetField(tifr, TIFFTAG_BITSPERSAMPLE, ref samples);
							if(samples > 1)
								pages.Add(i);
						}
						catch(Exception ex)
						{
							WriteToLog(ex);
						}

					}
					if(pages.Count > 0)
						return pages.ToArray();
				}
			}
			finally
			{
				if(bitmapAnotherFormat != null)
					bitmapAnotherFormat.Dispose();
				if(tifr != IntPtr.Zero)
					TiffCloseRead(ref tifr);
			}
			return null;
		}

		/// <summary>
		/// сохранение части страниц из файла в новый файл.
		/// если файл не задан, то к файлу добавляется .tmp. При существовании данного файла копирование не осуществляется.
		/// </summary>
		/// <param name="fileName">имя исходного файла, для копирования страниц</param>
		/// <param name="startPage">начальная страница. нумерация с 0</param>
		/// <param name="length">количество страниц для копирования. если не заданно копируестя до последней страницы</param>
		/// <param name="newFileName">имя файла, в который будет сохранен результат</param>
		/// <param name="color">список страниц для копирования в цвете</param>
		/// <returns>результат копирования</returns>
		public bool SavePart(string fileName, int startPage, int length, string newFileName, List<int> color)
		{
			bool copy = false;
			bool ret = false;
			Bitmap bitmapAnotherFormat = null;
			lock(lockobject)
			{
				IntPtr tifr = TiffOpenRead(ref fileName, out bitmapAnotherFormat, true);
				if(bitmapAnotherFormat != null)
				{
					throw new Exception("Incorrect file type");
				}
				if(tifr == null)
					throw new Exception("Can't open file " + fileName);
				if(string.IsNullOrEmpty(newFileName) || fileName.Equals(newFileName, StringComparison.CurrentCultureIgnoreCase))
				{
					if(!File.Exists(fileName + ".tmp"))
					{
						newFileName = fileName + ".tmp";
						copy = true;
					}
					else
						return false;
				}
				int pageCount = extLT.TIFNumberOfDirectories(tifr);
				if(pageCount == 0)
					pageCount = 1;
				extLT.TIFSetDirectory(tifr, (ushort)(pageCount - 1));
				int i = extLT.TIFLastDirectory(tifr);
				if(i == 0)
					++pageCount;
				if(startPage < 0)
					startPage = 0;
				if(length > pageCount || length < 1)
					length = pageCount;
				Int32 w = 0;
				Int32 h = 0;
				extLT.TIFClose(tifr);

				if(color != null)
				{
					color = color.FindAll(x => x >= startPage && x <= startPage + length);
					if(color.Count < 1)
						ret = extLT.TIFCopyFile(fileName, newFileName, (ushort)startPage, (ushort)length, null);
					else
						ret = extLT.TIFCopyFile(fileName, newFileName, (ushort)startPage, (ushort)length, color.ConvertAll<UInt16>(x => (UInt16)x).ToArray());
				}
				else
					ret = extLT.TIFCopyFile(fileName, newFileName, (ushort)startPage, (ushort)length, null);
			}
			if(copy && ret)
			{
				File.Copy(newFileName, fileName, true);
				File.Delete(newFileName);
			}

			return ret;
		}

		public void Replace(string sourceFile, string addFile, string destFile, int sourcePage, int numPage, int destPage)
		{
			extLT.TIFReplacePage(sourceFile, addFile, destFile, (ushort)sourcePage, (ushort)numPage, (ushort)destPage);
		}

		public void Replace(string sourceFile, string addFile, string destFile, ushort[] sourcePages, int numPage, ushort[] destPages)
		{
			extLT.TIFReplacePage(sourceFile, addFile, destFile, sourcePages, (ushort)numPage, destPages);
		}
	}

	public class LibTiffHelperx32 : LibTiffHelperxxx
	{
		public LibTiffHelperx32() { }

		private const string libTiff = "libtiff.dll";

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
		private static extern IntPtr TIFFOpenW(string filename, [MarshalAs(UnmanagedType.AnsiBStr)]string mode);//mode w,r,a

		public IntPtr TIFOpenW(string filename, string mode)
		{
			return TIFFOpenW(filename, mode);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool TIFFCopyFile(string input, string output, ushort startPage, ushort length, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeParamIndex = 5)]ushort[] colorPages, ushort clength);

		public bool TIFCopyFile(string input, string output, ushort startPage, ushort length, ushort[] colorPages)
		{
			if(colorPages != null)
				return TIFFCopyFile(input, output, startPage, length, colorPages, (ushort)colorPages.Length);
			else
				return TIFFCopyFile(input, output, startPage, length, new ushort[0], (ushort)0);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
		private static extern bool TIFFCopyFilePages(string input, string output, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeParamIndex = 5)]ushort[ ] pages, ushort pLenght, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeParamIndex = 5)]ushort[ ] colorPages, ushort clength, ushort compression);

		public bool TIFCopyFile(string input, string output, ushort[ ] pages, ushort[ ] colorPages, int compression)
		{
			if(pages.Length < 1)
				return false;
			if(colorPages != null)
				return TIFFCopyFilePages(input, output, pages, (ushort)pages.Length, colorPages, (ushort)colorPages.Length, (ushort)compression);
			else
				return TIFFCopyFilePages(input, output, pages, (ushort)pages.Length, new ushort[0], (ushort)0, (ushort)compression);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool TIFFCopyFileComp(string input, string output, ushort startPage, ushort length, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeParamIndex = 5)]ushort[ ] colorPages, ushort clength, ushort compression);

		public bool TIFCopyFile(string input, string output, ushort startPage, ushort length, ushort[ ] colorPages, int compression)
		{
			if(colorPages != null)
				return TIFFCopyFileComp(input, output, startPage, length, colorPages, (ushort)colorPages.Length, (ushort)compression);
			else
				return TIFFCopyFileComp(input, output, startPage, length, new ushort[0], (ushort)0, (ushort)compression);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool TIFFReplacePage(string baseFile, string addFile, string output, ushort[] basePage, ushort baseCount, ushort length, ushort[] replacePage);

		public bool TIFReplacePage(string baseFile, string addFile, string output, ushort basePage, ushort length, ushort replacePage)
		{
			return TIFFReplacePage(baseFile, addFile, output, new ushort[1] { basePage }, 1, length, new ushort[1] { replacePage });
		}

		public bool TIFReplacePage(string baseFile, string addFile, string output, ushort[] basePages, ushort length, ushort[] replacePages)
		{
			if(basePages == null || replacePages == null || basePages.Length != replacePages.Length || basePages.Length > ushort.MaxValue)
				return false;
			return TIFFReplacePage(baseFile, addFile, output, basePages, (ushort)basePages.Length, length, replacePages);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode, CallingConvention=CallingConvention.Cdecl)]
		private static extern bool TIFFDeletePart(string input, string output, ushort startPage, ushort length);

		public bool TIFDeletePart(string input, string output, ushort startPage, ushort length)
		{
			return TIFFDeletePart(input, output, startPage, length);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Auto)]
		private static extern IntPtr TIFFFdOpen(SafeFileHandle fileHandle, string filename, [MarshalAs(UnmanagedType.LPStr)]string mode);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr TIFFClientOpen(string filename, [MarshalAs(UnmanagedType.LPStr)]string mode, IntPtr clientHandle, LibTiffHelper.TIFFReadWriteProc readproc, LibTiffHelper.TIFFReadWriteProc writeproc, LibTiffHelper.TIFFSeekProc seekproc, LibTiffHelper.TIFFCloseProc closeproc, LibTiffHelper.TIFFSizeProc sizeproc, LibTiffHelper.TIFFMapFileProc mapproc, LibTiffHelper.TIFFUnmapFileProc unmapproc);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr TIFFClientOpen(string filename, [MarshalAs(UnmanagedType.LPStr)]string mode, byte[] clientHandle, LibTiffHelper.TIFFReadWriteProc readproc, LibTiffHelper.TIFFReadWriteProc writeproc, LibTiffHelper.TIFFSeekProc seekproc, LibTiffHelper.TIFFCloseProc closeproc, LibTiffHelper.TIFFSizeProc sizeproc, LibTiffHelper.TIFFMapFileProc mapproc, LibTiffHelper.TIFFUnmapFileProc unmapproc);

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern LibTiffHelper.TIFFErrorHandler TIFFSetErrorHandler(LibTiffHelper.TIFFErrorHandler handler);

		public LibTiffHelper.TIFFErrorHandler TIFSetErrorHandler(LibTiffHelper.TIFFErrorHandler handler)
		{
			return TIFFSetErrorHandler(handler);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern LibTiffHelper.TIFFExtendProc TIFFSetTagExtender(LibTiffHelper.TIFFExtendProc proc);

		public LibTiffHelper.TIFFExtendProc TIFSetTagExtender(LibTiffHelper.TIFFExtendProc proc)
		{
			return TIFSetTagExtender(proc);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern void TIFFMergeFieldInfo(IntPtr thandle_t, LibTiffHelper.TIFFFieldInfo[] fieldInfo, int n);

		public void TIFMergeFieldInfo(IntPtr thandle_t, LibTiffHelper.TIFFFieldInfo[] fieldInfo, int n)
		{
			TIFFMergeFieldInfo(thandle_t, fieldInfo, n);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr TIFFFieldWithTag(IntPtr tifr, uint n);
		[DllImport(libTiff, SetLastError = true, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr TIFFFindField(IntPtr tifr, int ttag_t, LibTiffHelper.TIFFDataType type);

		public IntPtr TIFFindField(IntPtr tifr, int ttag_t, LibTiffHelper.TIFFDataType type)
		{
			return TIFFFindField(tifr, ttag_t, type);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern void TIFFClose(IntPtr tif);

		public void TIFClose(IntPtr tif)
		{
			TIFFClose(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void TIFFFlushData(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void TIFFCleanup(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFFlush(IntPtr tif);

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr LoadTIFFinDIB(IntPtr tifr, UInt16 page);

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, ushort va_list, ushort va_list1);

		public int TIFSetField(IntPtr tifr, uint ttag_t, ushort va_list, ushort va_list1)
		{
			return TIFFSetField(tifr, ttag_t, va_list, va_list1);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, double va_list);

		public int TIFSetField(IntPtr tifr, uint ttag_t, double va_list)
		{
			return TIFFSetField(tifr, ttag_t, va_list);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, int va_list);
		public int TIFSetField(IntPtr tifr, uint ttag_t, int va_list)
		{
			return TIFFSetField(tifr, ttag_t, va_list);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, byte[] va_list);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, float[] va_list);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, uint len, IntPtr va_list);

		public int TIFSetField(IntPtr tifr, uint ttag_t, uint len, IntPtr va_list)
		{
			return TIFFSetField(tifr, ttag_t, len, va_list);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, byte[] va_list, byte[] va_list1, byte[] va_list2);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, ushort[] va_list, ushort[] va_list1, ushort[] va_list2);

		public int TIFSetField(IntPtr tifr, uint ttag_t, ushort[] va_list, ushort[] va_list1, ushort[] va_list2)
		{
			return TIFFSetField(tifr, ttag_t, va_list, va_list1, va_list2);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, IntPtr va_list, IntPtr va_list1, IntPtr va_list2);

		public int TIFSetField(IntPtr tifr, uint ttag_t, IntPtr va_list, IntPtr va_list1, IntPtr va_list2)
		{
			return TIFFSetField(tifr, ttag_t, va_list, va_list1, va_list2);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref float va_list);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref float va_list)
		{
			return TIFFGetField(tifr, ttag_t, ref va_list);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref uint va_list);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref int va_list);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref int va_list)
		{
			return TIFFGetField(tifr, ttag_t, ref va_list);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFIsTiled(IntPtr tif);

		public int TIFIsTiled(IntPtr tif)
		{
			return TIFFIsTiled(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref short va_list);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref int va_list, ref int va_list1);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref uint va_list, ref uint va_list1);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref uint va_list, ref uint va_list1)
		{
			return TIFFGetField(tifr, ttag_t, ref va_list, ref va_list1);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref ushort va_list);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref uint len, ref IntPtr ptr);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref uint len, ref IntPtr ptr)
		{
			return TIFFGetField(tifr, ttag_t, ref len, ref ptr);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref IntPtr va_list, ref IntPtr va_list1, ref IntPtr va_list2);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref IntPtr va_list, ref IntPtr va_list1, ref IntPtr va_list2)
		{
			return TIFFGetField(tifr, ttag_t, ref va_list, ref va_list1, ref va_list2);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref ushort[] va_list, ref ushort[] va_list1, ref ushort[] va_list2);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref ushort[] va_list, ref ushort[] va_list1, ref ushort[] va_list2)
		{
			return TIFFGetField(tifr, ttag_t, ref va_list, ref va_list1, ref va_list2);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteEncodedStrip(IntPtr tifr, uint strip, byte[] buf, int size);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFWriteEncodedStrip(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFWriteEncodedStrip(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFWriteEncodedStrip(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteRawStrip(IntPtr tifr, uint strip, IntPtr buf, int size);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFWriteRawStrip(IntPtr tifr, uint strip, byte[] buf, int size);

		public int TIFWriteRawStrip(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFWriteRawStrip(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFWriteRawTile(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFWriteRawTile(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFWriteRawTile(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFReadRawStrip(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFReadRawStrip(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFReadRawStrip(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadEncodedStrip(IntPtr tifr, uint strip, ref byte[] buf, int size);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFReadEncodedStrip(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFReadEncodedStrip(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFReadEncodedStrip(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFReadEncodedTile(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFReadEncodedTile(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFReadEncodedTile(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFReadRawTile(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFReadRawTile(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFReadRawTile(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern IntPtr _TIFFmalloc(int size);

		public IntPtr _TIFmalloc(int size)
		{
			return _TIFFmalloc(size);
		}

		//[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		//private static extern IntPtr _TIFFrealloc(IntPtr buffer, int size);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern void _TIFFfree(IntPtr buffer);

		public void _TIFfree(IntPtr buffer)
		{
			_TIFFfree(buffer);
		}

		//[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		//private static extern void _TIFFmemset(IntPtr s, int c, int n);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern void _TIFFmemcpy(IntPtr dest, IntPtr src, int n);

		public void _TIFmemcpy(IntPtr dest, IntPtr src, int n)
		{
			_TIFFmemcpy(dest, src, n);
		}

		//[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		//private static extern int _TIFFmemcmp(IntPtr s1, IntPtr s2, int n);

		//strip
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFStripSize(IntPtr tif);

		public int TIFStripSize(IntPtr tif)
		{
			return TIFFStripSize(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFNumberOfStrips(IntPtr tif);

		public int TIFNumberOfStrips(IntPtr tif)
		{
			return TIFFNumberOfStrips(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern uint TIFFComputeStrip(IntPtr tifr, uint row, uint sample);
		[DllImport(libTiff, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFDefaultStripSize(IntPtr tifr, uint estimate);

		public int TIFDefaultStripSize(IntPtr tifr, uint estimate)
		{
			return TIFFDefaultStripSize(tifr, estimate);
		}

		//Scanline
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFRasterScanlineSize(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFScanlineSize(IntPtr tif);

		public int TIFScanlineSize(IntPtr tif)
		{
			return TIFFScanlineSize(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFReadScanline(IntPtr tifr, IntPtr buf, uint row, UInt16 sample);

		public int TIFReadScanline(IntPtr tifr, IntPtr buf, uint row, UInt16 sample)
		{
			return TIFFReadScanline(tifr, buf, row, sample);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFReadScanline(IntPtr tifr, IntPtr buf, uint row);

		public int TIFReadScanline(IntPtr tifr, IntPtr buf, uint row)
		{
			return TIFFReadScanline(tifr, buf, row);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteScanline(IntPtr tifr, byte[] buf, uint row, UInt16 sample);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFWriteScanline(IntPtr tifr, IntPtr buf, uint row, UInt16 sample);

		public int TIFWriteScanline(IntPtr tifr, IntPtr buf, uint row, UInt16 sample)
		{
			return TIFFWriteScanline(tifr, buf, row, sample);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFWriteScanline(IntPtr tifr, IntPtr buf, uint row);

		public int TIFWriteScanline(IntPtr tifr, IntPtr buf, uint row)
		{
			return TIFFWriteScanline(tifr, buf, row);
		}

		//Tile
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFTileRowSize(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFTileSize(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFNumberOfTiles(IntPtr tif);

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFReadRGBAStrip(IntPtr tifr, uint row, IntPtr raster);

		public int TIFReadRGBAStrip(IntPtr tifr, uint row, IntPtr raster)
		{
			return TIFFReadRGBAStrip(tifr, row, raster);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFReadRGBAImage(IntPtr tifr, uint width, uint height, IntPtr raster, int stopOnError);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFReadRGBAImageOriented(IntPtr tifr, uint width, uint height, IntPtr raster, int orientation, int stopOnError);

		public int TIFReadRGBAImageOriented(IntPtr tifr, uint width, uint height, IntPtr raster, int orientation, int stopOnError)
		{
			return TIFFReadRGBAImageOriented(tifr, width, height, raster, orientation, stopOnError);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFNumberOfDirectories(IntPtr tif);

		public int TIFNumberOfDirectories(IntPtr tif)
		{
			return TIFFNumberOfDirectories(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFSetDirectory(IntPtr tifr, ushort dirnum);

		public int TIFSetDirectory(IntPtr tifr, ushort dirnum)
		{
			return TIFFSetDirectory(tifr, dirnum);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
		private static extern int TIFFWriteDirectory(IntPtr tif);

		public int TIFWriteDirectory(IntPtr tif)
		{
			return TIFFWriteDirectory(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadDirectory(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetDirectoryCount(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFRewriteDirectory(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetSubDirectory(IntPtr tifr, uint diroff);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFCheckpointDirectory(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern uint TIFFCurrentDirOffset(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFCreateDirectory(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFUnlinkDirectory(IntPtr tifr, ushort tdir_t);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void TIFFFreeDirectory(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi, CallingConvention=CallingConvention.Cdecl)]
		private static extern int TIFFLastDirectory(IntPtr tif);

		public int TIFLastDirectory(IntPtr tif)
		{
			return TIFFLastDirectory(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void TIFFError(string module, string fmt, string[] fmt1);
	}

	public class LibTiffHelperx64 : LibTiffHelperxxx
	{
		public LibTiffHelperx64() { }

		private const string libTiff = "libtiff.x64.dll";
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern IntPtr TIFFOpenW(string filename, [MarshalAs(UnmanagedType.AnsiBStr)]string mode);//mode w,r,a

		public IntPtr TIFOpenW(string filename, string mode)
		{
			return TIFFOpenW(filename, mode);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool TIFFCopyFile(string input, string output, ushort startPage, ushort length, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeParamIndex = 5)]ushort[] colorPages, ushort clength);

		public bool TIFCopyFile(string input, string output, ushort startPage, ushort length, ushort[] colorPages)
		{
			if(colorPages != null)
				return TIFFCopyFile(input, output, startPage, length, colorPages, (ushort)colorPages.Length);
			else
				return TIFFCopyFile(input, output, startPage, length, new ushort[0], (ushort)0);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool TIFFCopyFilePages(string input, string output, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeParamIndex = 5)]ushort[ ] pages, ushort pLenght, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeParamIndex = 5)]ushort[ ] colorPages, ushort clength, ushort compression);

		public bool TIFCopyFile(string input, string output, ushort[ ] pages, ushort[ ] colorPages, int compression)
		{
			if(pages.Length < 1)
				return false;
			if(colorPages != null)
				return TIFFCopyFilePages(input, output, pages, (ushort)pages.Length, colorPages, (ushort)colorPages.Length, (ushort)compression);
			else
				return TIFFCopyFilePages(input, output, pages, (ushort)pages.Length, new ushort[0], (ushort)0, (ushort)compression);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool TIFFCopyFileComp(string input, string output, ushort startPage, ushort length, [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U2, SizeParamIndex = 5)]ushort[] colorPages, ushort clength, ushort compression);

		public bool TIFCopyFile(string input, string output, ushort startPage, ushort length, ushort[] colorPages, int compression)
		{
			if(colorPages != null)
				return TIFFCopyFileComp(input, output, startPage, length, colorPages, (ushort)colorPages.Length, (ushort)compression);
			else
				return TIFFCopyFileComp(input, output, startPage, length, new ushort[0], (ushort)0, (ushort)compression);
		}


		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool TIFFReplacePage(string baseFile, string addFile, string output, ushort[] basePages, ushort baseCount, ushort length, ushort[] replacePages);

		public bool TIFReplacePage(string baseFile, string addFile, string output, ushort basePage, ushort length, ushort replacePage)
		{
			return TIFFReplacePage(baseFile, addFile, output, new ushort[1] { basePage }, 1, length, new ushort[1] { replacePage });
		}

		public bool TIFReplacePage(string baseFile, string addFile, string output, ushort[] basePages, ushort length, ushort[] replacePages)
		{
			if(basePages == null || replacePages == null || basePages.Length != replacePages.Length || basePages.Length > ushort.MaxValue)
				return false;
			return TIFFReplacePage(baseFile, addFile, output, basePages, (ushort)basePages.Length, length, replacePages);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Unicode)]
		private static extern bool TIFFDeletePart(string input, string output, ushort startPage, ushort length);

		public bool TIFDeletePart(string input, string output, ushort startPage, ushort length)
		{
			return TIFFDeletePart(input, output, startPage, length);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Auto)]
		private static extern IntPtr TIFFFdOpen(SafeFileHandle fileHandle, string filename, [MarshalAs(UnmanagedType.LPStr)]string mode);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr TIFFClientOpen(string filename, [MarshalAs(UnmanagedType.LPStr)]string mode, IntPtr clientHandle, LibTiffHelper.TIFFReadWriteProc readproc, LibTiffHelper.TIFFReadWriteProc writeproc, LibTiffHelper.TIFFSeekProc seekproc, LibTiffHelper.TIFFCloseProc closeproc, LibTiffHelper.TIFFSizeProc sizeproc, LibTiffHelper.TIFFMapFileProc mapproc, LibTiffHelper.TIFFUnmapFileProc unmapproc);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr TIFFClientOpen(string filename, [MarshalAs(UnmanagedType.LPStr)]string mode, byte[] clientHandle, LibTiffHelper.TIFFReadWriteProc readproc, LibTiffHelper.TIFFReadWriteProc writeproc, LibTiffHelper.TIFFSeekProc seekproc, LibTiffHelper.TIFFCloseProc closeproc, LibTiffHelper.TIFFSizeProc sizeproc, LibTiffHelper.TIFFMapFileProc mapproc, LibTiffHelper.TIFFUnmapFileProc unmapproc);

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern LibTiffHelper.TIFFErrorHandler TIFFSetErrorHandler(LibTiffHelper.TIFFErrorHandler handler);

		public LibTiffHelper.TIFFErrorHandler TIFSetErrorHandler(LibTiffHelper.TIFFErrorHandler handler)
		{
			return TIFFSetErrorHandler(handler);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern LibTiffHelper.TIFFExtendProc TIFFSetTagExtender(LibTiffHelper.TIFFExtendProc proc);

		public LibTiffHelper.TIFFExtendProc TIFSetTagExtender(LibTiffHelper.TIFFExtendProc proc)
		{
			return TIFSetTagExtender(proc);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void TIFFMergeFieldInfo(IntPtr thandle_t, LibTiffHelper.TIFFFieldInfo[] fieldInfo, int n);

		public void TIFMergeFieldInfo(IntPtr thandle_t, LibTiffHelper.TIFFFieldInfo[] fieldInfo, int n)
		{
			TIFFMergeFieldInfo(thandle_t, fieldInfo, n);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr TIFFFieldWithTag(IntPtr tifr, uint n);
		[DllImport(libTiff, SetLastError = true)]
		private static extern IntPtr TIFFFindField(IntPtr tifr, int ttag_t, LibTiffHelper.TIFFDataType type);

		public IntPtr TIFFindField(IntPtr tifr, int ttag_t, LibTiffHelper.TIFFDataType type)
		{
			return TIFFFindField(tifr, ttag_t, type);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void TIFFClose(IntPtr tif);

		public void TIFClose(IntPtr tif)
		{
			TIFFClose(tif);
			Marshal.Release(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void TIFFFlushData(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void TIFFCleanup(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFFlush(IntPtr tif);

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr LoadTIFFinDIB(IntPtr tifr, UInt16 page);

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, ushort va_list, ushort va_list1);

		public int TIFSetField(IntPtr tifr, uint ttag_t, ushort va_list, ushort va_list1)
		{
			return TIFFSetField(tifr, ttag_t, va_list, va_list1);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, double va_list);

		public int TIFSetField(IntPtr tifr, uint ttag_t, double va_list)
		{
			return TIFFSetField(tifr, ttag_t, va_list);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, int va_list);
		public int TIFSetField(IntPtr tifr, uint ttag_t, int va_list)
		{
			return TIFFSetField(tifr, ttag_t, va_list);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, byte[] va_list);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, float[] va_list);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, uint len, IntPtr va_list);

		public int TIFSetField(IntPtr tifr, uint ttag_t, uint len, IntPtr va_list)
		{
			return TIFFSetField(tifr, ttag_t, len, va_list);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, byte[] va_list, byte[] va_list1, byte[] va_list2);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, ushort[] va_list, ushort[] va_list1, ushort[] va_list2);

		public int TIFSetField(IntPtr tifr, uint ttag_t, ushort[] va_list, ushort[] va_list1, ushort[] va_list2)
		{
			return TIFFSetField(tifr, ttag_t, va_list, va_list1, va_list2);
		}


		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetField(IntPtr tifr, uint ttag_t, IntPtr va_list, IntPtr va_list1, IntPtr va_list2);

		public int TIFSetField(IntPtr tifr, uint ttag_t, IntPtr va_list, IntPtr va_list1, IntPtr va_list2)
		{
			return TIFFSetField(tifr, ttag_t, va_list, va_list1, va_list2);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref float va_list);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref float va_list)
		{
			return TIFFGetField(tifr, ttag_t, ref va_list);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFIsTiled(IntPtr tif);

		public int TIFIsTiled(IntPtr tif)
		{
			return TIFFIsTiled(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref uint va_list);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref int va_list);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref int va_list)
		{
			return TIFFGetField(tifr, ttag_t, ref va_list);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref short va_list);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref int va_list, ref int va_list1);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref uint va_list, ref uint va_list1);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref uint va_list, ref uint va_list1)
		{
			return TIFFGetField(tifr, ttag_t, ref va_list, ref va_list1);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref ushort va_list);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref uint len, ref IntPtr ptr);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref uint len, ref IntPtr ptr)
		{
			return TIFFGetField(tifr, ttag_t, ref len, ref ptr);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref IntPtr va_list, ref IntPtr va_list1, ref IntPtr va_list2);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref IntPtr va_list, ref IntPtr va_list1, ref IntPtr va_list2)
		{
			return TIFFGetField(tifr, ttag_t, ref va_list, ref va_list1, ref va_list2);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetField(IntPtr tifr, uint ttag_t, ref ushort[] va_list, ref ushort[] va_list1, ref ushort[] va_list2);

		public int TIFGetField(IntPtr tifr, uint ttag_t, ref ushort[] va_list, ref ushort[] va_list1, ref ushort[] va_list2)
		{
			return TIFFGetField(tifr, ttag_t, ref va_list, ref va_list1, ref va_list2);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteEncodedStrip(IntPtr tifr, uint strip, byte[] buf, int size);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteEncodedStrip(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFWriteEncodedStrip(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFWriteEncodedStrip(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadEncodedTile(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFReadEncodedTile(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFReadEncodedTile(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadRawTile(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFReadRawTile(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFReadRawTile(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteRawStrip(IntPtr tifr, uint strip, IntPtr buf, int size);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteRawStrip(IntPtr tifr, uint strip, byte[] buf, int size);

		public int TIFWriteRawStrip(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFWriteRawStrip(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadRawStrip(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFReadRawStrip(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFReadRawStrip(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteRawTile(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFWriteRawTile(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFWriteRawTile(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadEncodedStrip(IntPtr tifr, uint strip, ref byte[] buf, int size);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadEncodedStrip(IntPtr tifr, uint strip, IntPtr buf, int size);

		public int TIFReadEncodedStrip(IntPtr tifr, uint strip, IntPtr buf, int size)
		{
			return TIFFReadEncodedStrip(tifr, strip, buf, size);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern IntPtr _TIFFmalloc(int size);

		public IntPtr _TIFmalloc(int size)
		{
			return _TIFFmalloc(size);
		}

		//[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		//private static extern IntPtr _TIFFrealloc(IntPtr buffer, int size);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void _TIFFfree(IntPtr buffer);

		public void _TIFfree(IntPtr buffer)
		{
			_TIFFfree(buffer);
		}

		//[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		//private static extern void _TIFFmemset(IntPtr s, int c, int n);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void _TIFFmemcpy(IntPtr dest, IntPtr src, int n);

		public void _TIFmemcpy(IntPtr dest, IntPtr src, int n)
		{
			_TIFFmemcpy(dest, src, n);
		}

		//[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		//private static extern int _TIFFmemcmp(IntPtr s1, IntPtr s2, int n);

		//strip
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFStripSize(IntPtr tif);

		public int TIFStripSize(IntPtr tif)
		{
			return TIFFStripSize(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFNumberOfStrips(IntPtr tif);

		public int TIFNumberOfStrips(IntPtr tif)
		{
			return TIFFNumberOfStrips(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern uint TIFFComputeStrip(IntPtr tifr, uint row, uint sample);
		[DllImport(libTiff)]
		private static extern int TIFFDefaultStripSize(IntPtr tifr, uint estimate);

		public int TIFDefaultStripSize(IntPtr tifr, uint estimate)
		{
			return TIFFDefaultStripSize(tifr, estimate);
		}

		//Scanline
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFRasterScanlineSize(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFScanlineSize(IntPtr tif);

		public int TIFScanlineSize(IntPtr tif)
		{
			return TIFFScanlineSize(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadScanline(IntPtr tifr, IntPtr buf, uint row);

		public int TIFReadScanline(IntPtr tifr, IntPtr buf, uint row)
		{
			return TIFFReadScanline(tifr, buf, row);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadScanline(IntPtr tifr, IntPtr buf, uint row, UInt16 sample);

		public int TIFReadScanline(IntPtr tifr, IntPtr buf, uint row, UInt16 sample)
		{
			return TIFFReadScanline(tifr, buf, row, sample);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteScanline(IntPtr tifr, byte[] buf, uint row, UInt16 sample);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteScanline(IntPtr tifr, IntPtr buf, uint row, UInt16 sample);

		public int TIFWriteScanline(IntPtr tifr, IntPtr buf, uint row, UInt16 sample)
		{
			return TIFFWriteScanline(tifr, buf, row, sample);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteScanline(IntPtr tifr, IntPtr buf, uint row);

		public int TIFWriteScanline(IntPtr tifr, IntPtr buf, uint row)
		{
			return TIFFWriteScanline(tifr, buf, row);
		}

		//Tile
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFTileRowSize(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFTileSize(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFNumberOfTiles(IntPtr tif);

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadRGBAStrip(IntPtr tifr, uint row, IntPtr raster);

		public int TIFReadRGBAStrip(IntPtr tifr, uint row, IntPtr raster)
		{
			return TIFFReadRGBAStrip(tifr, row, raster);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadRGBAImage(IntPtr tifr, uint width, uint height, IntPtr raster, int stopOnError);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadRGBAImageOriented(IntPtr tifr, uint width, uint height, IntPtr raster, int orientation, int stopOnError);

		public int TIFReadRGBAImageOriented(IntPtr tifr, uint width, uint height, IntPtr raster, int orientation, int stopOnError)
		{
			return TIFFReadRGBAImageOriented(tifr, width, height, raster, orientation, stopOnError);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFNumberOfDirectories(IntPtr tif);

		public int TIFNumberOfDirectories(IntPtr tif)
		{
			return TIFFNumberOfDirectories(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetDirectory(IntPtr tifr, ushort dirnum);

		public int TIFSetDirectory(IntPtr tifr, ushort dirnum)
		{
			return TIFFSetDirectory(tifr, dirnum);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFWriteDirectory(IntPtr tif);

		public int TIFWriteDirectory(IntPtr tif)
		{
			return TIFFWriteDirectory(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFReadDirectory(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFGetDirectoryCount(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFRewriteDirectory(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFSetSubDirectory(IntPtr tifr, uint diroff);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFCheckpointDirectory(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern uint TIFFCurrentDirOffset(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFCreateDirectory(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFUnlinkDirectory(IntPtr tifr, ushort tdir_t);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void TIFFFreeDirectory(IntPtr tif);
		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern int TIFFLastDirectory(IntPtr tif);

		public int TIFLastDirectory(IntPtr tif)
		{
			return TIFFLastDirectory(tif);
		}

		[DllImport(libTiff, SetLastError = true, CharSet = CharSet.Ansi)]
		private static extern void TIFFError(string module, string fmt, string[] fmt1);
	}

	public interface LibTiffHelperxxx
	{
		void TIFClose(IntPtr tif);

		bool TIFCopyFile(string input, string output, ushort startPage, ushort length, ushort[] colorPages);

		bool TIFCopyFile(string input, string output, ushort startPage, ushort length, ushort[] colorPages, int compression);

		bool TIFCopyFile(string input, string output, ushort[] pages, ushort[] colorPages, int compression);

		bool TIFReplacePage(string baseFile, string addFile, string output, ushort basePage, ushort length, ushort replacePage);
		bool TIFReplacePage(string baseFile, string addFile, string output, ushort[] basePages, ushort length, ushort[] replacePages);

		bool TIFDeletePart(string input, string output, ushort startPage, ushort length);

		LibTiffHelper.TIFFExtendProc TIFSetTagExtender(LibTiffHelper.TIFFExtendProc proc);

		LibTiffHelper.TIFFErrorHandler TIFSetErrorHandler(LibTiffHelper.TIFFErrorHandler handler);

		IntPtr TIFFindField(IntPtr tifr, int ttag_t, LibTiffHelper.TIFFDataType type);

		int TIFGetField(IntPtr tifr, uint ttag_t, ref int va_list);
		int TIFGetField(IntPtr tifr, uint ttag_t, ref float va_list);
		int TIFGetField(IntPtr tifr, uint ttag_t, ref uint va_list, ref uint va_list1);
		int TIFGetField(IntPtr tifr, uint ttag_t, ref uint len, ref IntPtr ptr);
		int TIFGetField(IntPtr tifr, uint ttag_t, ref IntPtr va_list, ref IntPtr va_list1, ref IntPtr va_list2);
		int TIFGetField(IntPtr tifr, uint ttag_t, ref ushort[] va_list, ref ushort[] va_list1, ref ushort[] va_list2);

		int TIFSetField(IntPtr tifr, uint ttag_t, ushort va_list, ushort va_list1);
		int TIFSetField(IntPtr tifr, uint ttag_t, double va_list);
		int TIFSetField(IntPtr tifr, uint ttag_t, int va_list);
		int TIFSetField(IntPtr tifr, uint ttag_t, uint len, IntPtr va_list);
		int TIFSetField(IntPtr tifr, uint ttag_t, ushort[] va_list, ushort[] va_list1, ushort[] va_list2);
		int TIFSetField(IntPtr tifr, uint ttag_t, IntPtr va_list, IntPtr va_list1, IntPtr va_list2);

		void TIFMergeFieldInfo(IntPtr thandle_t, LibTiffHelper.TIFFFieldInfo[] fieldInfo, int n);

		IntPtr _TIFmalloc(int size);
		void _TIFmemcpy(IntPtr dest, IntPtr src, int n);
		void _TIFfree(IntPtr buffer);

		int TIFIsTiled(IntPtr tif);

		int TIFNumberOfDirectories(IntPtr tif);
		int TIFLastDirectory(IntPtr tif);
		int TIFWriteDirectory(IntPtr tif);
		int TIFSetDirectory(IntPtr tifr, ushort dirnum);

		IntPtr TIFOpenW(string filename, string mode);

		int TIFDefaultStripSize(IntPtr tifr, uint estimate);

		int TIFStripSize(IntPtr tif);

		int TIFNumberOfStrips(IntPtr tif);

		int TIFScanlineSize(IntPtr tif);

		int TIFWriteRawStrip(IntPtr tifr, uint strip, IntPtr buf, int size);

		int TIFWriteEncodedStrip(IntPtr tifr, uint strip, IntPtr buf, int size);

		int TIFWriteScanline(IntPtr tifr, IntPtr buf, uint row, UInt16 sample);

		int TIFWriteScanline(IntPtr tifr, IntPtr buf, uint row);

		int TIFReadRawStrip(IntPtr tifr, uint strip, IntPtr buf, int size);

		int TIFReadRawTile(IntPtr tifr, uint strip, IntPtr buf, int size);

		int TIFReadEncodedStrip(IntPtr tifr, uint strip, IntPtr buf, int size);

		int TIFReadRGBAStrip(IntPtr tifr, uint row, IntPtr raster);

		int TIFReadScanline(IntPtr tifr, IntPtr buf, uint row, UInt16 sample);

		int TIFReadScanline(IntPtr tifr, IntPtr buf, uint row);

		int TIFReadRGBAImageOriented(IntPtr tifr, uint width, uint height, IntPtr raster, int orientation, int stopOnError);

		int TIFWriteRawTile(IntPtr tifr, uint strip, IntPtr buf, int size);

	}

	public struct OptionForSave
	{
		public Boolean Is1bpp { get; set; }
		public Boolean Is8bpp { get; set; }
		public Boolean Is32bpp { get; set; }
		public Boolean Is8bppColor { get; set; }
		public Boolean IsCurrentFormat { get; set; }
	}
}