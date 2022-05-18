using System.Runtime.InteropServices;
using System.Text;

//Andy Schoen
//HVACR Advisors LLC
//andy@hvacradvisors.com
//hvacradvisors.com
namespace RefProp_wrapper
{
    public class iRefProp
    {
        private const int
            MaxComps = 20,
            WIN32MAXPATH = 260;

        public bool bubbleflag;

        public const int
            BUBBLEPOINT = 1,
            DEWPOINT = 2,
            NoDLL = 1;

        public double
            TC, PC, DC, MW;

        public string?
            herrstr;

        public static string?
            refpropPath;

        private double[] z = new double[MaxComps];

        private static IntPtr pDLL = IntPtr.Zero;

        //default installation folders for NIST RefProp
        const string FluidsDirectory = "\\fluids\\";
        const string MixturesDirectory = "\\mixtures\\";
        const string RefPropDirectory = "C:\\Program Files (x86)\\REFPROP\\";

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibrary(string strDLL);

        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FreeLibrary(IntPtr hModule);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern uint SearchPath(string path, string fileName, string fileExt,
            int bufSize, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder buffer, out IntPtr filePart);

        // certain RefProp function string parameters are required to be marshalled as VBByRefStr
        // which is marked as obsolete with Visual Studio 2022.  Fortunately, it still works.
        // pragma suppresses this warning message.

#pragma warning disable CS0618

