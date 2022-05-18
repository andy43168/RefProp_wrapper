using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RefProp_wrapper;

namespace TestApp
{
    public partial class Form1 : Form
    {
        InpData rpd = new InpData();
        iRefProp irp = new iRefProp();
        public Form1()
        {
            InitializeComponent();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            if (iRefProp.DLLinit() == iRefProp.NoDLL)
                MessageBox.Show(this, "Unable to locate NIST RefProp DLL file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            iRefProp.DLLend();
        }
        string? tstr, pstr, rstr, hstr, sstr, vstr, etastr, tcxstr;

        public static Func<double, double>? tin;
        public static Func<double, double>? tout;
        public static Func<double, double>? pin;
        public static Func<double, double>? pout;
        public static Func<double, double, double>? rout;
        public static Func<double, double, double>? hout;
        public static Func<double, double, double>? sout;
        public static Func<double, double>? vout;
        public static Func<double, double>? etaout;
        public static Func<double, double>? tcxout;
        public void setuom(int uom)
        {
            if (uom == 0)
            {
                tstr = "°F";
                pstr = "psia";
                rstr = "lbm/ft\u00B3";
                hstr = "Btu/lbm";
                sstr = "Btu/(lbm-°R)";
                vstr = "ft/s";
                etastr = "lbm/(ft-s)";
                tcxstr = "Btu/(h-ft-°R)";
                tin = iRefProp.ftok;
                tout = iRefProp.ktof;
                pin = iRefProp.psia_kpa;
                pout = iRefProp.kpa_psia;
                rout = iRefProp.moll_lbft3;
                hout = iRefProp.jmol_blb;
                sout = iRefProp.jmolk_blbr;
                vout = iRefProp.m_ft;
                etaout = iRefProp.upas_lbfts;
                tcxout = iRefProp.wmk_btuitfthft2f;
            }
            else
            {
                tstr = "K";
                pstr = "kPa";
                rstr = "kg/m\u00B3";
                hstr = "kJ/kg";
                sstr = "kJ/(kg-K)";
                vstr = "m/s";
                etastr = "\u00B5Pa-s";
                tcxstr = "W/(m-K)";
                tin = iRefProp.noconvert;
                tout = iRefProp.noconvert;
                pin = iRefProp.noconvert;
                pout = iRefProp.noconvert;
                rout = iRefProp.moll_kgm3;
                hout = iRefProp.jmol_jgm;
                sout = iRefProp.jmolk_jgmk;
                vout = iRefProp.noconvert;
                etaout = iRefProp.noconvert;
                tcxout = iRefProp.noconvert;
            }
        }

        // As noted in the iRefProp.cs file, Visual Studio 2022 gets annoyed when a nullable
        // variable is used where having a null value will create issues.
        // This is a test app.
        // These null warnings serve little purpose.
        // pragma suppresses these warning messages.

#pragma warning disable CS8602
#pragma warning disable CS8604

        private void preferencesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            double tinp, pinp, p = 0.0, pb = 0.0, t = 0.0, tb = 0.0, ldensity = 0.0, vdensity = 0.0,
                h = 0.0, hf = 0.0, hg = 0.0, q = 0.0, s = 0.0, sf = 0.0, sg = 0.0, cv = 0.0,
                cvf = 0.0, cvg = 0.0, cp = 0.0, cpf = 0.0, cpg = 0.0, w = 0.0, wf = 0.0, wg = 0.0,
                eta = 0.0, etaf = 0.0, etag = 0.0, tcx = 0.0, tcxf = 0.0, tcxg = 0.0, ignore = 0.0;
            Form2 preferences = new Form2(rpd);
            DialogResult result = preferences.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (irp.setref(rpd.refstr))
                {
                    if (String.IsNullOrEmpty(rpd.temp) && String.IsNullOrEmpty(rpd.press))
                        MessageBox.Show(this, "Temperature and/or pressure value is required", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        listBox1.Items.Clear();
                        setuom(rpd.uom);
                        listBox1.Items.Add("Refrigerant " + rpd.refstr);
                        if (!String.IsNullOrEmpty(rpd.temp) && String.IsNullOrEmpty(rpd.press))
                        {
                            Double.TryParse(rpd.temp, out tinp);
                            t = tin(tinp);
                            irp.sattdpt(t, iRefProp.BUBBLEPOINT, ref pb, ref ldensity, ref ignore, ref hf, ref sf, ref cvf, ref cpf,
                                ref wf, ref etaf, ref tcxf);
                            irp.sattdpt(t, iRefProp.DEWPOINT, ref p, ref ignore, ref vdensity, ref hg, ref sg, ref cvg, ref cpg,
                                ref wg, ref etag, ref tcxg);
                            listBox1.Items.Add("Temperature " + tstr + ": " + String.Format("{0} (input)", tinp));
                            listBox1.Items.Add("Bubble Point Pressure " + pstr + ": " + String.Format("{0:0.00}", pout(pb)));
                            listBox1.Items.Add("Dew Point Pressure " + pstr + ": " + String.Format("{0:0.00}", pout(p)));
                            listBox1.Items.Add("Liquid Density " + rstr + ": " + String.Format("{0:0.000}", rout(ldensity, irp.MW)));
                            listBox1.Items.Add("Vapor Density " + rstr + ": " + String.Format("{0:0.000}", rout(vdensity, irp.MW)));
                            listBox1.Items.Add("Liquid Enthalpy " + hstr + ": " + String.Format("{0:0.00}", hout(hf, irp.MW)));
                            listBox1.Items.Add("Vapor Enthalpy " + hstr + ": " + String.Format("{0:0.00}", hout(hg, irp.MW)));
                            listBox1.Items.Add("Liquid Entropy " + sstr + ": " + String.Format("{0:0.0000}", sout(sf, irp.MW)));
                            listBox1.Items.Add("Vapor Entropy " + sstr + ": " + String.Format("{0:0.0000}", sout(sg, irp.MW)));
                            listBox1.Items.Add("Liquid Isochoric Specific Heat " + sstr + ": " + String.Format("{0:0.0000}", sout(cvf, irp.MW)));
                            listBox1.Items.Add("Vapor Isochoric Specific Heat " + sstr + ": " + String.Format("{0:0.0000}", sout(cvg, irp.MW)));
                            listBox1.Items.Add("Liquid Isobaric Specific Heat" + sstr + ": " + String.Format("{0:0.0000}", sout(cpf, irp.MW)));
                            listBox1.Items.Add("Vapor Isobaric Specific Heat" + sstr + ": " + String.Format("{0:0.0000}", sout(cpg, irp.MW)));
                            listBox1.Items.Add("Liquid Speed of Sound " + vstr + ": " + String.Format("{0:0.00}", vout(wf)));
                            listBox1.Items.Add("Vapor Speed of Sound " + vstr + ": " + String.Format("{0:0.00}", vout(wg)));
                            listBox1.Items.Add("Liquid Dynamic Viscosity " + etastr + ": " + String.Format("{0:e4}", etaout(etaf)));
                            listBox1.Items.Add("Vapor Dynamic Viscosity " + etastr + ": " + String.Format("{0:e4}", etaout(etag)));
                            listBox1.Items.Add("Liquid Thermal Conductivity " + tcxstr + ": " + String.Format("{0:e4}", tcxout(tcxf)));
                            listBox1.Items.Add("Vapor Thermal Conductivity " + tcxstr + ": " + String.Format("{0:e4}", tcxout(tcxg)));
                        }
                        else if (String.IsNullOrEmpty(rpd.temp) && !String.IsNullOrEmpty(rpd.press))
                        {
                            Double.TryParse(rpd.press, out pinp);
                            p = pin(pinp);
                            irp.sattdpp(p, iRefProp.BUBBLEPOINT, ref tb, ref ldensity, ref ignore, ref hf, ref sf, ref cvf, ref cpf,
                                ref wf, ref etaf, ref tcxf);
                            irp.sattdpp(p, iRefProp.DEWPOINT, ref t, ref ignore, ref vdensity, ref hg, ref sg, ref cvg, ref cpg,
                                ref wg, ref etag, ref tcxg);
                            listBox1.Items.Add("Pressure " + pstr + ": " + String.Format("{0} (input)", pinp));
                            listBox1.Items.Add("Bubble Point Temperature " + tstr + ": " + String.Format("{0:0.00}", tout(tb)));
                            listBox1.Items.Add("Dew Point Temperature " + tstr + ": " + String.Format("{0:0.00}", tout(t)));
                            listBox1.Items.Add("Liquid Density " + rstr + ": " + String.Format("{0:0.000}", rout(ldensity, irp.MW)));
                            listBox1.Items.Add("Vapor Density " + rstr + ": " + String.Format("{0:0.000}", rout(vdensity, irp.MW)));
                            listBox1.Items.Add("Liquid Enthalpy " + hstr + ": " + String.Format("{0:0.00}", hout(hf, irp.MW)));
                            listBox1.Items.Add("Vapor Enthalpy " + hstr + ": " + String.Format("{0:0.00}", hout(hg, irp.MW)));
                            listBox1.Items.Add("Liquid Entropy " + sstr + ": " + String.Format("{0:0.0000}", sout(sf, irp.MW)));
                            listBox1.Items.Add("Vapor Entropy " + sstr + ": " + String.Format("{0:0.0000}", sout(sg, irp.MW)));
                            listBox1.Items.Add("Liquid Isochoric Specific Heat " + sstr + ": " + String.Format("{0:0.0000}", sout(cvf, irp.MW)));
                            listBox1.Items.Add("Vapor Isochoric Specific Heat " + sstr + ": " + String.Format("{0:0.0000}", sout(cvg, irp.MW)));
                            listBox1.Items.Add("Liquid Isobaric Specific Heat " + sstr + ": " + String.Format("{0:0.0000}", sout(cpf, irp.MW)));
                            listBox1.Items.Add("Vapor Isobaric Specific Heat " + sstr + ": " + String.Format("{0:0.0000}", sout(cpg, irp.MW)));
                            listBox1.Items.Add("Liquid Speed of Sound " + vstr + ": " + String.Format("{0:0.00}", vout(wf)));
                            listBox1.Items.Add("Vapor Speed of Sound " + vstr + ": " + String.Format("{0:0.00}", vout(wg)));
                            listBox1.Items.Add("Liquid Dynamic Viscosity " + etastr + ": " + String.Format("{0:e4}", etaout(etaf)));
                            listBox1.Items.Add("Vapor Dynamic Viscosity " + etastr + ": " + String.Format("{0:e4}", etaout(etag)));
                            listBox1.Items.Add("Liquid Thermal Conductivity " + tcxstr + ": " + String.Format("{0:e4}", tcxout(tcxf)));
                            listBox1.Items.Add("Vapor Thermal Conductivity " + tcxstr + ": " + String.Format("{0:e4}", tcxout(tcxg)));
                        }
                        else // !String.IsNullOrEmpty(rpd.temp) && !String.IsNullOrEmpty(rpd.press)
                        {
                            Double.TryParse(rpd.temp, out tinp);
                            Double.TryParse(rpd.press, out pinp);
                            t = tin(tinp);
                            p = pin(pinp);
                            irp.flshtdp(t, p, ref ldensity, ref vdensity, ref h, ref q, ref s, ref cv, ref cp, ref w, ref eta, ref tcx);
                            listBox1.Items.Add("Temperature " + tstr + ": " + String.Format("{0} (input)", tinp));
                            listBox1.Items.Add("Pressure " + pstr + ": " + String.Format("{0} (inpot)", pinp));
                            if (q < 0.0)
                            {
                                listBox1.Items.Add("Subcooled Liquid");
                                listBox1.Items.Add("Liquid Density " + rstr + ": " + String.Format("{0:0.000}", rout(ldensity, irp.MW)));
                            }
                            else if (q > 1.0)
                            {
                                listBox1.Items.Add("Superheated Vapor");
                                listBox1.Items.Add("Vapor Density " + rstr + ": " + String.Format("{0:0.000}", rout(vdensity, irp.MW)));
                            }
                            else // saturated refrigerant
                            {
                                listBox1.Items.Add("Liquid Density " + rstr + ": " + String.Format("{0:0.000}", rout(ldensity, irp.MW)));
                                listBox1.Items.Add("Vapor Density " + rstr + ": " + String.Format("{0:0.000}", rout(vdensity, irp.MW)));
                                listBox1.Items.Add("Saturated, Refrigerant Quality: " + String.Format("{0:0.0000}", q));
                            }
                            listBox1.Items.Add("Enthalpy " + hstr + ": " + String.Format("{0:0.00}", hout(h, irp.MW)));
                            listBox1.Items.Add("Entropy " + sstr + ": " + String.Format("{0:0.0000}", sout(s, irp.MW)));
                            if (q < 0.0 || q > 1.0)
                            {
                                listBox1.Items.Add("Isochoric Specific Heat " + sstr + ": " + String.Format("{0:0.0000}",
                                    sout(cv, irp.MW)));
                                listBox1.Items.Add("Isobaric Specific Heat " + sstr + ": " + String.Format("{0:0.0000}",
                                    sout(cp, irp.MW)));
                                listBox1.Items.Add("Speed of Sound " + vstr + ": " + String.Format("{0:0.00}", vout(w)));
                                listBox1.Items.Add("Dynamic Viscosity " + etastr + ": " + String.Format("{0:e4}", etaout(eta)));
                                listBox1.Items.Add("Thermal Conductivity " + tcxstr + ": " + String.Format("{0:e4}",
                                    tcxout(tcx)));
                            }
                        }
                        listBox1.Items.Add("Critical Temperature " + tstr + ": " + String.Format("{0:0.00}", tout(irp.TC)));
                        listBox1.Items.Add("Critical Pressure " + pstr + ": " + String.Format("{0:0.00}", pout(irp.PC)));
                        listBox1.Items.Add("Critical Density " + rstr + ": " + String.Format("{0:0.000}", rout(irp.DC, irp.MW)));
                        listBox1.Items.Add("Molecular Weight " + ": " + String.Format("{0:0.00}", irp.MW));
                        listBox1.Items.Add("RefProp Version " + ": " + String.Format("{0:0.0000}", iRefProp.dllversetup()));
                    }
                }
            }
        }
    }
    public partial class InpData
    {
        public int uom;
        public string? refstr;
        public string? temp;
        public string? press;
    }
}