        //RefProp API Functions

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ABFLSHdll(string ab, ref double a,
            ref double b, double[] z, ref int iFlag, ref double T,
            ref double P, ref double D, ref double Dl, ref double Dv,
            double[] xliq, double[] yvap, ref double q, ref double e,
            ref double h, ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int ab_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ALLPROPS0dll(ref int iIn, int[] iOut,
            ref int iFlag, ref double T, ref double D, double[] z,
            double[] Output, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ALLPROPS1dll(string hOut,
            ref int iUnits, ref double T, ref double D,
            double[] z, double[] c, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hOut_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ALLPROPSdll(string hOut, ref int iUnits,
            ref int iMass, ref int iFlag, ref double T,
            ref double D, double[] z, double[] Output,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hUnits,
            int[] iUCodeArray, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hOut_length, ref int hUnitsArray_length,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ERRMSGdll(ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void FLAGSdll(string hFlag, ref int jFlag,
            ref int kFlag, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hFlag_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GETENUMdll(ref int iFlag, string hEnum,
            ref int iEnum, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hEnum_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void REFPROPdll(string hFld, string hIn,
            string hOut, ref int iUnits, ref int iMass, ref int iFlag,
            ref double a, ref double b, double[] z, double[] Output,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hUnits,
            ref int iUCode, double[] xliq, double[] yvap, double[] x3,
            ref double q, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hFld_length, ref int hIn_length,
            ref int hOut_length, ref int hUnits_length,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void REFPROP1dll(string hIn, string hOut,
            ref int iUnits, ref int iMass, ref double a, ref double b,
            double[] z, ref double c, ref double q, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hIn_length, ref int hOut_length,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void REFPROP2dll(string hFld, string hIn,
            string hOut, ref int iUnits, ref int iFlag, ref double a,
            ref double b, double[] z, double[] Output, ref double q,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hFld_length, ref int hIn_length,
            ref int hOut_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETFLUIDSdll(string hFld,
            ref int hFld_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETMIXTUREdll(string hMixNme,
            double[] z, ref int ierr, ref int hMixNme_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETPATHdll(string hpth,
            ref int hpth_length);

        //Legacy API Functions. All legacy subroutines as documented in the NIST RefProp v10 user's manual.
        //Please note there exists additional legacy subroutines in the DLL file, but they are not listed here as
        //they are no longer being documented.

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ABFL1dll(ref double a, ref double b,
            double[] z, ref int kph, string ab, ref double Dmin,
            ref double Dmax, ref double T, ref double P,
            ref double D, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int ab_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ABFL2dll(ref double a, ref double b,
            double[] z, ref int kq, ref int ksat, string ab,
            ref double Tbub, ref double Tdew, ref double Pbub,
            ref double Pdew, ref double Dlbub, ref double Dvdew,
            double[] ybub, double[] xdew, ref double T, ref double P,
            ref double Dl, ref double Dv, double[] xliq, double[] yvap,
            ref double q, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int ab_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void AGdll(ref double T, ref double D, double[] z,
            ref double a, ref double g);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void B12dll(ref double T, double[] z, ref double B);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void BLCRVdll(ref double D, double[] z,
            ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void CHEMPOTdll(ref double T, ref double D,
            ref double[] z, ref double[] u, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void CP0dll(ref double T, ref double[] z,
            ref double Cp);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void CRITPdll(double[] z, ref double Tc,
            ref double Pc, ref double Dc, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void CRTPNTdll(double[] z, ref double Tc,
            ref double Pc, ref double Dc, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void CSATKdll(ref int icomp, ref double T,
            ref int kph, ref double P, ref double D,
            ref double Csat, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void CSTARdll(ref double T, ref double P,
            ref double v, double[] z, ref double Cs,
            ref double Ts, ref double Ds, ref double Ps,
            ref double ws, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void CV2PKdll(ref int icomp, ref double T,
            ref double D, ref double Cv2p, ref double Csat,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void CVCPdll(ref double T, ref double D, double[] z,
            ref double Cv, ref double Cp);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DBDTdll(ref double T, double[] z,
            ref double dBT);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DBFL1dll(ref double D, ref double b,
            double[] z, string hab, ref double T, ref double P,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hab_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DEFL1dll(ref double D, ref double e,
           double[] z, ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
           ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DEFLSHdll(ref double D, ref double e,
            double[] z, ref double T, ref double P, ref double Dl,
            ref double Dv, double[] xliq, double[] yvap, ref double q,
            ref double h, ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DERVPVTdll(ref double T, ref double D,
            double[] z, ref double dPdD, ref double dPdT, ref double d2PdD2,
            ref double d2PdT2, ref double d2PdTD, ref double dDdP,
            ref double dDdT, ref double d2DdP2, ref double d2DdT2,
            ref double d2DdPT, ref double dTdP, ref double dTdD,
            ref double d2TdP2, ref double d2TdD2, ref double d2TdPD);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DHD1dll(ref double T, ref double D, double[] z,
            ref double dhdt_d, ref double dhdt_p, ref double dhdd_t,
            ref double dhdd_p, ref double dhdp_t, ref double dhdp_d);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DHFL1dll(ref double D, ref double h, double[] z,
            ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DHFLSHdll(ref double D, ref double h,
            double[] z, ref double T, ref double P, ref double Dl,
            ref double Dv, double[] xliq, double[] yvap, ref double q,
            ref double e, ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DIELECdll(ref double T, ref double D,
            double[] z, ref double de);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DLSATKdll(ref int icomp, ref double T,
            ref double D, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DPDD2dll(ref double T, ref double D, double[] z,
            ref double d2PdD2);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DPTSATKdll(ref int icomp, ref double T,
            ref int kph, ref double P, ref double D, ref double Csat,
            ref double dPdT, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DQFL2dll(ref double D, ref double q,
            double[] z, ref int kq, ref double T, ref double P,
            ref double Dl, ref double Dv, double[] xliq, double[] yvap,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DSD1dll(ref double T, ref double D, double[] z,
            ref double dsdt_d, ref double dsdt_p, ref double dsdd_t,
            ref double dsdd_p, ref double dsdp_t, ref double dsdp_d);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DSFL1dll(ref double D, ref double s, double[] z,
            ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DSFLSHdll(ref double D, ref double s,
            double[] x, ref double T, ref double P, ref double Dl,
            ref double Dv, double[] xliq, double[] yvap, ref double q,
            ref double e, ref double h, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void DVSATKdll(ref int icomp, ref double T,
            ref double D, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ENTHALdll(ref double T, ref double D, double[] z,
            ref double h);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ENTROdll(ref double T, ref double D, double[] z,
            ref double s);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void ESFLSHdll(ref double e, ref double s,
            double[] z, ref double T, ref double P, ref double D,
            ref double Dl, ref double Dv, double[] xliq, double[] yvap,
            ref double q, ref double h, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void EXCESSdll(ref double T, ref double P,
            double[] z, ref int kph, ref double D, ref double vE,
            ref double eE, ref double hE, ref double sE, ref double aE,
            ref double gE, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void FGCTYdll(ref double T, ref double D,
            double[] z, double[] f);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void FGCTY2dll(ref double T, ref double D,
            double[] z, double[] f, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void FPVdll(ref double T, ref double D,
            ref double P, double[] z, ref double Fpvx);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void FUGCOFdll(ref double T, ref double D,
            double[] z, double[] phi, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GERG04dll(ref int ncomp, ref int iFlag,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GERG08dll(ref int ncomp, ref int iFlag,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GETFIJdll(string hmodij, double[] fij,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hfij,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hmxrul,
            ref int hmodij_length, ref int hfij_length,
            ref int hmxrul_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GETKTVdll(ref int icomp, ref int jcomp,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hmodij,
            double[] fij,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hFmix,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hfij,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hbinp,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hmxrul,
            ref int hmodij_length, ref int hFmix_length,
            ref int hfij_length, ref int hbinp_length,
            ref int hmxrul_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GETMODdll(ref int icomp, string htype,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hcode,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hcite,
            ref int htype_length, ref int hcode_length,
            ref int hcite_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GETREFDIRdll(
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hpth,
            ref int hpth_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void GIBBSdll(ref double T, ref double D, double[] z,
            ref double ar, ref double gr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void HEATdll(ref double T, ref double D, double[] z,
            ref double hg, ref double hn, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void HEATFRMdll(ref double T, ref double D, double[] z,
            ref double hFrm, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void HMXORDERdll(ref int i, ref int j,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hcasi,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hcasj,
            ref int iFlag, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hcasi_length, ref int hcasj_length,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void HSFL1dll(ref double h, ref double s,
            double[] z, ref double Dmin, ref double Dmax, ref double T,
            ref double D, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void HSFLSHdll(ref double h, ref double s,
            double[] z, ref double T, ref double P, ref double D,
            ref double Dl, ref double Dv, double[] xliq, double[] yvap,
            ref double q, ref double e, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void IDCRVdll(ref double D, double[] z,
            ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void INFOdll(ref int icomp, ref double wmm,
            ref double Ttrp, ref double Tnbpt, ref double Tc,
            ref double Pc, ref double Dc, ref double Zc,
            ref double acf, ref double dip, ref double Rgas);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void JICRVdll(ref double D, double[] z,
            ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void JTCRVdll(ref double D, double[] z,
            ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void LIMITKdll(string htyp, ref int icomp,
            ref double T, ref double D, ref double P, ref double Tmin,
            ref double Tmax, ref double Dmax, ref double Pmax,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int htyp_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void LIMITSdll(string htyp, double[] z,
            ref double Tmin, ref double Tmax, ref double Dmax,
            ref double Pmax, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int htyp_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void LIMITXdll(string htyp, ref double T,
            ref double D, ref double P, double[] z, ref double Tmin,
            ref double Tmax, ref double Dmax, ref double Pmax,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int htyp_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void LIQSPNDLdll(ref double T, double[] z,
            ref double D, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MASSFLUXdll(ref double Tm, ref double P,
            double[] z, ref double beta, ref double rf,
            ref double fluxm, ref double Cs, ref double T0,
            ref double P0, ref double xMach, ref double u,
            ref double Ts, ref double Ps, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MAXPdll(double[] z, ref double Tm,
            ref double Pm, ref double Dm, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MAXTdll(double[] z, ref double Tm,
            ref double Pm, ref double Dm, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MELTKdll(ref int icomp, ref double T,
            ref double P, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MELTPdll(ref double P, double[] z,
            ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MELTTdll(ref double T, double[] z,
            ref double P, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void MLTH2Odll(ref double T,
            ref double P1, ref double P2);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void NAMEdll(ref int icomp,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hnam,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hn80,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hcasn,
            ref int hnam_length, ref int hn80_length,
            ref int hcasn_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PASSCMNdll(string hvr, ref int iset,
            ref int icomp, ref int jcomp,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hstr,
            ref int ilng, ref double dbl, double[] arr,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hvr_length, ref int hstr_length,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PDFL1dll(ref double P, ref double D,
            double[] z, ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PDFLSHdll(ref double P, ref double D,
            double[] z, ref double T, ref double Dl,
            ref double Dv, double[] xliq, double[] yvap,
            ref double q, ref double e, ref double h,
            ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PEFL1dll(ref double P, ref double e,
            double[] z, ref int kph, ref double T, ref double D,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PEFLSHdll(ref double P, ref double e,
            double[] z, ref double T, ref double D, ref double Dl,
            ref double Dv, double[] xliq, double[] yvap, ref double q,
            ref double h, ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PHFL1dll(ref double P, ref double h,
            double[] z, ref int kph, ref double T, ref double D,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PHFLSHdll(ref double P, ref double h,
            double[] z, ref double T, ref double D, ref double Dl,
            ref double Dv, double[] xliq, double[] yvap, ref double q,
            ref double e, ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PHI0dll(ref int itau, ref int idel,
            ref double T, ref double D, double[] z, ref double phi00);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PHIDERVdll(ref int iderv, ref double T,
            ref double D, double[] z, double[] dadn, double[] dnadn,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PHIHMXdll(ref int itau, ref int idel,
            ref double tau, ref double delta, double[] z,
            ref double phi);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PHIKdll(ref int icomp, ref int itau,
            ref int idel, ref double tau, ref double delta,
            ref double phi);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PHIMIXdll(ref int i, ref int j,
            ref int itau, ref int idel, ref double tau,
            ref double delta, double[] z, ref double phi);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PHIXdll(ref int itau, ref int idel,
            ref double tau, ref double delta, double[] z,
            ref double phixx);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PQFLSHdll(ref double P, ref double q,
            double[] z, ref int kq, ref double T, ref double D,
            ref double Dl, ref double Dv, double[] xliq,
            double[] yvap, ref double e, ref double h,
            ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PREOSdll(ref int i);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PRESSdll(ref double T, ref double D, double[] z,
            ref double P);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PSATKdll(ref int icomp, ref double T,
            ref double P, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PSFL1dll(ref double P, ref double s,
            double[] z, ref int kph, ref double T, ref double D,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PSFLSHdll(ref double P, ref double s,
            double[] z, ref double T, ref double D, ref double Dl,
            ref double Dv, double[] xliq, double[] yvap, ref double q,
            ref double e, ref double h, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void PUREFLDdll(ref int icomp);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void QMASSdll(ref double qmol, double[] xl,
            double[] xv, ref double qkg, double[] xlkg, double[] xvkg,
            ref double wliq, ref double wvap, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void QMOLEdll(ref double qkg, double[] xlkg,
            double[] xvkg, ref double qmol, double[] xl,
            double[] xv, ref double wliq, ref double wvap,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RDXHMXdll(ref int ix, ref int icmp,
            ref int icmp2, double[] z, ref double Tred,
            ref double Dred, int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void REDXdll(double[] z, ref double Tred,
            ref double Dred);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RESIDUALdll(ref double T, ref double D,
            double[] z, ref double Pr, ref double er, ref double hr,
            ref double sr, ref double Cvr, ref double Cpr,
            ref double ar, ref double gr);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RIEMdll(ref double T, ref double D,
            double[] z, ref double riemc);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void RMIX2dll(double[] z, ref double Rgas);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATDdll(ref double D, double[] z,
            ref int kph, ref int kr, ref double T, ref double P,
            ref double Dl, ref double Dv, double[] xliq,
            double[] yvap, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATEdll(ref double e, double[] z,
            ref int kph, ref int nroot, ref int k1, ref double T1,
            ref double P1, ref double Dl, ref int k2,
            ref double T2, ref double P2, ref double D2,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATESTdll(ref int iFlash,
            ref double T, ref double P, double[] z,
            double[] xliq, double[] yvap, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATGUESSdll(ref int kph,
            ref int iprop, double[] x, ref double T,
            ref double P, ref double D, ref double h,
            ref double s, ref double Dy, double[] y,
            ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATGVdll(ref double T,
            ref double P, double[] z, ref double vf,
            ref double b, ref int ipv, ref int ityp,
            ref int isp, ref double Dx, ref double Dy,
            double[] x, double[] y, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATHdll(ref double h, double[] z,
            ref int kph, ref int nroot, ref int k1,
            ref double T1, ref double P1, ref double D1,
            ref int k2, ref double T2, ref double P2,
            ref double D2, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATPdll(ref double P, double[] z,
            ref int kph, ref double T, ref double Dl, ref double Dv,
            double[] xliq, double[] yvap, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATSdll(ref double s, double[] z,
            ref int kph, ref int nroot, ref int k1,
            ref double T1, ref double P1, ref double D1,
            ref int k2, ref double T2, ref double P2,
            ref double D2, ref int k3, ref double T3, ref double P3,
            ref double D3, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATSPLNdll(double[] z, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATTdll(ref double T, double[] z,
            ref int kph, ref double P, ref double Dl, ref double Dv,
            double[] xliq, double[] yvap, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SATTPdll(ref double T, ref double P,
            double[] z, ref int iFlsh, ref int iGuress, ref double D,
            ref double Dl, ref double Dv, double[] xliq, double[] yvap,
            ref double q, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETAGAdll(ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETKTVdll(ref int icomp, ref int jcomp,
            string hmodij, ref double fij, string hFmix, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hmodij_length, ref int hFmix_length,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETMIXdll(string hMixNme, string hFmix,
            string hrf, ref int ncc,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string hFiles,
            double[] z, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hMixNme_length, ref int hFmix_length,
            ref int hrf_length, ref int hFiles_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETMODdll(ref int ncomp, string htype,
            string hmix, string hcomp, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int htype_length, ref int hmix_length,
            ref int hcomp_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETNCdll(ref int ncomp);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETREFDIRdll(string hpth, ref int hpth_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETREFdll(string hrf, ref int ixflag,
            double[] x0, ref double h0, ref double s0, ref double T0,
            ref double P0, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hrf_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SETUPdll(ref int ncomp, string hFiles,
            string hFmix, string hrf, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int hFiles_length, ref int hFmix_length,
            ref int hrf_length, ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SPLNROOTdll(ref int isp, ref int iderv,
            ref double f, ref double a, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SPLNVALdll(ref int isp, ref int iderv,
            ref double f, ref double a, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void STNdll(ref double T, ref double Dl,
            ref double Dv, double[] xliq, double[] yvap,
            ref double sigma, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SUBLPdll(ref double P, double[] z,
            ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SUBLTdll(ref double T, double[] z,
            ref double P, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SURFTdll(ref double T, ref double Dl,
            double[] z, ref double sigma, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void SURTENdll(ref double T, ref double Dl,
            ref double[] Dv, double[] xliq, double[] yvap,
            ref double sigma, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TDFLSHdll(ref double T, ref double D,
            double[] z, ref double P, ref double Dl,
            ref double Dv, double[] xliq, double[] yvap,
            ref double q, ref double e, ref double h,
            ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TEFL1dll(ref double T, ref double e,
            double[] z, ref double Dmin, ref double Dmax,
            ref double D, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TEFLSHdll(ref double T, ref double e,
            double[] z, ref int kr, ref double P, ref double D,
            ref double Dl, ref double Dv, double[] xliq,
            double[] yvap, ref double q, ref double h,
            ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void THERMdll(ref double T, ref double D, double[] z,
            ref double P, ref double e, ref double h, ref double s,
            ref double Cv, ref double Cp, ref double w, ref double hjt);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void THERM0dll(ref double T, ref double D,
            double[] z, ref double P0, ref double e0, ref double h0,
            ref double s0, ref double Cv0, ref double Cp0,
            ref double w0, ref double a0, ref double g0);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void THERM2dll(ref double T, ref double D,
            double[] z, ref double P, ref double e, ref double h,
            ref double s, ref double Cv, ref double Cp,
            ref double w, ref double zz, ref double hjt,
            ref double a, ref double g, ref double xkappa,
            ref double beta, ref double dPdD, ref double d2PdD2,
            ref double dPdT, ref double dDdT, ref double dDdP,
            ref double d2PdT2, ref double d2PdTD,
            ref double spare3, ref double spare4);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void THERM3dll(ref double T, ref double D,
            double[] z, ref double xkappa, ref double beta,
            ref double xisenk, ref double xkt, ref double betas,
            ref double bs, ref double xkkt, ref double thrott,
            ref double pi, ref double spht);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void THFL1dll(ref double T, ref double h,
            double[] z, ref double Dmin, ref double Dmax,
            ref double D, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void THFLSHdll(ref double T, ref double h,
            double[] z, ref int kr, ref double P, ref double D,
            ref double Dl, ref double Dv, double[] xliq,
            double[] yvap, ref double q, ref double e,
            ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TPFL2dll(ref double T, ref double P,
            double[] z, ref double Dl, ref double Dv, double[] xliq,
            double[] yvap, ref double q, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TPFLSHdll(ref double T, ref double P,
            double[] z, ref double D, ref double Dl, ref double Dv,
            double[] xliq, double[] yvap, ref double q,
            ref double e, ref double h, ref double s, ref double Cv,
            ref double Cp, ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TPRHOdll(ref double T, ref double P, double[] z,
            ref int kph, ref int kguess, ref double D, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TPRHOPRdll(ref double T, ref double P, double[] z,
            ref double D1, ref double D2);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TQFLSHdll(ref double T, ref double q,
            double[] z, ref int kq, ref double P, ref double D,
            ref double Dl, ref double Dv, double[] xliq,
            double[] yvap, ref double e, ref double h,
            ref double s, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TRNPRPdll(ref double T, ref double D,
            double[] z, ref double eta, ref double tcx, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TSATDdll(ref double D, double[] z,
            ref double T, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TSATPdll(ref double P, double[] z,
            ref double T, ref int ierr, char[] herr,
            int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TSFL1dll(ref double T, ref double s,
            double[] z, ref double Dmin, ref double Dmax,
            ref double D, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void TSFLSHdll(ref double T, ref double s,
            double[] z, ref int kr, ref double P, ref double D,
            ref double Dl, ref double Dv, double[] xliq,
            double[] yvap, ref double q, ref double e,
            ref double h, ref double Cv, ref double Cp,
            ref double w, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void UNSETAGAdll();

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void VAPSPNDdll(ref double T,
            double[] z, ref double D, ref int ierr,
            [MarshalAs(UnmanagedType.VBByRefStr)] ref string herr,
            ref int herr_length);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void VIRBdll(ref double T, double[] z,
            ref double B);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void VIRBAdll(ref double T, double[] z,
            ref double Ba);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void VIRBCDdll(ref double T, double[] z,
            ref double B, ref double C, ref double D, ref double E);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void VIRCdll(ref double T, double[] z,
            ref double C);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void VIRCAdll(ref double T, double[] z,
            ref double Ca);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void WMOLdll(double[] z,
            ref double wmm);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void WMOLIdll(ref int icomp,
            ref double wmm);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void XMASSdll(double[] xmol, double[] xkg,
            ref double wmix);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void XMOLEdll(double[] xkg, double[] xmol,
            ref double wmix);

        public static ABFL1dll? abfl1dll;
        public static ABFL2dll? abfl2dll;
        public static ABFLSHdll? abflshdll;
        public static AGdll? agdll;
        public static ALLPROPS0dll? allprops0dll;
        public static ALLPROPS1dll? allprops1dll;
        public static ALLPROPSdll? allpropsdll;
        public static B12dll? b12dll;
        public static BLCRVdll? blcrvdll;
        public static CHEMPOTdll? chempotdll;
        public static CP0dll? cp0dll;
        public static CRITPdll? critpdll;
        public static CRTPNTdll? crtpntdll;
        public static CSATKdll? csatkdll;
        public static CSTARdll? cstardll;
        public static CV2PKdll? cv2pkdll;
        public static CVCPdll? cvcpdll;
        public static DBDTdll? dbdtdll;
        public static DBFL1dll? dbfl1dll;
        public static DEFL1dll? defl1dll;
        public static DEFLSHdll? deflshdll;
        public static DERVPVTdll? dervpvtdll;
        public static DHD1dll? dhd1dll;
        public static DHFL1dll? dhfl1dll;
        public static DHFLSHdll? dhflshdll;
        public static DIELECdll? dielecdll;
        public static DLSATKdll? dlsatkdll;
        public static DPDD2dll? dpdd2dll;
        public static DPTSATKdll? dptsatkdll;
        public static DQFL2dll? dqfl2dll;
        public static DSD1dll? dsd1dll;
        public static DSFL1dll? dsfl1dll;
        public static DSFLSHdll? dsflshdll;
        public static DVSATKdll? dvsatkdll;
        public static ENTHALdll? enthaldll;
        public static ENTROdll? entrodll;
        public static ERRMSGdll? errmsgdll;
        public static ESFLSHdll? esflshdll;
        public static EXCESSdll? excessdll;
        public static FGCTYdll? fgctydll;
        public static FGCTY2dll? fgcty2dll;
        public static FLAGSdll? flagsdll;
        public static FPVdll? fpvdll;
        public static FUGCOFdll? fugcofdll;
        public static GERG04dll? gerg04dll;
        public static GERG08dll? gerg08dll;
        public static GETENUMdll? getenumdll;
        public static GETFIJdll? getfijdll;
        public static GETKTVdll? getktvdll;
        public static GETMODdll? getmoddll;
        public static GETREFDIRdll? getrefdirdll;
        public static GIBBSdll? gibbsdll;
        public static HEATFRMdll? heatfrmdll;
        public static HEATdll? heatdll;
        public static HMXORDERdll? hmxorderdll;
        public static HSFL1dll? hsfl1dll;
        public static HSFLSHdll? hsflshdll;
        public static IDCRVdll? idcrvdll;
        public static INFOdll? infodll;
        public static JICRVdll? jicrvdll;
        public static JTCRVdll? jtcrvdll;
        public static LIMITKdll? limitkdll;
        public static LIMITSdll? limitsdll;
        public static LIMITXdll? limitxdll;
        public static LIQSPNDLdll? liqspndldll;
        public static MASSFLUXdll? massfluxdll;
        public static MAXPdll? maxpdll;
        public static MAXTdll? maxtdll;
        public static MELTKdll? meltkdll;
        public static MELTPdll? meltpdll;
        public static MELTTdll? melttdll;
        public static MLTH2Odll? mlth2odll;
        public static NAMEdll? namedll;
        public static PASSCMNdll? passcmndll;
        public static PDFL1dll? pdfl1dll;
        public static PDFLSHdll? pdflshdll;
        public static PEFL1dll? pefl1dll;
        public static PEFLSHdll? peflshdll;
        public static PHFL1dll? phfl1dll;
        public static PHFLSHdll? phflshdll;
        public static PHI0dll? phi0dll;
        public static PHIDERVdll? phidervdll;
        public static PHIHMXdll? phihmxdll;
        public static PHIKdll? phikdll;
        public static PHIMIXdll? phimixdll;
        public static PHIXdll? phixdll;
        public static PQFLSHdll? pqflshdll;
        public static PREOSdll? preosdll;
        public static PRESSdll? pressdll;
        public static PSATKdll? psatkdll;
        public static PSFL1dll? psfl1dll;
        public static PSFLSHdll? psflshdll;
        public static PUREFLDdll? pureflddll;
        public static QMASSdll? qmassdll;
        public static QMOLEdll? qmoledll;
        public static RDXHMXdll? rdxhmxdll;
        public static REDXdll? redxdll;
        public static REFPROPdll? refpropdll;
        public static REFPROP1dll? refprop1dll;
        public static REFPROP2dll? refprop2dll;
        public static RESIDUALdll? residualdll;
        public static RIEMdll? riemdll;
        public static RMIX2dll? rmix2dll;
        public static SATDdll? satddll;
        public static SATEdll? satedll;
        public static SATESTdll? satestdll;
        public static SATGUESSdll? satguessdll;
        public static SATGVdll? satgvdll;
        public static SATHdll? sathdll;
        public static SATPdll? satpdll;
        public static SATSdll? satsdll;
        public static SATSPLNdll? satsplndll;
        public static SATTdll? sattdll;
        public static SATTPdll? sattpdll;
        public static SETAGAdll? setagadll;
        public static SETFLUIDSdll? setfluidsdll;
        public static SETKTVdll? setktvdll;
        public static SETMIXdll? setmixdll;
        public static SETMIXTUREdll? setmixturedll;
        public static SETMODdll? setmoddll;
        public static SETNCdll? setncdll;
        public static SETPATHdll? setpathdll;
        public static SETREFDIRdll? setrefdirdll;
        public static SETREFdll? setrefdll;
        public static SETUPdll? setupdll;
        public static SPLNROOTdll? splnrootdll;
        public static SPLNVALdll? splnvaldll;
        public static STNdll? stndll;
        public static SUBLPdll? sublpdll;
        public static SUBLTdll? subltdll;
        public static SURFTdll? surftdll;
        public static SURTENdll? surtendll;
        public static TDFLSHdll? tdflshdll;
        public static TEFL1dll? tefl1dll;
        public static TEFLSHdll? teflshdll;
        public static THERM0dll? therm0dll;
        public static THERM2dll? therm2dll;
        public static THERM3dll? therm3dll;
        public static THERMdll? thermdll;
        public static THFL1dll? thfl1dll;
        public static THFLSHdll? thflshdll;
        public static TPFL2dll? tpfl2dll;
        public static TPFLSHdll? tpflshdll;
        public static TPRHOdll? tprhodll;
        public static TPRHOPRdll? tprhoprdll;
        public static TQFLSHdll? tqflshdll;
        public static TRNPRPdll? trnprpdll;
        public static TSATDdll? tsatddll;
        public static TSATPdll? tsatpdll;
        public static TSFL1dll? tsfl1dll;
        public static TSFLSHdll? tsflshdll;
        public static UNSETAGAdll? unsetagadll;
        public static VAPSPNDdll? vapspnddll;
        public static VIRBdll? virbdll;
        public static VIRBAdll? virbadll;
        public static VIRBCDdll? virbcddll;
        public static VIRCdll? vircdll;
        public static VIRCAdll? vircadll;
        public static WMOLdll? wmoldll;
        public static WMOLIdll? wmolidll;
        public static XMASSdll? xmassdll;
        public static XMOLEdll? xmoledll;

        // DLLinit() - initialize RefProp for use with .NET managed code
        // return values:
        //    0 = located REFPROP.dll/REFPRP64.dll
        //    1 = unable to locate REFPROP.dll/REFPRP64.dll
        public static int DLLinit()
        {
            IntPtr filepart;
            string RefPropFileName = (IntPtr.Size == 4) ? "REFPROP.dll" : "REFPRP64.dll";
            string? refpropEnv;
            StringBuilder rppath = new StringBuilder(WIN32MAXPATH);
            // see if NIST RefProp environment variable exists
            refpropEnv = Environment.GetEnvironmentVariable("RPPrefix");
            if (!String.IsNullOrEmpty(refpropEnv))
            {
                refpropPath = refpropEnv + "\\" + RefPropFileName;
                pDLL = LoadLibrary(refpropPath);
            }
            // if no RefProp environment variable, see if RefProp located in its default installation directory
            if (pDLL == IntPtr.Zero)
            {
                refpropPath = RefPropDirectory + RefPropFileName;
                pDLL = LoadLibrary(refpropPath);
            }
            if (pDLL == IntPtr.Zero)
                // unable to locate DLL in default NIST RefProp installation directory
                // use the Win32 SearchPath function to attempt to locate DLL
                if (SearchPath(String.Empty, RefPropFileName, String.Empty, WIN32MAXPATH, rppath,
                    out filepart) > 0)
                {
                    refpropPath = rppath.ToString();
                    pDLL = LoadLibrary(refpropPath);
                }
            if (pDLL == IntPtr.Zero)  // unable to locate DLL
                return NoDLL;
            else
            {
                IntPtr pFuncAddr;
                abfl1dll = ((pFuncAddr = GetProcAddress(pDLL, "ABFL1dll")) != IntPtr.Zero) ?
                    (ABFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(ABFL1dll)) : null;
                abfl2dll = ((pFuncAddr = GetProcAddress(pDLL, "ABFL2dll")) != IntPtr.Zero) ?
                    (ABFL2dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(ABFL2dll)) : null;
                abflshdll = ((pFuncAddr = GetProcAddress(pDLL, "ABFLSHdll")) != IntPtr.Zero) ?
                    (ABFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(ABFLSHdll)) : null;
                agdll = ((pFuncAddr = GetProcAddress(pDLL, "AGdll")) != IntPtr.Zero) ?
                    (AGdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(AGdll)) : null;
                allprops0dll = ((pFuncAddr = GetProcAddress(pDLL, "ALLPROPS0dll")) != IntPtr.Zero) ?
                    (ALLPROPS0dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(ALLPROPS0dll)) : null;
                allprops1dll = ((pFuncAddr = GetProcAddress(pDLL, "ALLPROPS1dll")) != IntPtr.Zero) ?
                    (ALLPROPS1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(ALLPROPS1dll)) : null;
                allpropsdll = ((pFuncAddr = GetProcAddress(pDLL, "ALLPROPSdll")) != IntPtr.Zero) ?
                    (ALLPROPSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(ALLPROPSdll)) : null;
                b12dll = ((pFuncAddr = GetProcAddress(pDLL, "B12dll")) != IntPtr.Zero) ?
                    (B12dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(B12dll)) : null;
                blcrvdll = ((pFuncAddr = GetProcAddress(pDLL, "BLCRdll")) != IntPtr.Zero) ?
                    (BLCRVdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(BLCRVdll)) : null;
                chempotdll = ((pFuncAddr = GetProcAddress(pDLL, "CHEMPOTdll")) != IntPtr.Zero) ?
                    (CHEMPOTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(CHEMPOTdll)) : null;
                cp0dll = ((pFuncAddr = GetProcAddress(pDLL, "CP0dll")) != IntPtr.Zero) ?
                    (CP0dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(CP0dll)) : null;
                critpdll = ((pFuncAddr = GetProcAddress(pDLL, "CRITPdll")) != IntPtr.Zero) ?
                    (CRITPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(CRITPdll)) : null;
                crtpntdll = ((pFuncAddr = GetProcAddress(pDLL, "CRTPNTdll")) != IntPtr.Zero) ?
                    (CRTPNTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(CRTPNTdll)) : null;
                csatkdll = ((pFuncAddr = GetProcAddress(pDLL, "CSATKdll")) != IntPtr.Zero) ?
                    (CSATKdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(CSATKdll)) : null;
                cstardll = ((pFuncAddr = GetProcAddress(pDLL, "CSTARdll")) != IntPtr.Zero) ?
                    (CSTARdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(CSTARdll)) : null;
                cv2pkdll = ((pFuncAddr = GetProcAddress(pDLL, "CV2PKdll")) != IntPtr.Zero) ?
                    (CV2PKdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(CV2PKdll)) : null;
                cvcpdll = ((pFuncAddr = GetProcAddress(pDLL, "CVCPdll")) != IntPtr.Zero) ?
                    (CVCPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(CVCPdll)) : null;
                dbdtdll = ((pFuncAddr = GetProcAddress(pDLL, "DBDTdll")) != IntPtr.Zero) ?
                    (DBDTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DBDTdll)) : null;
                dbfl1dll = ((pFuncAddr = GetProcAddress(pDLL, "DBFL1dll")) != IntPtr.Zero) ?
                    (DBFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DBFL1dll)) : null;
                defl1dll = ((pFuncAddr = GetProcAddress(pDLL, "DEFL1dll")) != IntPtr.Zero) ?
                    (DEFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DEFL1dll)) : null;
                deflshdll = ((pFuncAddr = GetProcAddress(pDLL, "DEFLSHdll")) != IntPtr.Zero) ?
                    (DEFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DEFLSHdll)) : null;
                dervpvtdll = ((pFuncAddr = GetProcAddress(pDLL, "DERVPVTdll")) != IntPtr.Zero) ?
                    (DERVPVTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DERVPVTdll)) : null;
                dhd1dll = ((pFuncAddr = GetProcAddress(pDLL, "DHD1dll")) != IntPtr.Zero) ?
                    (DHD1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DHD1dll)) : null;
                dhfl1dll = ((pFuncAddr = GetProcAddress(pDLL, "DHFL1dll")) != IntPtr.Zero) ?
                    (DHFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DHFL1dll)) : null;
                dhflshdll = ((pFuncAddr = GetProcAddress(pDLL, "DHFLSHdll")) != IntPtr.Zero) ?
                    (DHFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DHFLSHdll)) : null;
                dielecdll = ((pFuncAddr = GetProcAddress(pDLL, "DIELECdll")) != IntPtr.Zero) ?
                    (DIELECdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DIELECdll)) : null;
                dlsatkdll = ((pFuncAddr = GetProcAddress(pDLL, "DLSATKdll")) != IntPtr.Zero) ?
                    (DLSATKdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DLSATKdll)) : null;
                dpdd2dll = ((pFuncAddr = GetProcAddress(pDLL, "DPDD2dll")) != IntPtr.Zero) ?
                    (DPDD2dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DPDD2dll)) : null;
                dptsatkdll = ((pFuncAddr = GetProcAddress(pDLL, "DPTSATKdll")) != IntPtr.Zero) ?
                    (DPTSATKdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DPTSATKdll)) : null;
                dqfl2dll = ((pFuncAddr = GetProcAddress(pDLL, "DQFL2dll")) != IntPtr.Zero) ?
                    (DQFL2dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DQFL2dll)) : null;
                dsd1dll = ((pFuncAddr = GetProcAddress(pDLL, "DSD1dll")) != IntPtr.Zero) ?
                    (DSD1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DSD1dll)) : null;
                dsfl1dll = ((pFuncAddr = GetProcAddress(pDLL, "DSFL1dll")) != IntPtr.Zero) ?
                    (DSFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DSFL1dll)) : null;
                dsflshdll = ((pFuncAddr = GetProcAddress(pDLL, "DSFLSHdll")) != IntPtr.Zero) ?
                    (DSFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DSFLSHdll)) : null;
                dvsatkdll = ((pFuncAddr = GetProcAddress(pDLL, "DVSATKdll")) != IntPtr.Zero) ?
                    (DVSATKdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(DVSATKdll)) : null;
                enthaldll = ((pFuncAddr = GetProcAddress(pDLL, "ENTHALdll")) != IntPtr.Zero) ?
                    (ENTHALdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(ENTHALdll)) : null;
                entrodll = ((pFuncAddr = GetProcAddress(pDLL, "ENTROdll")) != IntPtr.Zero) ?
                    (ENTROdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(ENTROdll)) : null;
                errmsgdll = ((pFuncAddr = GetProcAddress(pDLL, "ERRMSGdll")) != IntPtr.Zero) ?
                    (ERRMSGdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(ERRMSGdll)) : null;
                esflshdll = ((pFuncAddr = GetProcAddress(pDLL, "ESFLSHdll")) != IntPtr.Zero) ?
                    (ESFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(ESFLSHdll)) : null;
                excessdll = ((pFuncAddr = GetProcAddress(pDLL, "EXCESSdll")) != IntPtr.Zero) ?
                    (EXCESSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(EXCESSdll)) : null;
                fgctydll = ((pFuncAddr = GetProcAddress(pDLL, "FGCTYdll")) != IntPtr.Zero) ?
                    (FGCTYdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(FGCTYdll)) : null;
                fgcty2dll = ((pFuncAddr = GetProcAddress(pDLL, "FGCTY2dll")) != IntPtr.Zero) ?
                    (FGCTY2dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(FGCTY2dll)) : null;
                flagsdll = ((pFuncAddr = GetProcAddress(pDLL, "FLAGSdll")) != IntPtr.Zero) ?
                    (FLAGSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(FLAGSdll)) : null;
                fpvdll = ((pFuncAddr = GetProcAddress(pDLL, "FPVdll")) != IntPtr.Zero) ?
                    (FPVdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(FPVdll)) : null;
                fugcofdll = ((pFuncAddr = GetProcAddress(pDLL, "FUGCOFdll")) != IntPtr.Zero) ?
                    (FUGCOFdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(FUGCOFdll)) : null;
                gerg04dll = ((pFuncAddr = GetProcAddress(pDLL, "GERG04dll")) != IntPtr.Zero) ?
                    (GERG04dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(GERG04dll)) : null;
                gerg08dll = ((pFuncAddr = GetProcAddress(pDLL, "GERG08dll")) != IntPtr.Zero) ?
                    (GERG08dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(GERG08dll)) : null;
                getenumdll = ((pFuncAddr = GetProcAddress(pDLL, "GETENUMdll")) != IntPtr.Zero) ?
                    (GETENUMdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(GETENUMdll)) : null;
                getfijdll = ((pFuncAddr = GetProcAddress(pDLL, "GETFIJdll")) != IntPtr.Zero) ?
                    (GETFIJdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(GETFIJdll)) : null;
                getktvdll = ((pFuncAddr = GetProcAddress(pDLL, "GETKTVdll")) != IntPtr.Zero) ?
                    (GETKTVdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(GETKTVdll)) : null;
                getmoddll = ((pFuncAddr = GetProcAddress(pDLL, "GETMODdll")) != IntPtr.Zero) ?
                    (GETMODdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(GETMODdll)) : null;
                getrefdirdll = ((pFuncAddr = GetProcAddress(pDLL, "GETREFDIRdll")) != IntPtr.Zero) ?
                    (GETREFDIRdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(GETREFDIRdll)) : null;
                gibbsdll = ((pFuncAddr = GetProcAddress(pDLL, "GIBBSdll")) != IntPtr.Zero) ?
                    (GIBBSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(GIBBSdll)) : null;
                heatdll = ((pFuncAddr = GetProcAddress(pDLL, "HEATdll")) != IntPtr.Zero) ?
                    (HEATdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(HEATdll)) : null;
                heatfrmdll = ((pFuncAddr = GetProcAddress(pDLL, "HEATFRMdll")) != IntPtr.Zero) ?
                    (HEATFRMdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(HEATFRMdll)) : null;
                hmxorderdll = ((pFuncAddr = GetProcAddress(pDLL, "HMXORDERdll")) != IntPtr.Zero) ?
                    (HMXORDERdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(HMXORDERdll)) : null;
                hsfl1dll = ((pFuncAddr = GetProcAddress(pDLL, "HSFL1dll")) != IntPtr.Zero) ?
                    (HSFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(HSFL1dll)) : null;
                hsflshdll = ((pFuncAddr = GetProcAddress(pDLL, "HSFLSHdll")) != IntPtr.Zero) ?
                    (HSFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(HSFLSHdll)) : null;
                idcrvdll = ((pFuncAddr = GetProcAddress(pDLL, "IDCRVdll")) != IntPtr.Zero) ?
                    (IDCRVdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(IDCRVdll)) : null;
                infodll = ((pFuncAddr = GetProcAddress(pDLL, "INFOdll")) != IntPtr.Zero) ?
                    (INFOdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(INFOdll)) : null;
                jicrvdll = ((pFuncAddr = GetProcAddress(pDLL, "JICRVdll")) != IntPtr.Zero) ?
                    (JICRVdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(JICRVdll)) : null;
                jtcrvdll = ((pFuncAddr = GetProcAddress(pDLL, "JTCRVdll")) != IntPtr.Zero) ?
                    (JTCRVdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(JTCRVdll)) : null;
                limitkdll = ((pFuncAddr = GetProcAddress(pDLL, "LIMITKdll")) != IntPtr.Zero) ?
                    (LIMITKdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(LIMITKdll)) : null;
                limitsdll = ((pFuncAddr = GetProcAddress(pDLL, "LIMITSdll")) != IntPtr.Zero) ?
                    (LIMITSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(LIMITSdll)) : null;
                limitxdll = ((pFuncAddr = GetProcAddress(pDLL, "LIMITXdll")) != IntPtr.Zero) ?
                    (LIMITXdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(LIMITXdll)) : null;
                liqspndldll = ((pFuncAddr = GetProcAddress(pDLL, "LIQSPNDLdll")) != IntPtr.Zero) ?
                    (LIQSPNDLdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(LIQSPNDLdll)) : null;
                massfluxdll = ((pFuncAddr = GetProcAddress(pDLL, "MASSFLUXdll")) != IntPtr.Zero) ?
                    (MASSFLUXdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(MASSFLUXdll)) : null;
                maxpdll = ((pFuncAddr = GetProcAddress(pDLL, "MAXPdll")) != IntPtr.Zero) ?
                    (MAXPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(MAXPdll)) : null;
                maxtdll = ((pFuncAddr = GetProcAddress(pDLL, "MAXTdll")) != IntPtr.Zero) ?
                    (MAXTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(MAXTdll)) : null;
                meltkdll = ((pFuncAddr = GetProcAddress(pDLL, "MELTKdll")) != IntPtr.Zero) ?
                    (MELTKdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(MELTKdll)) : null;
                meltpdll = ((pFuncAddr = GetProcAddress(pDLL, "MELTPdll")) != IntPtr.Zero) ?
                    (MELTPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(MELTPdll)) : null;
                melttdll = ((pFuncAddr = GetProcAddress(pDLL, "MELTTdll")) != IntPtr.Zero) ?
                    (MELTTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(MELTTdll)) : null;
                mlth2odll = ((pFuncAddr = GetProcAddress(pDLL, "MLYH2Odll")) != IntPtr.Zero) ?
                    (MLTH2Odll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(MLTH2Odll)) : null;
                namedll = ((pFuncAddr = GetProcAddress(pDLL, "NAMEdll")) != IntPtr.Zero) ?
                    (NAMEdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(NAMEdll)) : null;
                passcmndll = ((pFuncAddr = GetProcAddress(pDLL, "PASSCMNdll")) != IntPtr.Zero) ?
                    (PASSCMNdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PASSCMNdll)) : null;
                pdfl1dll = ((pFuncAddr = GetProcAddress(pDLL, "PDFL1dll")) != IntPtr.Zero) ?
                    (PDFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PDFL1dll)) : null;
                pdflshdll = ((pFuncAddr = GetProcAddress(pDLL, "PDFLSHdll")) != IntPtr.Zero) ?
                    (PDFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PDFLSHdll)) : null;
                pefl1dll = ((pFuncAddr = GetProcAddress(pDLL, "PEFL1dll")) != IntPtr.Zero) ?
                    (PEFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PEFL1dll)) : null;
                peflshdll = ((pFuncAddr = GetProcAddress(pDLL, "PEFLSHdll")) != IntPtr.Zero) ?
                    (PEFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PEFLSHdll)) : null;
                phfl1dll = ((pFuncAddr = GetProcAddress(pDLL, "PHFL1dll")) != IntPtr.Zero) ?
                    (PHFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PHFL1dll)) : null;
                phflshdll = ((pFuncAddr = GetProcAddress(pDLL, "PHFLSHdll")) != IntPtr.Zero) ?
                    (PHFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PHFLSHdll)) : null;
                phi0dll = ((pFuncAddr = GetProcAddress(pDLL, "PHI0dll")) != IntPtr.Zero) ?
                    (PHI0dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PHI0dll)) : null;
                phidervdll = ((pFuncAddr = GetProcAddress(pDLL, "PHIDERVdll")) != IntPtr.Zero) ?
                    (PHIDERVdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PHIDERVdll)) : null;
                phihmxdll = ((pFuncAddr = GetProcAddress(pDLL, "PHIHMXdll")) != IntPtr.Zero) ?
                    (PHIHMXdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PHIHMXdll)) : null;
                phikdll = ((pFuncAddr = GetProcAddress(pDLL, "PHIKdll")) != IntPtr.Zero) ?
                    (PHIKdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PHIKdll)) : null;
                phimixdll = ((pFuncAddr = GetProcAddress(pDLL, "PHIMIXdll")) != IntPtr.Zero) ?
                    (PHIMIXdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PHIMIXdll)) : null;
                phixdll = ((pFuncAddr = GetProcAddress(pDLL, "PHIXdll")) != IntPtr.Zero) ?
                    (PHIXdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PHIXdll)) : null;
                pqflshdll = ((pFuncAddr = GetProcAddress(pDLL, "PQFLSHdll")) != IntPtr.Zero) ?
                    (PQFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PQFLSHdll)) : null;
                preosdll = ((pFuncAddr = GetProcAddress(pDLL, "PREOSdll")) != IntPtr.Zero) ?
                    (PREOSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PREOSdll)) : null;
                pressdll = ((pFuncAddr = GetProcAddress(pDLL, "PRESSdll")) != IntPtr.Zero) ?
                    (PRESSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PRESSdll)) : null;
                psatkdll = ((pFuncAddr = GetProcAddress(pDLL, "PSATKdll")) != IntPtr.Zero) ?
                    (PSATKdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PSATKdll)) : null;
                psfl1dll = ((pFuncAddr = GetProcAddress(pDLL, "PSFL1dll")) != IntPtr.Zero) ?
                    (PSFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PSFL1dll)) : null;
                psflshdll = ((pFuncAddr = GetProcAddress(pDLL, "PSFLSHdll")) != IntPtr.Zero) ?
                    (PSFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PSFLSHdll)) : null;
                pureflddll = ((pFuncAddr = GetProcAddress(pDLL, "PUREFLDdll")) != IntPtr.Zero) ?
                    (PUREFLDdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(PUREFLDdll)) : null;
                qmassdll = ((pFuncAddr = GetProcAddress(pDLL, "QMASSdll")) != IntPtr.Zero) ?
                    (QMASSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(QMASSdll)) : null;
                qmoledll = ((pFuncAddr = GetProcAddress(pDLL, "QMOLEdll")) != IntPtr.Zero) ?
                    (QMOLEdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(QMOLEdll)) : null;
                rdxhmxdll = ((pFuncAddr = GetProcAddress(pDLL, "RDXHMXdll")) != IntPtr.Zero) ?
                    (RDXHMXdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(RDXHMXdll)) : null;
                redxdll = ((pFuncAddr = GetProcAddress(pDLL, "REDXdll")) != IntPtr.Zero) ?
                    (REDXdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(REDXdll)) : null;
                refpropdll = ((pFuncAddr = GetProcAddress(pDLL, "REFPROPdll")) != IntPtr.Zero) ?
                    (REFPROPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(REFPROPdll)) : null;
                refprop1dll = ((pFuncAddr = GetProcAddress(pDLL, "REFPROP1dll")) != IntPtr.Zero) ?
                    (REFPROP1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(REFPROP1dll)) : null;
                refprop2dll = ((pFuncAddr = GetProcAddress(pDLL, "REFPROP2dll")) != IntPtr.Zero) ?
                    (REFPROP2dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(REFPROP2dll)) : null;
                residualdll = ((pFuncAddr = GetProcAddress(pDLL, "RESIDUALdll")) != IntPtr.Zero) ?
                    (RESIDUALdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(RESIDUALdll)) : null;
                riemdll = ((pFuncAddr = GetProcAddress(pDLL, "RIEMdll")) != IntPtr.Zero) ?
                    (RIEMdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(RIEMdll)) : null;
                rmix2dll = ((pFuncAddr = GetProcAddress(pDLL, "RMIX2dll")) != IntPtr.Zero) ?
                    (RMIX2dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(RMIX2dll)) : null;
                satddll = ((pFuncAddr = GetProcAddress(pDLL, "SATDdll")) != IntPtr.Zero) ?
                    (SATDdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATDdll)) : null;
                satedll = ((pFuncAddr = GetProcAddress(pDLL, "SATEdll")) != IntPtr.Zero) ?
                    (SATEdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATEdll)) : null;
                satestdll = ((pFuncAddr = GetProcAddress(pDLL, "SATESTdll")) != IntPtr.Zero) ?
                    (SATESTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATESTdll)) : null;
                satguessdll = ((pFuncAddr = GetProcAddress(pDLL, "SATGUESSdll")) != IntPtr.Zero) ?
                    (SATGUESSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATGUESSdll)) : null;
                satgvdll = ((pFuncAddr = GetProcAddress(pDLL, "SATGVdll")) != IntPtr.Zero) ?
                    (SATGVdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATGVdll)) : null;
                sathdll = ((pFuncAddr = GetProcAddress(pDLL, "SATHdll")) != IntPtr.Zero) ?
                    (SATHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATHdll)) : null;
                satpdll = ((pFuncAddr = GetProcAddress(pDLL, "SATPdll")) != IntPtr.Zero) ?
                    (SATPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATPdll)) : null;
                satsdll = ((pFuncAddr = GetProcAddress(pDLL, "SATSdll")) != IntPtr.Zero) ?
                    (SATSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATSdll)) : null;
                satsplndll = ((pFuncAddr = GetProcAddress(pDLL, "SATSPLNdll")) != IntPtr.Zero) ?
                    (SATSPLNdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATSPLNdll)) : null;
                sattdll = ((pFuncAddr = GetProcAddress(pDLL, "SATTdll")) != IntPtr.Zero) ?
                    (SATTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATTdll)) : null;
                sattpdll = ((pFuncAddr = GetProcAddress(pDLL, "SATTPdll")) != IntPtr.Zero) ?
                    (SATTPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SATTPdll)) : null;
                setagadll = ((pFuncAddr = GetProcAddress(pDLL, "SETAGAdll")) != IntPtr.Zero) ?
                    (SETAGAdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETAGAdll)) : null;
                setfluidsdll = ((pFuncAddr = GetProcAddress(pDLL, "SETFLUIDSdll")) != IntPtr.Zero) ?
                    (SETFLUIDSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETFLUIDSdll)) : null;
                setktvdll = ((pFuncAddr = GetProcAddress(pDLL, "SETKTVdll")) != IntPtr.Zero) ?
                    (SETKTVdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETKTVdll)) : null;
                setmixdll = ((pFuncAddr = GetProcAddress(pDLL, "SETMIXdll")) != IntPtr.Zero) ?
                    (SETMIXdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETMIXdll)) : null;
                setmixturedll = ((pFuncAddr = GetProcAddress(pDLL, "SETMIXTUREdll")) != IntPtr.Zero) ?
                    (SETMIXTUREdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETMIXTUREdll)) : null;
                setmoddll = ((pFuncAddr = GetProcAddress(pDLL, "SETMODdll")) != IntPtr.Zero) ?
                    (SETMODdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETMODdll)) : null;
                setncdll = ((pFuncAddr = GetProcAddress(pDLL, "SETNCdll")) != IntPtr.Zero) ?
                    (SETNCdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETNCdll)) : null;
                setpathdll = ((pFuncAddr = GetProcAddress(pDLL, "SETPATHdll")) != IntPtr.Zero) ?
                    (SETPATHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETPATHdll)) : null;
                setrefdirdll = ((pFuncAddr = GetProcAddress(pDLL, "SETREFDIRdll")) != IntPtr.Zero) ?
                    (SETREFDIRdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETREFDIRdll)) : null;
                setrefdll = ((pFuncAddr = GetProcAddress(pDLL, "SETREFdll")) != IntPtr.Zero) ?
                    (SETREFdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETREFdll)) : null;
                setupdll = ((pFuncAddr = GetProcAddress(pDLL, "SETUPdll")) != IntPtr.Zero) ?
                    (SETUPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SETUPdll)) : null;
                splnrootdll = ((pFuncAddr = GetProcAddress(pDLL, "SPLNROOTdll")) != IntPtr.Zero) ?
                    (SPLNROOTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SPLNROOTdll)) : null;
                splnvaldll = ((pFuncAddr = GetProcAddress(pDLL, "SPLNVALdll")) != IntPtr.Zero) ?
                    (SPLNVALdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SPLNVALdll)) : null;
                stndll = ((pFuncAddr = GetProcAddress(pDLL, "STNdll")) != IntPtr.Zero) ?
                    (STNdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(STNdll)) : null;
                sublpdll = ((pFuncAddr = GetProcAddress(pDLL, "SUBLPdll")) != IntPtr.Zero) ?
                    (SUBLPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SUBLPdll)) : null;
                subltdll = ((pFuncAddr = GetProcAddress(pDLL, "SUBLTdll")) != IntPtr.Zero) ?
                    (SUBLTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SUBLTdll)) : null;
                surftdll = ((pFuncAddr = GetProcAddress(pDLL, "SURFTdll")) != IntPtr.Zero) ?
                    (SURFTdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SURFTdll)) : null;
                surtendll = ((pFuncAddr = GetProcAddress(pDLL, "SURTENdll")) != IntPtr.Zero) ?
                    (SURTENdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(SURTENdll)) : null;
                tdflshdll = ((pFuncAddr = GetProcAddress(pDLL, "TDFLSHdll")) != IntPtr.Zero) ?
                    (TDFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TDFLSHdll)) : null;
                tefl1dll = ((pFuncAddr = GetProcAddress(pDLL, "TEFL1dll")) != IntPtr.Zero) ?
                    (TEFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TEFL1dll)) : null;
                teflshdll = ((pFuncAddr = GetProcAddress(pDLL, "TEFLSHdll")) != IntPtr.Zero) ?
                    (TEFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TEFLSHdll)) : null;
                thermdll = ((pFuncAddr = GetProcAddress(pDLL, "THERMdll")) != IntPtr.Zero) ?
                    (THERMdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(THERMdll)) : null;
                therm0dll = ((pFuncAddr = GetProcAddress(pDLL, "THERM0dll")) != IntPtr.Zero) ?
                    (THERM0dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(THERM0dll)) : null;
                therm2dll = ((pFuncAddr = GetProcAddress(pDLL, "THERM2dll")) != IntPtr.Zero) ?
                    (THERM2dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(THERM2dll)) : null;
                therm3dll = ((pFuncAddr = GetProcAddress(pDLL, "THERM3dll")) != IntPtr.Zero) ?
                    (THERM3dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(THERM3dll)) : null;
                thfl1dll = ((pFuncAddr = GetProcAddress(pDLL, "THFL1dll")) != IntPtr.Zero) ?
                    (THFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(THFL1dll)) : null;
                thflshdll = ((pFuncAddr = GetProcAddress(pDLL, "THFLSHdll")) != IntPtr.Zero) ?
                    (THFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(THFLSHdll)) : null;
                tpfl2dll = ((pFuncAddr = GetProcAddress(pDLL, "TPFL2dll")) != IntPtr.Zero) ?
                    (TPFL2dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TPFL2dll)) : null;
                tpflshdll = ((pFuncAddr = GetProcAddress(pDLL, "TPFLSHdll")) != IntPtr.Zero) ?
                    (TPFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TPFLSHdll)) : null;
                tprhodll = ((pFuncAddr = GetProcAddress(pDLL, "TPRHOdll")) != IntPtr.Zero) ?
                    (TPRHOdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TPRHOdll)) : null;
                tprhoprdll = ((pFuncAddr = GetProcAddress(pDLL, "TPRHOPRdll")) != IntPtr.Zero) ?
                    (TPRHOPRdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TPRHOPRdll)) : null;
                tqflshdll = ((pFuncAddr = GetProcAddress(pDLL, "TQFLSHdll")) != IntPtr.Zero) ?
                    (TQFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TQFLSHdll)) : null;
                trnprpdll = ((pFuncAddr = GetProcAddress(pDLL, "TRNPRPdll")) != IntPtr.Zero) ?
                    (TRNPRPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TRNPRPdll)) : null;
                tsatddll = ((pFuncAddr = GetProcAddress(pDLL, "TSATDdll")) != IntPtr.Zero) ?
                    (TSATDdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TSATDdll)) : null;
                tsatpdll = ((pFuncAddr = GetProcAddress(pDLL, "TSATPdll")) != IntPtr.Zero) ?
                    (TSATPdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TSATPdll)) : null;
                tsfl1dll = ((pFuncAddr = GetProcAddress(pDLL, "TSFL1dll")) != IntPtr.Zero) ?
                    (TSFL1dll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TSFL1dll)) : null;
                tsflshdll = ((pFuncAddr = GetProcAddress(pDLL, "TSFLSHdll")) != IntPtr.Zero) ?
                    (TSFLSHdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(TSFLSHdll)) : null;
                unsetagadll = ((pFuncAddr = GetProcAddress(pDLL, "UNSETAGAdll")) != IntPtr.Zero) ?
                    (UNSETAGAdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(UNSETAGAdll)) : null;
                vapspnddll = ((pFuncAddr = GetProcAddress(pDLL, "VAPSPNDdll")) != IntPtr.Zero) ?
                    (VAPSPNDdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(VAPSPNDdll)) : null;
                virbdll = ((pFuncAddr = GetProcAddress(pDLL, "VIRBdll")) != IntPtr.Zero) ?
                    (VIRBdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(VIRBdll)) : null;
                virbadll = ((pFuncAddr = GetProcAddress(pDLL, "VIRBAdll")) != IntPtr.Zero) ?
                    (VIRBAdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(VIRBAdll)) : null;
                virbcddll = ((pFuncAddr = GetProcAddress(pDLL, "VIRBCDdll")) != IntPtr.Zero) ?
                    (VIRBCDdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(VIRBCDdll)) : null;
                vircdll = ((pFuncAddr = GetProcAddress(pDLL, "VIRCdll")) != IntPtr.Zero) ?
                    (VIRCdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(VIRCdll)) : null;
                vircadll = ((pFuncAddr = GetProcAddress(pDLL, "VIRCAdll")) != IntPtr.Zero) ?
                    (VIRCAdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(VIRCAdll)) : null;
                wmoldll = ((pFuncAddr = GetProcAddress(pDLL, "WMOLdll")) != IntPtr.Zero) ?
                    (WMOLdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(WMOLdll)) : null;
                wmolidll = ((pFuncAddr = GetProcAddress(pDLL, "WMOLIdll")) != IntPtr.Zero) ?
                    (WMOLIdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(WMOLIdll)) : null;
                xmassdll = ((pFuncAddr = GetProcAddress(pDLL, "XMASSdll")) != IntPtr.Zero) ?
                    (XMASSdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(XMASSdll)) : null;
                xmoledll = ((pFuncAddr = GetProcAddress(pDLL, "XMOLEdll")) != IntPtr.Zero) ?
                    (XMOLEdll)Marshal.GetDelegateForFunctionPointer(pFuncAddr, typeof(XMOLEdll)) : null;
            }
            return 0;
        }

        public static bool DLLend()
        {
            bool retval = false;
            if (pDLL != IntPtr.Zero)
                retval = FreeLibrary(pDLL);
            return retval;
        }

        private const double // exact conversions
            FT_M = 0.3048, // converts feet to meters
            IN2_M2 = 0.00064516,  // converts in2 to m2
            LBF_KN = 0.0044482216152605, // converts pounds force to kilonewtons
            PSIA_KPA = LBF_KN / IN2_M2, // converts psia to kPa
            FT3_L = 28.316846592, // converts ft3 to liters
            LB_GM = 453.59237, // converts pounds to grams
            FT3LB_LGM = FT3_L / LB_GM, // converts ft3 per pound to liters per gram
            BTUitLB_KJKG = 2.326, // converts BtuIT per pround to kilojoules per kilogram
            BTUitLBR_KJKGK = 4.1868, // converts BtuIT per pound-°F to kilojoules per kilogram-K
            LBFTS_UPAS = 0.45359237 / 0.0000003048, // converts lbm/(ft-s) to uPa-s
            BTUITFTHFT2F_WMK = 0.52752792631 / 0.3048; // converts BtuIT-ft/h-ft2-°F to W/m-K
        public static double ftok(double t)
        {
            return ((t + 459.67) / 1.8);
        }
        public static double ktof(double t)
        {
            return (1.8 * t - 459.67);
        }
        public static double psia_kpa(double p)
        {
            return (p * PSIA_KPA);
        }
        public static double kpa_psia(double p)
        {
            return (p / PSIA_KPA);
        }
        public static double moll_kgm3(double r, double mw)
        {
            return (r * mw);
        }
        public static double moll_lbft3(double r, double mw)
        {
            return (r * mw * FT3LB_LGM);
        }
        public static double jmol_jgm(double h, double mw)
        {
            return (h / mw);
        }
        public static double jmol_blb(double h, double mw)
        {
            return (h / (mw * BTUitLB_KJKG));
        }
        public static double jmolk_jgmk(double s, double mw)
        {
            return (s / mw);
        }
        public static double jmolk_blbr(double s, double mw)
        {
            return (s / (mw * BTUitLBR_KJKGK));
        }
        public static double m_ft(double l)
        {
            return (l / FT_M);
        }
        public static double upas_lbfts(double eta)
        {
            return (eta / LBFTS_UPAS);
        }
        public static double wmk_btuitfthft2f(double tcx)
        {
            return (tcx / BTUITFTHFT2F_WMK);
        }
        public static double noconvert(double x)
        {
            return (x);
        }

        // Unlike previous versions of Visual Studio, VS2022 gets annoyed when a nullable
        // variable is used where having a null value will create issues.  The problem
        // here is with the function pointers to the RefProp functions.
        // pragma suppresses this warning message.

#pragma warning disable CS8602

        // NISTSetup - setup refrigerant
        // Run DLLinit() before NISTSetup().  DLLinit() only needs to be run once.
        // Multiple instances of NISTSetup can be run as needed
        // return values:
        //    true = success
        //    false = failure, see herrstr
        // Adapted from Frank J. Doyle's Excel 97 VBA code.  This function uses legacy NIST RefProp subroutines.
        public bool NISTSetup(string FluidName)
        {
            bool retval = false;
            int i, sc, ncc, nc, nc2 = 0, ierr = 0, mass = 0, ixflag = 2,
                hMixNme_length, hFmix_length, hrf_length = 3,
                hFiles_length = 10000, herr_length = 255, hpth_length;
            double sum, hRef = 0.0, sRef = 0.0, Tref = 0.0, pref = 0.0;
            double[] xkg = new double[MaxComps], ztemp = new double[MaxComps];
            string hFmix, hrf, hMixNme, hFiles, a, ab, FluidNme, FlNme, FluidsPrefix, MixturesPrefix, herr;
            string? rpDirectory;
            FlNme = FluidName;
            rpDirectory = Path.GetDirectoryName(refpropPath);
            FluidsPrefix = rpDirectory + FluidsDirectory;
            MixturesPrefix = rpDirectory + MixturesDirectory;
            hrf = "DEF";
            hFmix = FluidsPrefix + "hmx.bnc";
            hFmix_length = hFmix.Length;
            herr = new string(' ', herr_length);
            a = String.Empty;
            for (i = 0; i < MaxComps; i++)
                ztemp[i] = 0.0;
            if (FluidName.ToUpper().IndexOf(".MIX") != -1)
            {
                hMixNme = MixturesPrefix + FluidName;
                hMixNme_length = hMixNme.Length;
                hFiles = new String(' ', hFiles_length);
                hpth_length = rpDirectory.Length;
                setpathdll(rpDirectory, ref hpth_length);
                setmixdll(hMixNme, hFmix, hrf, ref nc2, ref hFiles, ztemp, ref ierr,
                    ref herr, ref hMixNme_length, ref hFmix_length, ref hrf_length,
                    ref hFiles_length, ref herr_length);
            }
            else if (FluidName.IndexOf(',') != -1 || FluidName.IndexOf(';') != -1)
            {
                FluidNme = FluidName.Trim();
                sc = (FluidName.IndexOf(';') != -1) ? 1 : 0;
                if (FluidNme.Substring(FluidNme.Length - 4).ToUpper() == "MASS")
                {
                    mass = 1;
                    FluidNme = FluidNme.Substring(0, FluidNme.Length - 4).Trim();
                }
                nc2 = 0;
                do
                {
                    i = (sc == 0) ? FluidNme.IndexOf(',') : FluidNme.IndexOf(';');
                    if (i == -1)
                        i = FluidNme.Length;
                    nc2++;
                    if (nc2 > MaxComps)
                    {
                        ierr = 1;
                        herrstr = "Too many components";
                        return retval;
                    }
                    ab = FluidNme.Substring(0, i).Trim();
                    if (ab.ToLower().IndexOf(".fld") == -1)
                        ab += ".fld";
                    a += FluidsPrefix + ab + "|";
                    FluidNme = FluidNme.Substring(i + 1);
                    i = (sc == 0) ? FluidNme.IndexOf(',') : FluidNme.IndexOf(';');
                    if (i == -1)
                        i = FluidNme.Length;
                    Double.TryParse(FluidNme.Substring(0, i), out ztemp[nc2 - 1]);
                    if (FluidNme.Length > i)
                        FluidNme = FluidNme.Substring(i + 1).Trim();
                    else
                        FluidNme = String.Empty;
                }
                while (!String.IsNullOrEmpty(FluidNme));
                sum = 0.0;
                for (i = 0; i < nc2; i++)
                    sum += sum + ztemp[i];
                if (sum <= 0.0)
                {
                    ierr = 1;
                    herrstr = "Composition not set";
                    return retval;
                }
                for (i = 0; i < nc2; i++)
                    ztemp[i] = ztemp[i] / sum;
                hFiles = a;
                if (nc2 < 1)
                {
                    ierr = 1;
                    herrstr = "Setup failed";
                    return retval;
                }
                setupdll(ref nc2, hFiles, hFmix, hrf, ref ierr, ref herr, ref hFiles_length,
                    ref hFmix_length, ref hrf_length, ref herr_length);
            }
            else if (FluidName.IndexOf('/') != -1 && FluidName.IndexOf('(') != -1)
            {
                FluidNme = FluidName.Trim();
                if (FluidNme.Substring(FluidNme.Length - 4) == "MASS")
                {
                    mass = 1;
                    FluidNme = FluidNme.Substring(0, FluidNme.Length - 4).Trim();
                }
                nc2 = 0;
                do
                {
                    i = FluidNme.IndexOf('/');
                    if (FluidNme.IndexOf('(') < i)
                        i = FluidNme.IndexOf('(');
                    if (i == 0)
                        i = FluidNme.Length + 1;
                    nc2++;
                    if (nc2 > MaxComps)
                    {
                        ierr = 1;
                        herrstr = "Too many components";
                        return retval;
                    }
                    ab = FluidNme.Substring(0, i - 1).Trim();
                    if (ab.ToLower().IndexOf(".fld") == -1)
                        ab += ".fld";
                    a += FluidsPrefix + ab + "|";
                    FluidNme = FluidNme.Substring(i);
                    if (FluidNme[0] == '/')
                        FluidNme = FluidNme.Substring(1).Trim();
                }
                while (FluidNme[0] != '(');
                FluidNme = FluidNme.Substring(1);
                if (FluidNme.Substring(FluidNme.Length - 1) == ")")
                    FluidNme = FluidNme.Substring(0, FluidNme.Length - 1).Trim();
                ncc = 0;
                do
                {
                    i = FluidNme.IndexOf('/');
                    if (i == 0) i = FluidNme.Length + 1;
                    ncc++;
                    if (ncc > MaxComps)
                    {
                        ierr = 1;
                        herrstr = "Too many components";
                        return retval;
                    }
                    Double.TryParse(FluidNme.Substring(0, i - 1), out ztemp[ncc - 1]);
                    FluidNme = FluidNme.Substring(i).Trim();
                }
                while (!String.IsNullOrEmpty(FluidNme));
                sum = 0.0;
                for (i = 0; i < nc2; i++)
                    sum += ztemp[i];
                if (sum <= 0.0)
                {
                    ierr = 1;
                    herrstr = "Composition not set";
                    return retval;
                }
                for (i = 0; i < nc2; i++)
                    ztemp[i] = ztemp[i] / sum;
                hFiles = a;
                if (nc2 < 1)
                {
                    ierr = 1;
                    herrstr = "Setup failed";
                    return retval;
                }
                setupdll(ref nc2, hFiles, hFmix, hrf, ref ierr, ref herr, ref hFiles_length,
                    ref hFmix_length, ref hrf_length, ref herr_length);
            }
            else
            {
                nc2 = 1;
                if (FluidName.ToLower().IndexOf(".fld") == -1 &&
                    FluidName.ToLower().IndexOf(".ppf") == -1)
                    FluidName += ".fld";
                if (FluidName.IndexOf('\\') != -1)
                    hFiles = FluidName;
                else
                    hFiles = FluidsPrefix + FluidName;
                setupdll(ref nc2, hFiles, hFmix, hrf, ref ierr, ref herr, ref hFiles_length,
                    ref hFmix_length, ref hrf_length, ref herr_length);
            }
            if (mass != 0)
            {
                for (i = 0; i < nc2; i++)
                    xkg[i] = ztemp[i];
                xmoledll(xkg, ztemp, ref MW);
            }
            else
                wmoldll(ztemp, ref MW);
            if (ierr <= 0)
            {
                nc = nc2;       //If setup was successful, load new values of nc and x()
                for (i = 0; i < nc; i++)
                    z[i] = ztemp[i];
                retval = true;
                setrefdll(hrf, ref ixflag, z, ref hRef, ref sRef, ref Tref, ref pref,
                    ref ierr, ref herr, ref hrf_length, ref herr_length);
                critpdll(z, ref TC, ref PC, ref DC, ref ierr, ref herr, ref herr_length);
            }
            else
                herrstr = herr;
            return retval;
        }

        // dllversetup - returns RefProp DLL version using setupdll
        public static string dllversetup()
        {
            int ncomp = -1, ierr = 0, hFiles_length = 10000, hFmix_length = 255, hrf_length = 3, herr_length = 255;
            string hFmix = String.Empty, hrf = String.Empty, hFiles = String.Empty, herr;
            herr = new string(' ', herr_length);
            setupdll(ref ncomp, hFiles, hFmix, hrf, ref ierr, ref herr, ref hFiles_length,
                ref hFmix_length, ref hrf_length, ref herr_length);
            return herr;
        }

        // sattdpt - Calculate saturated thermodynamic/thermophysical properties given temperature (K)
        // return values:
        //    true = success
        //    false = failure.  See herrstr for error message
        public bool sattdpt(double t, int kph, ref double p, ref double ldensity, ref double vdensity, ref double h,
            ref double s, ref double cv, ref double cp, ref double w, ref double eta, ref double tcx)
        {
            bool retval = false;
            int ierr = 0, herr_length = 255;
            double e = 0.0, hjt = 0.0, ptherm = 0.0;
            double[] xliq = new double[MaxComps], yvap = new double[MaxComps];
            string herr = new string(' ', herr_length);
            if (sattdll != null && thermdll != null && trnprpdll != null)
            {
                sattdll(ref t, z, ref kph, ref p, ref ldensity, ref vdensity, xliq, yvap, ref ierr,
                    ref herr, ref herr_length);
                if (ierr == 0)
                {
                    if (kph == BUBBLEPOINT)
                    {
                        thermdll(ref t, ref ldensity, xliq, ref ptherm, ref e, ref h, ref s, ref cv,
                            ref cp, ref w, ref hjt);
                        trnprpdll(ref t, ref ldensity, xliq, ref eta, ref tcx, ref ierr, ref herr,
                            ref herr_length);
                    }
                    else
                    {
                        thermdll(ref t, ref vdensity, yvap, ref ptherm, ref e, ref h, ref s, ref cv,
                            ref cp, ref w, ref hjt);
                        trnprpdll(ref t, ref vdensity, yvap, ref eta, ref tcx, ref ierr, ref herr,
                            ref herr_length);
                    }
                    if (ierr == 0)
                        retval = true;
                    else
                        herrstr = herr;
                }
                else
                    herrstr = herr;
            }
            else
                herrstr = "sattdll(), thermdll(), and/or trnprpdll() subroutine not found";
            return retval;
        }

        // sattdpp - Calculate saturated thermodynamic/thermophysical properties given pressure (kPa)
        // return values:
        //    true = success
        //    false = failure.  See herrstr for error message
        public bool sattdpp(double p, int kph, ref double t, ref double ldensity, ref double vdensity, ref double h,
            ref double s, ref double cv, ref double cp, ref double w, ref double eta, ref double tcx)
        {
            bool retval = false;
            int ierr = 0, herr_length = 255;
            double e = 0.0, hjt = 0.0, ptherm = 0.0;
            double[] xliq = new double[MaxComps], yvap = new double[MaxComps];
            string herr = new string(' ', herr_length);
            if (satpdll != null && thermdll != null && trnprpdll != null)
            {
                satpdll(ref p, z, ref kph, ref t, ref ldensity, ref vdensity, xliq, yvap, ref ierr,
                    ref herr, ref herr_length);
                if (ierr == 0)
                {
                    if (kph == BUBBLEPOINT)
                    {
                        thermdll(ref t, ref ldensity, xliq, ref ptherm, ref e, ref h, ref s, ref cv,
                            ref cp, ref w, ref hjt);
                        trnprpdll(ref t, ref ldensity, xliq, ref eta, ref tcx, ref ierr, ref herr,
                            ref herr_length);
                    }
                    else
                    {
                        thermdll(ref t, ref vdensity, yvap, ref ptherm, ref e, ref h, ref s, ref cv,
                            ref cp, ref w, ref hjt);
                        trnprpdll(ref t, ref vdensity, yvap, ref eta, ref tcx, ref ierr, ref herr,
                            ref herr_length);
                    }
                    if (ierr == 0)
                        retval = true;
                    else
                        herrstr = herr;
                }
                else
                    herrstr = herr;
            }
            else
                herrstr = "satpdll(), thermdll(), and/or trnprpdll() subroutine not found";
            return retval;
        }

        // flshtdp - Calculate thermodynamic/thermophysical properties given temperature (K) and pressure (kPa)
        // return values:
        //    true = success
        //    false = failure.  See herrstr for error message
        public bool flshtdp(double t, double p, ref double ldensity, ref double vdensity, ref double h,
            ref double q, ref double s, ref double cv, ref double cp, ref double w, ref double eta, ref double tcx)
        {
            bool retval = false;
            int ierr = 0, herr_length = 255;
            double e = 0.0, rhobulk = 0.0, hjt = 0.0, ptherm = 0.0;
            double[] xliq = new double[MaxComps], yvap = new double[MaxComps];
            string herr = new string(' ', herr_length);
            if (tpflshdll != null)
            {
                tpflshdll(ref t, ref p, z, ref rhobulk, ref ldensity, ref vdensity, xliq,
                    yvap, ref q, ref e, ref h, ref s, ref cv, ref cp, ref w, ref ierr,
                    ref herr, ref herr_length);
                if (ierr == 0)
                {
                    if (q < 0.0)
                    {
                        thermdll(ref t, ref ldensity, xliq, ref ptherm, ref e, ref h, ref s, ref cv,
                            ref cp, ref w, ref hjt);
                        trnprpdll(ref t, ref ldensity, xliq, ref eta, ref tcx, ref ierr, ref herr,
                            ref herr_length);
                        if (ierr == 0)
                            retval = true;
                        else
                            herrstr = herr;
                    }
                    else if (q > 1.0)
                    {
                        thermdll(ref t, ref vdensity, yvap, ref ptherm, ref e, ref h, ref s, ref cv,
                            ref cp, ref w, ref hjt);
                        trnprpdll(ref t, ref vdensity, yvap, ref eta, ref tcx, ref ierr, ref herr,
                            ref herr_length);
                    }
                }
                else
                    herrstr = herr;
            }
            else
                herrstr = "tpflshdll() subroutine not found";
            return retval;
        }

        public bool setref(string refstr)
        {
            bool retflag = false;
            if (refstr == "12")
            {
                bubbleflag = false;
                NISTSetup("r12.fld");
                retflag = true;
            }
            else if (refstr == "134a")
            {
                bubbleflag = false;
                NISTSetup("r134a.fld");
                retflag = true;
            }
            else if (refstr == "22")
            {
                bubbleflag = false;
                NISTSetup("r22.fld");
                retflag = true;
            }
            else if (refstr == "152a")
            {
                bubbleflag = false;
                NISTSetup("r152a.fld");
                retflag = true;
            }
            else if (refstr == "245fa")
            {
                bubbleflag = false;
                NISTSetup("r245fa.fld");
                retflag = true;
            }
            else if (refstr == "290" || refstr == "290 (propane)" || refstr == "propane")
            {
                bubbleflag = false;
                NISTSetup("propane.fld");
                retflag = true;
            }
            else if (refstr == "32")
            {
                bubbleflag = false;
                NISTSetup("r32.fld");
                retflag = true;
            }
            else if (refstr == "401A")
            {
                bubbleflag = true;
                NISTSetup("R401A.mix");
                retflag = true;
            }
            else if (refstr == "402A")
            {
                bubbleflag = true;
                NISTSetup("R402A.mix");
                retflag = true;
            }
            else if (refstr == "404A")
            {
                bubbleflag = true;
                NISTSetup("R404A.mix");
                retflag = true;
            }
            else if (refstr == "407A")
            {
                bubbleflag = true;
                NISTSetup("R407A.mix");
                retflag = true;
            }
            else if (refstr == "407C")
            {
                bubbleflag = true;
                NISTSetup("R407C.mix");
                retflag = true;
            }
            else if (refstr == "407F")
            {
                bubbleflag = true;
                NISTSetup("R407F.mix");
                retflag = true;
            }
            else if (refstr == "407H")
            {
                bubbleflag = true;
                NISTSetup("R407H.mix");
                retflag = true;
            }
            else if (refstr == "408A")
            {
                bubbleflag = true;
                NISTSetup("R408A.mix");
                retflag = true;
            }
            else if (refstr == "409A")
            {
                bubbleflag = true;
                NISTSetup("R409A.mix");
                retflag = true;
            }
            else if (refstr == "410A")
            {
                bubbleflag = true;
                NISTSetup("R410A.mix");
                retflag = true;
            }
            else if (refstr == "422B")
            {
                bubbleflag = true;
                NISTSetup("R422B.mix");
                retflag = true;
            }
            else if (refstr == "422D")
            {
                bubbleflag = true;
                NISTSetup("R422D.mix");
                retflag = true;
            }
            else if (refstr == "427A")
            {
                bubbleflag = true;
                NISTSetup("R427A.mix");
                retflag = true;
            }
            else if (refstr == "438A")
            {
                bubbleflag = true;
                NISTSetup("R438A.mix");
                retflag = true;
            }
            else if (refstr == "441A")
            {
                bubbleflag = true;
                NISTSetup("R441A.mix");
                retflag = true;
            }
            else if (refstr == "442A")
            {
                bubbleflag = true;
                NISTSetup("R442A.mix");
                retflag = true;
            }
            else if (refstr == "447A")
            {
                bubbleflag = true;
                NISTSetup("R447A.mix");
                retflag = true;
            }
            else if (refstr == "448A")
            {
                bubbleflag = true;
                NISTSetup("R448A.mix");
                retflag = true;
            }
            else if (refstr == "449A")
            {
                bubbleflag = true;
                NISTSetup("R449A.mix");
                retflag = true;
            }
            else if (refstr == "450A")
            {
                bubbleflag = true;
                NISTSetup("R450A.mix");
                retflag = true;
            }
            else if (refstr == "452A")
            {
                bubbleflag = true;
                NISTSetup("R452A.mix");
                retflag = true;
            }
            else if (refstr == "452B")
            {
                bubbleflag = true;
                NISTSetup("R452B.mix");
                retflag = true;
            }
            else if (refstr == "454A")
            {
                bubbleflag = true;
                NISTSetup("R454A.mix");
                retflag = true;
            }
            else if (refstr == "454B")
            {
                bubbleflag = true;
                NISTSetup("R454B.mix");
                retflag = true;
            }
            else if (refstr == "454C")
            {
                bubbleflag = true;
                NISTSetup("R454C.mix");
                retflag = true;
            }
            else if (refstr == "455A")
            {
                bubbleflag = true;
                NISTSetup("R455A.mix");
                retflag = true;
            }
            else if (refstr == "458A")
            {
                bubbleflag = true;
                NISTSetup("R458A.mix");
                retflag = true;
            }
            else if (refstr == "466A")
            {
                bubbleflag = true;
                NISTSetup("R32;4900;R125;1150;CF3I;3950 mass");
                retflag = true;
            }
            else if (refstr == "470A")
            {
                bubbleflag = true;
                NISTSetup("CO2;1000;R32;1700;R125;1900;R134a;700;R227ea;300;R1234zee;4400 mass");
                retflag = true;
            }
            else if (refstr == "471A")
            {
                bubbleflag = true;
                NISTSetup("R1234zee;7870;R227ea;430;R1336mzze;1700 mass");
                retflag = true;
            }
            else if (refstr == "507")
            {
                bubbleflag = false;
                NISTSetup("R507A.mix");
                retflag = true;
            }
            else if (refstr == "513A")
            {
                bubbleflag = false;
                NISTSetup("R513A.mix");
                retflag = true;
            }
            else if (refstr == "515B")
            {
                bubbleflag = false;
                NISTSetup("R1234zee;9110;R227ea;890 mass");
                retflag = true;
            }
            else if (refstr == "600a" || refstr == "600a (isobutane)" || refstr == "isobutane")
            {
                bubbleflag = false;
                NISTSetup("isobutan.fld");
                retflag = true;
            }
            else if (refstr == "717" || refstr == "717 (ammonia)" || refstr == "ammonia")
            {
                bubbleflag = false;
                NISTSetup("ammonia.fld");
                retflag = true;
            }
            else if (refstr == "718" || refstr == "718 (H2O)" || refstr == "718 (water)" || refstr == "H2O" || refstr == "water")
            {
                bubbleflag = false;
                NISTSetup("water.fld");
                retflag = true;
            }
            else if (refstr == "728" || refstr == "728 (nitrogen)" || refstr == "nitrogen")
            {
                bubbleflag = false;
                NISTSetup("nitrogen.fld");
                retflag = true;
            }
            else if (refstr == "744" || refstr == "744 (CO2)" || refstr == "CO2" || refstr == "carbon dioxide")
            {
                bubbleflag = false;
                NISTSetup("co2.fld");
                retflag = true;
            }
            else if (refstr == "744A" || refstr == "744A (N2O)" || refstr == "N2O" || refstr == "nitrous oxide")
            {
                bubbleflag = false;
                NISTSetup("n2o.fld");
                retflag = true;
            }
            else if (refstr == "1234yf")
            {
                bubbleflag = false;
                NISTSetup("R1234YF.fld");
                retflag = true;
            }
            else if (refstr == "1234zee" || refstr == "1234ze(E)")
            {
                bubbleflag = false;
                NISTSetup("R1234ZEE.fld");
                retflag = true;
            }
            return retflag;
        }
    }
}