using System;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace TimingTest
{
	class Test
	{

		[DllImport("KERNEL32")]
        public static extern bool QueryPerformanceCounter(ref Int64 nPfCt);

        [DllImport("KERNEL32")]
        public static extern bool QueryPerformanceFrequency(ref Int64 nPfFreq);

        [DllImport("kernel32.dll")]
        static extern bool FlushInstructionCache(IntPtr hProcess, IntPtr lpBaseAddress, UIntPtr dwSize);

        static int[,] cachePolluter = new int[1024, 1024];
        static Random rand = new Random();
        static int initialized = 0;

        static int polluteDataCache()
        {
            int ret, rnd;
            ret = rnd = 0;

            if (0 == initialized)
            {
                for (int i = 0; i < 1024; i++)
                {
                    for (int k = 0; k < 1024; k++)
                    {
                        cachePolluter[i, k] = rand.Next();
                    }
                }
                initialized = 1;
            }

            for (int i = 0; i < 1024; i++)
            {
                for (int k = 0; k < 1024; k++)
                {
                    rnd = rand.Next();
                    cachePolluter[i, k] = (cachePolluter[i, k] + rand.Next()) % ((rnd > 0) ? rnd : 1);
                }
            }

            for (int i = 0; i < 1024; i++)
            {
                for (int k = 0; k < 1024; k++)
                {
                    rnd = rand.Next();
                    ret = (ret + cachePolluter[i, k]) % ((rnd > 0) ? rnd : 1);
                }
            }
            return ret;
        }
		
		static int Main (string[] args)
		{
			#region startuptime
            //
            // Aufruf aus Shell: mono 1000Methods-startup.exe timingsJIT.txt $(date +%s::%N)
            //
            DateTime mainEntryTime = DateTime.Now;
            string sHours;
            string sMinutes;
            string sSeconds;
            string sMilliseconds;
            string sYear;
            string sMonth;
            string sDay;

            int iHours;
            int iMinutes;
            int iSeconds;
            int iMilliseconds;
            int iYear;
            int iMonth;
            int iDay;
            long lStartupTime;

            bool success;
            DateTime launchTime;
            TimeSpan startupTime;
            int result = 0;
            int index = 0;

//            Console.WriteLine("args.Length: " + args.Length.ToString());
//            for (int i = 0; i < args.Length; i++)
//            {
//                //Console.WriteLine("Arg Nr. " + i.ToString() + ": " + args[i]);
//            }
            if ((args.Length < 2) || (args[0] == String.Empty) || (args[1] == String.Empty)) 
            {
                Console.WriteLine("Arguments missing!");
                Console.WriteLine("Usage: 1000Methods-startup.exe %time% %date%");
                Console.WriteLine("Exit.");
                return 1;
            }
            index = args[0].IndexOf(":");
            sHours = args[0].Substring(0, index);
//            Console.WriteLine("hours: " + sHours);

            sMinutes = args[0].Substring((index+1), 2);
            //sMinutes = args[0].Substring(3, 2);
//            Console.WriteLine("minutes: " + sMinutes);

            sSeconds = args[0].Substring((index+4), 2);
            //sSeconds = args[0].Substring(6, 2);
//           Console.WriteLine("seconds: " + sSeconds);

            sMilliseconds = args[0].Substring((index+7), 2);
            //sMilliseconds = args[0].Substring(9, 2);
//            Console.WriteLine("milliseconds: " + sMilliseconds);

            sDay = args[1].Substring(0, 2);
//            Console.WriteLine("day: " + sDay);

            sMonth = args[1].Substring(3, 2);
//            Console.WriteLine("month: " + sMonth);

            sYear = args[1].Substring(6, 4);
//            Console.WriteLine("year: " + sYear);

            success = Int32.TryParse(sHours, out iHours);
            if (success == false)
                return 4;
//            Console.WriteLine("iHours: " + iHours);

            success = Int32.TryParse(sMinutes, out iMinutes);
            if (success == false)
                return 4;
//            Console.WriteLine("iMinutes: " + iMinutes);

            success = Int32.TryParse(sSeconds, out iSeconds);
            if (success == false)
                return 4;
//            Console.WriteLine("iSeconds: " + iSeconds);

            success = Int32.TryParse(sMilliseconds, out iMilliseconds);
            if (success == false)
                return 4;
//            Console.WriteLine("iMilliseconds: " + iMilliseconds);

            success = Int32.TryParse(sYear, out iYear);
            if (success == false)
                return 4;
//            Console.WriteLine("iYear: " + iYear);

            success = Int32.TryParse(sMonth, out iMonth);
            if (success == false)
                return 4;
//            Console.WriteLine("iMonth: " + iMonth);

            success = Int32.TryParse(sDay, out iDay);
            if (success == false)
                return 4;
//            Console.WriteLine("iDay: " + iDay);

            launchTime = new DateTime(iYear, iMonth, iDay, iHours, iMinutes, iSeconds, iMilliseconds);
/*
            Console.WriteLine("MainEntryTime hr: " + mainEntryTime.Hour.ToString());
            Console.WriteLine("MainEntryTime min: " + mainEntryTime.Minute.ToString());
            Console.WriteLine("MainEntryTime sec: " + mainEntryTime.Second.ToString());
            Console.WriteLine("MainEntryTime ms: " + mainEntryTime.Millisecond.ToString());

            Console.WriteLine("Launchtime hr: " + launchTime.Hour.ToString());
            Console.WriteLine("Launchtime min: " + launchTime.Minute.ToString());
            Console.WriteLine("Launchtime sec: " + launchTime.Second.ToString());
            Console.WriteLine("Launchtime ms: " + launchTime.Millisecond.ToString());
*/
            startupTime = mainEntryTime - launchTime;
            lStartupTime = startupTime.Ticks / TimeSpan.TicksPerMillisecond;
            Console.WriteLine(lStartupTime.ToString());
			#endregion
			
			#region measurement_initialization
			Int64 tsc_before, tsc_after, overhead, min, frequency, time1, time2, time3, time4;
            tsc_before = tsc_after = overhead = min = frequency = time1 = time2 = time3 = time4 = 0;

            int cnt, sum;
            sum = 0;
            IntPtr procHandle = Process.GetCurrentProcess().Handle;
            bool retVal = true;

            TextWriter tempfile = new StreamWriter("tempfile");

            for (cnt = 0; cnt < 5; cnt++)
            {
                sum += polluteDataCache();
                retVal &= FlushInstructionCache(procHandle, IntPtr.Zero, UIntPtr.Zero);

                QueryPerformanceCounter(ref tsc_before);

                if (cnt < 4)
                {
                    sum++;
                }
                else
                {
                    sum--;
                }

                QueryPerformanceCounter(ref tsc_after);
                overhead = tsc_after - tsc_before;

                if (cnt == 0)
                    min = overhead;

                //Console.WriteLine ("Overhead " + cnt + ": " + overhead + " CPU cycles");

                if (overhead < min)
                {
                    min = overhead;
                }
            }

            Console.WriteLine("Final Overhead: " + min + " Ticks");
			#endregion
			
			#region VariablenDeklaration
			int var0=0;
			int var1=1;
			int var2=2;
			int var3=3;
			int var4=4;
			int var5=5;
			int var6=6;
			int var7=7;
			int var8=8;
			int var9=9;
			int var10=10;
			int var11=11;
			int var12=12;
			int var13=13;
			int var14=14;
			int var15=15;
			int var16=16;
			int var17=17;
			int var18=18;
			int var19=19;
			int var20=20;
			int var21=21;
			int var22=22;
			int var23=23;
			int var24=24;
			int var25=25;
			int var26=26;
			int var27=27;
			int var28=28;
			int var29=29;
			int var30=30;
			int var31=31;
			int var32=32;
			int var33=33;
			int var34=34;
			int var35=35;
			int var36=36;
			int var37=37;
			int var38=38;
			int var39=39;
			int var40=40;
			int var41=41;
			int var42=42;
			int var43=43;
			int var44=44;
			int var45=45;
			int var46=46;
			int var47=47;
			int var48=48;
			int var49=49;
			int var50=50;
			int var51=51;
			int var52=52;
			int var53=53;
			int var54=54;
			int var55=55;
			int var56=56;
			int var57=57;
			int var58=58;
			int var59=59;
			int var60=60;
			int var61=61;
			int var62=62;
			int var63=63;
			int var64=64;
			int var65=65;
			int var66=66;
			int var67=67;
			int var68=68;
			int var69=69;
			int var70=70;
			int var71=71;
			int var72=72;
			int var73=73;
			int var74=74;
			int var75=75;
			int var76=76;
			int var77=77;
			int var78=78;
			int var79=79;
			int var80=80;
			int var81=81;
			int var82=82;
			int var83=83;
			int var84=84;
			int var85=85;
			int var86=86;
			int var87=87;
			int var88=88;
			int var89=89;
			int var90=90;
			int var91=91;
			int var92=92;
			int var93=93;
			int var94=94;
			int var95=95;
			int var96=96;
			int var97=97;
			int var98=98;
			int var99=99;
			int var100=100;
			int var101=101;
			int var102=102;
			int var103=103;
			int var104=104;
			int var105=105;
			int var106=106;
			int var107=107;
			int var108=108;
			int var109=109;
			int var110=110;
			int var111=111;
			int var112=112;
			int var113=113;
			int var114=114;
			int var115=115;
			int var116=116;
			int var117=117;
			int var118=118;
			int var119=119;
			int var120=120;
			int var121=121;
			int var122=122;
			int var123=123;
			int var124=124;
			int var125=125;
			int var126=126;
			int var127=127;
			int var128=128;
			int var129=129;
			int var130=130;
			int var131=131;
			int var132=132;
			int var133=133;
			int var134=134;
			int var135=135;
			int var136=136;
			int var137=137;
			int var138=138;
			int var139=139;
			int var140=140;
			int var141=141;
			int var142=142;
			int var143=143;
			int var144=144;
			int var145=145;
			int var146=146;
			int var147=147;
			int var148=148;
			int var149=149;
			int var150=150;
			int var151=151;
			int var152=152;
			int var153=153;
			int var154=154;
			int var155=155;
			int var156=156;
			int var157=157;
			int var158=158;
			int var159=159;
			int var160=160;
			int var161=161;
			int var162=162;
			int var163=163;
			int var164=164;
			int var165=165;
			int var166=166;
			int var167=167;
			int var168=168;
			int var169=169;
			int var170=170;
			int var171=171;
			int var172=172;
			int var173=173;
			int var174=174;
			int var175=175;
			int var176=176;
			int var177=177;
			int var178=178;
			int var179=179;
			int var180=180;
			int var181=181;
			int var182=182;
			int var183=183;
			int var184=184;
			int var185=185;
			int var186=186;
			int var187=187;
			int var188=188;
			int var189=189;
			int var190=190;
			int var191=191;
			int var192=192;
			int var193=193;
			int var194=194;
			int var195=195;
			int var196=196;
			int var197=197;
			int var198=198;
			int var199=199;
			int var200=200;
			int var201=201;
			int var202=202;
			int var203=203;
			int var204=204;
			int var205=205;
			int var206=206;
			int var207=207;
			int var208=208;
			int var209=209;
			int var210=210;
			int var211=211;
			int var212=212;
			int var213=213;
			int var214=214;
			int var215=215;
			int var216=216;
			int var217=217;
			int var218=218;
			int var219=219;
			int var220=220;
			int var221=221;
			int var222=222;
			int var223=223;
			int var224=224;
			int var225=225;
			int var226=226;
			int var227=227;
			int var228=228;
			int var229=229;
			int var230=230;
			int var231=231;
			int var232=232;
			int var233=233;
			int var234=234;
			int var235=235;
			int var236=236;
			int var237=237;
			int var238=238;
			int var239=239;
			int var240=240;
			int var241=241;
			int var242=242;
			int var243=243;
			int var244=244;
			int var245=245;
			int var246=246;
			int var247=247;
			int var248=248;
			int var249=249;
			int var250=250;
			int var251=251;
			int var252=252;
			int var253=253;
			int var254=254;
			int var255=255;
			int var256=256;
			int var257=257;
			int var258=258;
			int var259=259;
			int var260=260;
			int var261=261;
			int var262=262;
			int var263=263;
			int var264=264;
			int var265=265;
			int var266=266;
			int var267=267;
			int var268=268;
			int var269=269;
			int var270=270;
			int var271=271;
			int var272=272;
			int var273=273;
			int var274=274;
			int var275=275;
			int var276=276;
			int var277=277;
			int var278=278;
			int var279=279;
			int var280=280;
			int var281=281;
			int var282=282;
			int var283=283;
			int var284=284;
			int var285=285;
			int var286=286;
			int var287=287;
			int var288=288;
			int var289=289;
			int var290=290;
			int var291=291;
			int var292=292;
			int var293=293;
			int var294=294;
			int var295=295;
			int var296=296;
			int var297=297;
			int var298=298;
			int var299=299;
			int var300=300;
			int var301=301;
			int var302=302;
			int var303=303;
			int var304=304;
			int var305=305;
			int var306=306;
			int var307=307;
			int var308=308;
			int var309=309;
			int var310=310;
			int var311=311;
			int var312=312;
			int var313=313;
			int var314=314;
			int var315=315;
			int var316=316;
			int var317=317;
			int var318=318;
			int var319=319;
			int var320=320;
			int var321=321;
			int var322=322;
			int var323=323;
			int var324=324;
			int var325=325;
			int var326=326;
			int var327=327;
			int var328=328;
			int var329=329;
			int var330=330;
			int var331=331;
			int var332=332;
			int var333=333;
			int var334=334;
			int var335=335;
			int var336=336;
			int var337=337;
			int var338=338;
			int var339=339;
			int var340=340;
			int var341=341;
			int var342=342;
			int var343=343;
			int var344=344;
			int var345=345;
			int var346=346;
			int var347=347;
			int var348=348;
			int var349=349;
			int var350=350;
			int var351=351;
			int var352=352;
			int var353=353;
			int var354=354;
			int var355=355;
			int var356=356;
			int var357=357;
			int var358=358;
			int var359=359;
			int var360=360;
			int var361=361;
			int var362=362;
			int var363=363;
			int var364=364;
			int var365=365;
			int var366=366;
			int var367=367;
			int var368=368;
			int var369=369;
			int var370=370;
			int var371=371;
			int var372=372;
			int var373=373;
			int var374=374;
			int var375=375;
			int var376=376;
			int var377=377;
			int var378=378;
			int var379=379;
			int var380=380;
			int var381=381;
			int var382=382;
			int var383=383;
			int var384=384;
			int var385=385;
			int var386=386;
			int var387=387;
			int var388=388;
			int var389=389;
			int var390=390;
			int var391=391;
			int var392=392;
			int var393=393;
			int var394=394;
			int var395=395;
			int var396=396;
			int var397=397;
			int var398=398;
			int var399=399;
			int var400=400;
			int var401=401;
			int var402=402;
			int var403=403;
			int var404=404;
			int var405=405;
			int var406=406;
			int var407=407;
			int var408=408;
			int var409=409;
			int var410=410;
			int var411=411;
			int var412=412;
			int var413=413;
			int var414=414;
			int var415=415;
			int var416=416;
			int var417=417;
			int var418=418;
			int var419=419;
			int var420=420;
			int var421=421;
			int var422=422;
			int var423=423;
			int var424=424;
			int var425=425;
			int var426=426;
			int var427=427;
			int var428=428;
			int var429=429;
			int var430=430;
			int var431=431;
			int var432=432;
			int var433=433;
			int var434=434;
			int var435=435;
			int var436=436;
			int var437=437;
			int var438=438;
			int var439=439;
			int var440=440;
			int var441=441;
			int var442=442;
			int var443=443;
			int var444=444;
			int var445=445;
			int var446=446;
			int var447=447;
			int var448=448;
			int var449=449;
			int var450=450;
			int var451=451;
			int var452=452;
			int var453=453;
			int var454=454;
			int var455=455;
			int var456=456;
			int var457=457;
			int var458=458;
			int var459=459;
			int var460=460;
			int var461=461;
			int var462=462;
			int var463=463;
			int var464=464;
			int var465=465;
			int var466=466;
			int var467=467;
			int var468=468;
			int var469=469;
			int var470=470;
			int var471=471;
			int var472=472;
			int var473=473;
			int var474=474;
			int var475=475;
			int var476=476;
			int var477=477;
			int var478=478;
			int var479=479;
			int var480=480;
			int var481=481;
			int var482=482;
			int var483=483;
			int var484=484;
			int var485=485;
			int var486=486;
			int var487=487;
			int var488=488;
			int var489=489;
			int var490=490;
			int var491=491;
			int var492=492;
			int var493=493;
			int var494=494;
			int var495=495;
			int var496=496;
			int var497=497;
			int var498=498;
			int var499=499;
			int var500=500;
			int var501=501;
			int var502=502;
			int var503=503;
			int var504=504;
			int var505=505;
			int var506=506;
			int var507=507;
			int var508=508;
			int var509=509;
			int var510=510;
			int var511=511;
			int var512=512;
			int var513=513;
			int var514=514;
			int var515=515;
			int var516=516;
			int var517=517;
			int var518=518;
			int var519=519;
			int var520=520;
			int var521=521;
			int var522=522;
			int var523=523;
			int var524=524;
			int var525=525;
			int var526=526;
			int var527=527;
			int var528=528;
			int var529=529;
			int var530=530;
			int var531=531;
			int var532=532;
			int var533=533;
			int var534=534;
			int var535=535;
			int var536=536;
			int var537=537;
			int var538=538;
			int var539=539;
			int var540=540;
			int var541=541;
			int var542=542;
			int var543=543;
			int var544=544;
			int var545=545;
			int var546=546;
			int var547=547;
			int var548=548;
			int var549=549;
			int var550=550;
			int var551=551;
			int var552=552;
			int var553=553;
			int var554=554;
			int var555=555;
			int var556=556;
			int var557=557;
			int var558=558;
			int var559=559;
			int var560=560;
			int var561=561;
			int var562=562;
			int var563=563;
			int var564=564;
			int var565=565;
			int var566=566;
			int var567=567;
			int var568=568;
			int var569=569;
			int var570=570;
			int var571=571;
			int var572=572;
			int var573=573;
			int var574=574;
			int var575=575;
			int var576=576;
			int var577=577;
			int var578=578;
			int var579=579;
			int var580=580;
			int var581=581;
			int var582=582;
			int var583=583;
			int var584=584;
			int var585=585;
			int var586=586;
			int var587=587;
			int var588=588;
			int var589=589;
			int var590=590;
			int var591=591;
			int var592=592;
			int var593=593;
			int var594=594;
			int var595=595;
			int var596=596;
			int var597=597;
			int var598=598;
			int var599=599;
			int var600=600;
			int var601=601;
			int var602=602;
			int var603=603;
			int var604=604;
			int var605=605;
			int var606=606;
			int var607=607;
			int var608=608;
			int var609=609;
			int var610=610;
			int var611=611;
			int var612=612;
			int var613=613;
			int var614=614;
			int var615=615;
			int var616=616;
			int var617=617;
			int var618=618;
			int var619=619;
			int var620=620;
			int var621=621;
			int var622=622;
			int var623=623;
			int var624=624;
			int var625=625;
			int var626=626;
			int var627=627;
			int var628=628;
			int var629=629;
			int var630=630;
			int var631=631;
			int var632=632;
			int var633=633;
			int var634=634;
			int var635=635;
			int var636=636;
			int var637=637;
			int var638=638;
			int var639=639;
			int var640=640;
			int var641=641;
			int var642=642;
			int var643=643;
			int var644=644;
			int var645=645;
			int var646=646;
			int var647=647;
			int var648=648;
			int var649=649;
			int var650=650;
			int var651=651;
			int var652=652;
			int var653=653;
			int var654=654;
			int var655=655;
			int var656=656;
			int var657=657;
			int var658=658;
			int var659=659;
			int var660=660;
			int var661=661;
			int var662=662;
			int var663=663;
			int var664=664;
			int var665=665;
			int var666=666;
			int var667=667;
			int var668=668;
			int var669=669;
			int var670=670;
			int var671=671;
			int var672=672;
			int var673=673;
			int var674=674;
			int var675=675;
			int var676=676;
			int var677=677;
			int var678=678;
			int var679=679;
			int var680=680;
			int var681=681;
			int var682=682;
			int var683=683;
			int var684=684;
			int var685=685;
			int var686=686;
			int var687=687;
			int var688=688;
			int var689=689;
			int var690=690;
			int var691=691;
			int var692=692;
			int var693=693;
			int var694=694;
			int var695=695;
			int var696=696;
			int var697=697;
			int var698=698;
			int var699=699;
			int var700=700;
			int var701=701;
			int var702=702;
			int var703=703;
			int var704=704;
			int var705=705;
			int var706=706;
			int var707=707;
			int var708=708;
			int var709=709;
			int var710=710;
			int var711=711;
			int var712=712;
			int var713=713;
			int var714=714;
			int var715=715;
			int var716=716;
			int var717=717;
			int var718=718;
			int var719=719;
			int var720=720;
			int var721=721;
			int var722=722;
			int var723=723;
			int var724=724;
			int var725=725;
			int var726=726;
			int var727=727;
			int var728=728;
			int var729=729;
			int var730=730;
			int var731=731;
			int var732=732;
			int var733=733;
			int var734=734;
			int var735=735;
			int var736=736;
			int var737=737;
			int var738=738;
			int var739=739;
			int var740=740;
			int var741=741;
			int var742=742;
			int var743=743;
			int var744=744;
			int var745=745;
			int var746=746;
			int var747=747;
			int var748=748;
			int var749=749;
			int var750=750;
			int var751=751;
			int var752=752;
			int var753=753;
			int var754=754;
			int var755=755;
			int var756=756;
			int var757=757;
			int var758=758;
			int var759=759;
			int var760=760;
			int var761=761;
			int var762=762;
			int var763=763;
			int var764=764;
			int var765=765;
			int var766=766;
			int var767=767;
			int var768=768;
			int var769=769;
			int var770=770;
			int var771=771;
			int var772=772;
			int var773=773;
			int var774=774;
			int var775=775;
			int var776=776;
			int var777=777;
			int var778=778;
			int var779=779;
			int var780=780;
			int var781=781;
			int var782=782;
			int var783=783;
			int var784=784;
			int var785=785;
			int var786=786;
			int var787=787;
			int var788=788;
			int var789=789;
			int var790=790;
			int var791=791;
			int var792=792;
			int var793=793;
			int var794=794;
			int var795=795;
			int var796=796;
			int var797=797;
			int var798=798;
			int var799=799;
			int var800=800;
			int var801=801;
			int var802=802;
			int var803=803;
			int var804=804;
			int var805=805;
			int var806=806;
			int var807=807;
			int var808=808;
			int var809=809;
			int var810=810;
			int var811=811;
			int var812=812;
			int var813=813;
			int var814=814;
			int var815=815;
			int var816=816;
			int var817=817;
			int var818=818;
			int var819=819;
			int var820=820;
			int var821=821;
			int var822=822;
			int var823=823;
			int var824=824;
			int var825=825;
			int var826=826;
			int var827=827;
			int var828=828;
			int var829=829;
			int var830=830;
			int var831=831;
			int var832=832;
			int var833=833;
			int var834=834;
			int var835=835;
			int var836=836;
			int var837=837;
			int var838=838;
			int var839=839;
			int var840=840;
			int var841=841;
			int var842=842;
			int var843=843;
			int var844=844;
			int var845=845;
			int var846=846;
			int var847=847;
			int var848=848;
			int var849=849;
			int var850=850;
			int var851=851;
			int var852=852;
			int var853=853;
			int var854=854;
			int var855=855;
			int var856=856;
			int var857=857;
			int var858=858;
			int var859=859;
			int var860=860;
			int var861=861;
			int var862=862;
			int var863=863;
			int var864=864;
			int var865=865;
			int var866=866;
			int var867=867;
			int var868=868;
			int var869=869;
			int var870=870;
			int var871=871;
			int var872=872;
			int var873=873;
			int var874=874;
			int var875=875;
			int var876=876;
			int var877=877;
			int var878=878;
			int var879=879;
			int var880=880;
			int var881=881;
			int var882=882;
			int var883=883;
			int var884=884;
			int var885=885;
			int var886=886;
			int var887=887;
			int var888=888;
			int var889=889;
			int var890=890;
			int var891=891;
			int var892=892;
			int var893=893;
			int var894=894;
			int var895=895;
			int var896=896;
			int var897=897;
			int var898=898;
			int var899=899;
			int var900=900;
			int var901=901;
			int var902=902;
			int var903=903;
			int var904=904;
			int var905=905;
			int var906=906;
			int var907=907;
			int var908=908;
			int var909=909;
			int var910=910;
			int var911=911;
			int var912=912;
			int var913=913;
			int var914=914;
			int var915=915;
			int var916=916;
			int var917=917;
			int var918=918;
			int var919=919;
			int var920=920;
			int var921=921;
			int var922=922;
			int var923=923;
			int var924=924;
			int var925=925;
			int var926=926;
			int var927=927;
			int var928=928;
			int var929=929;
			int var930=930;
			int var931=931;
			int var932=932;
			int var933=933;
			int var934=934;
			int var935=935;
			int var936=936;
			int var937=937;
			int var938=938;
			int var939=939;
			int var940=940;
			int var941=941;
			int var942=942;
			int var943=943;
			int var944=944;
			int var945=945;
			int var946=946;
			int var947=947;
			int var948=948;
			int var949=949;
			int var950=950;
			int var951=951;
			int var952=952;
			int var953=953;
			int var954=954;
			int var955=955;
			int var956=956;
			int var957=957;
			int var958=958;
			int var959=959;
			int var960=960;
			int var961=961;
			int var962=962;
			int var963=963;
			int var964=964;
			int var965=965;
			int var966=966;
			int var967=967;
			int var968=968;
			int var969=969;
			int var970=970;
			int var971=971;
			int var972=972;
			int var973=973;
			int var974=974;
			int var975=975;
			int var976=976;
			int var977=977;
			int var978=978;
			int var979=979;
			int var980=980;
			int var981=981;
			int var982=982;
			int var983=983;
			int var984=984;
			int var985=985;
			int var986=986;
			int var987=987;
			int var988=988;
			int var989=989;
			int var990=990;
			int var991=991;
			int var992=992;
			int var993=993;
			int var994=994;
			int var995=995;
			int var996=996;
			int var997=997;
			int var998=998;
			int var999=999;
			#endregion
			#region ObjektAllokation
			mthdcls0  mc0 = new mthdcls0();
			mthdcls1  mc1 = new mthdcls1();
			mthdcls2  mc2 = new mthdcls2();
			mthdcls3  mc3 = new mthdcls3();
			mthdcls4  mc4 = new mthdcls4();
			mthdcls5  mc5 = new mthdcls5();
			mthdcls6  mc6 = new mthdcls6();
			mthdcls7  mc7 = new mthdcls7();
			mthdcls8  mc8 = new mthdcls8();
			mthdcls9  mc9 = new mthdcls9();
			mthdcls10  mc10 = new mthdcls10();
			mthdcls11  mc11 = new mthdcls11();
			mthdcls12  mc12 = new mthdcls12();
			mthdcls13  mc13 = new mthdcls13();
			mthdcls14  mc14 = new mthdcls14();
			mthdcls15  mc15 = new mthdcls15();
			mthdcls16  mc16 = new mthdcls16();
			mthdcls17  mc17 = new mthdcls17();
			mthdcls18  mc18 = new mthdcls18();
			mthdcls19  mc19 = new mthdcls19();
			mthdcls20  mc20 = new mthdcls20();
			mthdcls21  mc21 = new mthdcls21();
			mthdcls22  mc22 = new mthdcls22();
			mthdcls23  mc23 = new mthdcls23();
			mthdcls24  mc24 = new mthdcls24();
			mthdcls25  mc25 = new mthdcls25();
			mthdcls26  mc26 = new mthdcls26();
			mthdcls27  mc27 = new mthdcls27();
			mthdcls28  mc28 = new mthdcls28();
			mthdcls29  mc29 = new mthdcls29();
			mthdcls30  mc30 = new mthdcls30();
			mthdcls31  mc31 = new mthdcls31();
			mthdcls32  mc32 = new mthdcls32();
			mthdcls33  mc33 = new mthdcls33();
			mthdcls34  mc34 = new mthdcls34();
			mthdcls35  mc35 = new mthdcls35();
			mthdcls36  mc36 = new mthdcls36();
			mthdcls37  mc37 = new mthdcls37();
			mthdcls38  mc38 = new mthdcls38();
			mthdcls39  mc39 = new mthdcls39();
			mthdcls40  mc40 = new mthdcls40();
			mthdcls41  mc41 = new mthdcls41();
			mthdcls42  mc42 = new mthdcls42();
			mthdcls43  mc43 = new mthdcls43();
			mthdcls44  mc44 = new mthdcls44();
			mthdcls45  mc45 = new mthdcls45();
			mthdcls46  mc46 = new mthdcls46();
			mthdcls47  mc47 = new mthdcls47();
			mthdcls48  mc48 = new mthdcls48();
			mthdcls49  mc49 = new mthdcls49();
			mthdcls50  mc50 = new mthdcls50();
			mthdcls51  mc51 = new mthdcls51();
			mthdcls52  mc52 = new mthdcls52();
			mthdcls53  mc53 = new mthdcls53();
			mthdcls54  mc54 = new mthdcls54();
			mthdcls55  mc55 = new mthdcls55();
			mthdcls56  mc56 = new mthdcls56();
			mthdcls57  mc57 = new mthdcls57();
			mthdcls58  mc58 = new mthdcls58();
			mthdcls59  mc59 = new mthdcls59();
			mthdcls60  mc60 = new mthdcls60();
			mthdcls61  mc61 = new mthdcls61();
			mthdcls62  mc62 = new mthdcls62();
			mthdcls63  mc63 = new mthdcls63();
			mthdcls64  mc64 = new mthdcls64();
			mthdcls65  mc65 = new mthdcls65();
			mthdcls66  mc66 = new mthdcls66();
			mthdcls67  mc67 = new mthdcls67();
			mthdcls68  mc68 = new mthdcls68();
			mthdcls69  mc69 = new mthdcls69();
			mthdcls70  mc70 = new mthdcls70();
			mthdcls71  mc71 = new mthdcls71();
			mthdcls72  mc72 = new mthdcls72();
			mthdcls73  mc73 = new mthdcls73();
			mthdcls74  mc74 = new mthdcls74();
			mthdcls75  mc75 = new mthdcls75();
			mthdcls76  mc76 = new mthdcls76();
			mthdcls77  mc77 = new mthdcls77();
			mthdcls78  mc78 = new mthdcls78();
			mthdcls79  mc79 = new mthdcls79();
			mthdcls80  mc80 = new mthdcls80();
			mthdcls81  mc81 = new mthdcls81();
			mthdcls82  mc82 = new mthdcls82();
			mthdcls83  mc83 = new mthdcls83();
			mthdcls84  mc84 = new mthdcls84();
			mthdcls85  mc85 = new mthdcls85();
			mthdcls86  mc86 = new mthdcls86();
			mthdcls87  mc87 = new mthdcls87();
			mthdcls88  mc88 = new mthdcls88();
			mthdcls89  mc89 = new mthdcls89();
			mthdcls90  mc90 = new mthdcls90();
			mthdcls91  mc91 = new mthdcls91();
			mthdcls92  mc92 = new mthdcls92();
			mthdcls93  mc93 = new mthdcls93();
			mthdcls94  mc94 = new mthdcls94();
			mthdcls95  mc95 = new mthdcls95();
			mthdcls96  mc96 = new mthdcls96();
			mthdcls97  mc97 = new mthdcls97();
			mthdcls98  mc98 = new mthdcls98();
			mthdcls99  mc99 = new mthdcls99();
			mthdcls100  mc100 = new mthdcls100();
			mthdcls101  mc101 = new mthdcls101();
			mthdcls102  mc102 = new mthdcls102();
			mthdcls103  mc103 = new mthdcls103();
			mthdcls104  mc104 = new mthdcls104();
			mthdcls105  mc105 = new mthdcls105();
			mthdcls106  mc106 = new mthdcls106();
			mthdcls107  mc107 = new mthdcls107();
			mthdcls108  mc108 = new mthdcls108();
			mthdcls109  mc109 = new mthdcls109();
			mthdcls110  mc110 = new mthdcls110();
			mthdcls111  mc111 = new mthdcls111();
			mthdcls112  mc112 = new mthdcls112();
			mthdcls113  mc113 = new mthdcls113();
			mthdcls114  mc114 = new mthdcls114();
			mthdcls115  mc115 = new mthdcls115();
			mthdcls116  mc116 = new mthdcls116();
			mthdcls117  mc117 = new mthdcls117();
			mthdcls118  mc118 = new mthdcls118();
			mthdcls119  mc119 = new mthdcls119();
			mthdcls120  mc120 = new mthdcls120();
			mthdcls121  mc121 = new mthdcls121();
			mthdcls122  mc122 = new mthdcls122();
			mthdcls123  mc123 = new mthdcls123();
			mthdcls124  mc124 = new mthdcls124();
			mthdcls125  mc125 = new mthdcls125();
			mthdcls126  mc126 = new mthdcls126();
			mthdcls127  mc127 = new mthdcls127();
			mthdcls128  mc128 = new mthdcls128();
			mthdcls129  mc129 = new mthdcls129();
			mthdcls130  mc130 = new mthdcls130();
			mthdcls131  mc131 = new mthdcls131();
			mthdcls132  mc132 = new mthdcls132();
			mthdcls133  mc133 = new mthdcls133();
			mthdcls134  mc134 = new mthdcls134();
			mthdcls135  mc135 = new mthdcls135();
			mthdcls136  mc136 = new mthdcls136();
			mthdcls137  mc137 = new mthdcls137();
			mthdcls138  mc138 = new mthdcls138();
			mthdcls139  mc139 = new mthdcls139();
			mthdcls140  mc140 = new mthdcls140();
			mthdcls141  mc141 = new mthdcls141();
			mthdcls142  mc142 = new mthdcls142();
			mthdcls143  mc143 = new mthdcls143();
			mthdcls144  mc144 = new mthdcls144();
			mthdcls145  mc145 = new mthdcls145();
			mthdcls146  mc146 = new mthdcls146();
			mthdcls147  mc147 = new mthdcls147();
			mthdcls148  mc148 = new mthdcls148();
			mthdcls149  mc149 = new mthdcls149();
			mthdcls150  mc150 = new mthdcls150();
			mthdcls151  mc151 = new mthdcls151();
			mthdcls152  mc152 = new mthdcls152();
			mthdcls153  mc153 = new mthdcls153();
			mthdcls154  mc154 = new mthdcls154();
			mthdcls155  mc155 = new mthdcls155();
			mthdcls156  mc156 = new mthdcls156();
			mthdcls157  mc157 = new mthdcls157();
			mthdcls158  mc158 = new mthdcls158();
			mthdcls159  mc159 = new mthdcls159();
			mthdcls160  mc160 = new mthdcls160();
			mthdcls161  mc161 = new mthdcls161();
			mthdcls162  mc162 = new mthdcls162();
			mthdcls163  mc163 = new mthdcls163();
			mthdcls164  mc164 = new mthdcls164();
			mthdcls165  mc165 = new mthdcls165();
			mthdcls166  mc166 = new mthdcls166();
			mthdcls167  mc167 = new mthdcls167();
			mthdcls168  mc168 = new mthdcls168();
			mthdcls169  mc169 = new mthdcls169();
			mthdcls170  mc170 = new mthdcls170();
			mthdcls171  mc171 = new mthdcls171();
			mthdcls172  mc172 = new mthdcls172();
			mthdcls173  mc173 = new mthdcls173();
			mthdcls174  mc174 = new mthdcls174();
			mthdcls175  mc175 = new mthdcls175();
			mthdcls176  mc176 = new mthdcls176();
			mthdcls177  mc177 = new mthdcls177();
			mthdcls178  mc178 = new mthdcls178();
			mthdcls179  mc179 = new mthdcls179();
			mthdcls180  mc180 = new mthdcls180();
			mthdcls181  mc181 = new mthdcls181();
			mthdcls182  mc182 = new mthdcls182();
			mthdcls183  mc183 = new mthdcls183();
			mthdcls184  mc184 = new mthdcls184();
			mthdcls185  mc185 = new mthdcls185();
			mthdcls186  mc186 = new mthdcls186();
			mthdcls187  mc187 = new mthdcls187();
			mthdcls188  mc188 = new mthdcls188();
			mthdcls189  mc189 = new mthdcls189();
			mthdcls190  mc190 = new mthdcls190();
			mthdcls191  mc191 = new mthdcls191();
			mthdcls192  mc192 = new mthdcls192();
			mthdcls193  mc193 = new mthdcls193();
			mthdcls194  mc194 = new mthdcls194();
			mthdcls195  mc195 = new mthdcls195();
			mthdcls196  mc196 = new mthdcls196();
			mthdcls197  mc197 = new mthdcls197();
			mthdcls198  mc198 = new mthdcls198();
			mthdcls199  mc199 = new mthdcls199();
			mthdcls200  mc200 = new mthdcls200();
			mthdcls201  mc201 = new mthdcls201();
			mthdcls202  mc202 = new mthdcls202();
			mthdcls203  mc203 = new mthdcls203();
			mthdcls204  mc204 = new mthdcls204();
			mthdcls205  mc205 = new mthdcls205();
			mthdcls206  mc206 = new mthdcls206();
			mthdcls207  mc207 = new mthdcls207();
			mthdcls208  mc208 = new mthdcls208();
			mthdcls209  mc209 = new mthdcls209();
			mthdcls210  mc210 = new mthdcls210();
			mthdcls211  mc211 = new mthdcls211();
			mthdcls212  mc212 = new mthdcls212();
			mthdcls213  mc213 = new mthdcls213();
			mthdcls214  mc214 = new mthdcls214();
			mthdcls215  mc215 = new mthdcls215();
			mthdcls216  mc216 = new mthdcls216();
			mthdcls217  mc217 = new mthdcls217();
			mthdcls218  mc218 = new mthdcls218();
			mthdcls219  mc219 = new mthdcls219();
			mthdcls220  mc220 = new mthdcls220();
			mthdcls221  mc221 = new mthdcls221();
			mthdcls222  mc222 = new mthdcls222();
			mthdcls223  mc223 = new mthdcls223();
			mthdcls224  mc224 = new mthdcls224();
			mthdcls225  mc225 = new mthdcls225();
			mthdcls226  mc226 = new mthdcls226();
			mthdcls227  mc227 = new mthdcls227();
			mthdcls228  mc228 = new mthdcls228();
			mthdcls229  mc229 = new mthdcls229();
			mthdcls230  mc230 = new mthdcls230();
			mthdcls231  mc231 = new mthdcls231();
			mthdcls232  mc232 = new mthdcls232();
			mthdcls233  mc233 = new mthdcls233();
			mthdcls234  mc234 = new mthdcls234();
			mthdcls235  mc235 = new mthdcls235();
			mthdcls236  mc236 = new mthdcls236();
			mthdcls237  mc237 = new mthdcls237();
			mthdcls238  mc238 = new mthdcls238();
			mthdcls239  mc239 = new mthdcls239();
			mthdcls240  mc240 = new mthdcls240();
			mthdcls241  mc241 = new mthdcls241();
			mthdcls242  mc242 = new mthdcls242();
			mthdcls243  mc243 = new mthdcls243();
			mthdcls244  mc244 = new mthdcls244();
			mthdcls245  mc245 = new mthdcls245();
			mthdcls246  mc246 = new mthdcls246();
			mthdcls247  mc247 = new mthdcls247();
			mthdcls248  mc248 = new mthdcls248();
			mthdcls249  mc249 = new mthdcls249();
			mthdcls250  mc250 = new mthdcls250();
			mthdcls251  mc251 = new mthdcls251();
			mthdcls252  mc252 = new mthdcls252();
			mthdcls253  mc253 = new mthdcls253();
			mthdcls254  mc254 = new mthdcls254();
			mthdcls255  mc255 = new mthdcls255();
			mthdcls256  mc256 = new mthdcls256();
			mthdcls257  mc257 = new mthdcls257();
			mthdcls258  mc258 = new mthdcls258();
			mthdcls259  mc259 = new mthdcls259();
			mthdcls260  mc260 = new mthdcls260();
			mthdcls261  mc261 = new mthdcls261();
			mthdcls262  mc262 = new mthdcls262();
			mthdcls263  mc263 = new mthdcls263();
			mthdcls264  mc264 = new mthdcls264();
			mthdcls265  mc265 = new mthdcls265();
			mthdcls266  mc266 = new mthdcls266();
			mthdcls267  mc267 = new mthdcls267();
			mthdcls268  mc268 = new mthdcls268();
			mthdcls269  mc269 = new mthdcls269();
			mthdcls270  mc270 = new mthdcls270();
			mthdcls271  mc271 = new mthdcls271();
			mthdcls272  mc272 = new mthdcls272();
			mthdcls273  mc273 = new mthdcls273();
			mthdcls274  mc274 = new mthdcls274();
			mthdcls275  mc275 = new mthdcls275();
			mthdcls276  mc276 = new mthdcls276();
			mthdcls277  mc277 = new mthdcls277();
			mthdcls278  mc278 = new mthdcls278();
			mthdcls279  mc279 = new mthdcls279();
			mthdcls280  mc280 = new mthdcls280();
			mthdcls281  mc281 = new mthdcls281();
			mthdcls282  mc282 = new mthdcls282();
			mthdcls283  mc283 = new mthdcls283();
			mthdcls284  mc284 = new mthdcls284();
			mthdcls285  mc285 = new mthdcls285();
			mthdcls286  mc286 = new mthdcls286();
			mthdcls287  mc287 = new mthdcls287();
			mthdcls288  mc288 = new mthdcls288();
			mthdcls289  mc289 = new mthdcls289();
			mthdcls290  mc290 = new mthdcls290();
			mthdcls291  mc291 = new mthdcls291();
			mthdcls292  mc292 = new mthdcls292();
			mthdcls293  mc293 = new mthdcls293();
			mthdcls294  mc294 = new mthdcls294();
			mthdcls295  mc295 = new mthdcls295();
			mthdcls296  mc296 = new mthdcls296();
			mthdcls297  mc297 = new mthdcls297();
			mthdcls298  mc298 = new mthdcls298();
			mthdcls299  mc299 = new mthdcls299();
			mthdcls300  mc300 = new mthdcls300();
			mthdcls301  mc301 = new mthdcls301();
			mthdcls302  mc302 = new mthdcls302();
			mthdcls303  mc303 = new mthdcls303();
			mthdcls304  mc304 = new mthdcls304();
			mthdcls305  mc305 = new mthdcls305();
			mthdcls306  mc306 = new mthdcls306();
			mthdcls307  mc307 = new mthdcls307();
			mthdcls308  mc308 = new mthdcls308();
			mthdcls309  mc309 = new mthdcls309();
			mthdcls310  mc310 = new mthdcls310();
			mthdcls311  mc311 = new mthdcls311();
			mthdcls312  mc312 = new mthdcls312();
			mthdcls313  mc313 = new mthdcls313();
			mthdcls314  mc314 = new mthdcls314();
			mthdcls315  mc315 = new mthdcls315();
			mthdcls316  mc316 = new mthdcls316();
			mthdcls317  mc317 = new mthdcls317();
			mthdcls318  mc318 = new mthdcls318();
			mthdcls319  mc319 = new mthdcls319();
			mthdcls320  mc320 = new mthdcls320();
			mthdcls321  mc321 = new mthdcls321();
			mthdcls322  mc322 = new mthdcls322();
			mthdcls323  mc323 = new mthdcls323();
			mthdcls324  mc324 = new mthdcls324();
			mthdcls325  mc325 = new mthdcls325();
			mthdcls326  mc326 = new mthdcls326();
			mthdcls327  mc327 = new mthdcls327();
			mthdcls328  mc328 = new mthdcls328();
			mthdcls329  mc329 = new mthdcls329();
			mthdcls330  mc330 = new mthdcls330();
			mthdcls331  mc331 = new mthdcls331();
			mthdcls332  mc332 = new mthdcls332();
			mthdcls333  mc333 = new mthdcls333();
			mthdcls334  mc334 = new mthdcls334();
			mthdcls335  mc335 = new mthdcls335();
			mthdcls336  mc336 = new mthdcls336();
			mthdcls337  mc337 = new mthdcls337();
			mthdcls338  mc338 = new mthdcls338();
			mthdcls339  mc339 = new mthdcls339();
			mthdcls340  mc340 = new mthdcls340();
			mthdcls341  mc341 = new mthdcls341();
			mthdcls342  mc342 = new mthdcls342();
			mthdcls343  mc343 = new mthdcls343();
			mthdcls344  mc344 = new mthdcls344();
			mthdcls345  mc345 = new mthdcls345();
			mthdcls346  mc346 = new mthdcls346();
			mthdcls347  mc347 = new mthdcls347();
			mthdcls348  mc348 = new mthdcls348();
			mthdcls349  mc349 = new mthdcls349();
			mthdcls350  mc350 = new mthdcls350();
			mthdcls351  mc351 = new mthdcls351();
			mthdcls352  mc352 = new mthdcls352();
			mthdcls353  mc353 = new mthdcls353();
			mthdcls354  mc354 = new mthdcls354();
			mthdcls355  mc355 = new mthdcls355();
			mthdcls356  mc356 = new mthdcls356();
			mthdcls357  mc357 = new mthdcls357();
			mthdcls358  mc358 = new mthdcls358();
			mthdcls359  mc359 = new mthdcls359();
			mthdcls360  mc360 = new mthdcls360();
			mthdcls361  mc361 = new mthdcls361();
			mthdcls362  mc362 = new mthdcls362();
			mthdcls363  mc363 = new mthdcls363();
			mthdcls364  mc364 = new mthdcls364();
			mthdcls365  mc365 = new mthdcls365();
			mthdcls366  mc366 = new mthdcls366();
			mthdcls367  mc367 = new mthdcls367();
			mthdcls368  mc368 = new mthdcls368();
			mthdcls369  mc369 = new mthdcls369();
			mthdcls370  mc370 = new mthdcls370();
			mthdcls371  mc371 = new mthdcls371();
			mthdcls372  mc372 = new mthdcls372();
			mthdcls373  mc373 = new mthdcls373();
			mthdcls374  mc374 = new mthdcls374();
			mthdcls375  mc375 = new mthdcls375();
			mthdcls376  mc376 = new mthdcls376();
			mthdcls377  mc377 = new mthdcls377();
			mthdcls378  mc378 = new mthdcls378();
			mthdcls379  mc379 = new mthdcls379();
			mthdcls380  mc380 = new mthdcls380();
			mthdcls381  mc381 = new mthdcls381();
			mthdcls382  mc382 = new mthdcls382();
			mthdcls383  mc383 = new mthdcls383();
			mthdcls384  mc384 = new mthdcls384();
			mthdcls385  mc385 = new mthdcls385();
			mthdcls386  mc386 = new mthdcls386();
			mthdcls387  mc387 = new mthdcls387();
			mthdcls388  mc388 = new mthdcls388();
			mthdcls389  mc389 = new mthdcls389();
			mthdcls390  mc390 = new mthdcls390();
			mthdcls391  mc391 = new mthdcls391();
			mthdcls392  mc392 = new mthdcls392();
			mthdcls393  mc393 = new mthdcls393();
			mthdcls394  mc394 = new mthdcls394();
			mthdcls395  mc395 = new mthdcls395();
			mthdcls396  mc396 = new mthdcls396();
			mthdcls397  mc397 = new mthdcls397();
			mthdcls398  mc398 = new mthdcls398();
			mthdcls399  mc399 = new mthdcls399();
			mthdcls400  mc400 = new mthdcls400();
			mthdcls401  mc401 = new mthdcls401();
			mthdcls402  mc402 = new mthdcls402();
			mthdcls403  mc403 = new mthdcls403();
			mthdcls404  mc404 = new mthdcls404();
			mthdcls405  mc405 = new mthdcls405();
			mthdcls406  mc406 = new mthdcls406();
			mthdcls407  mc407 = new mthdcls407();
			mthdcls408  mc408 = new mthdcls408();
			mthdcls409  mc409 = new mthdcls409();
			mthdcls410  mc410 = new mthdcls410();
			mthdcls411  mc411 = new mthdcls411();
			mthdcls412  mc412 = new mthdcls412();
			mthdcls413  mc413 = new mthdcls413();
			mthdcls414  mc414 = new mthdcls414();
			mthdcls415  mc415 = new mthdcls415();
			mthdcls416  mc416 = new mthdcls416();
			mthdcls417  mc417 = new mthdcls417();
			mthdcls418  mc418 = new mthdcls418();
			mthdcls419  mc419 = new mthdcls419();
			mthdcls420  mc420 = new mthdcls420();
			mthdcls421  mc421 = new mthdcls421();
			mthdcls422  mc422 = new mthdcls422();
			mthdcls423  mc423 = new mthdcls423();
			mthdcls424  mc424 = new mthdcls424();
			mthdcls425  mc425 = new mthdcls425();
			mthdcls426  mc426 = new mthdcls426();
			mthdcls427  mc427 = new mthdcls427();
			mthdcls428  mc428 = new mthdcls428();
			mthdcls429  mc429 = new mthdcls429();
			mthdcls430  mc430 = new mthdcls430();
			mthdcls431  mc431 = new mthdcls431();
			mthdcls432  mc432 = new mthdcls432();
			mthdcls433  mc433 = new mthdcls433();
			mthdcls434  mc434 = new mthdcls434();
			mthdcls435  mc435 = new mthdcls435();
			mthdcls436  mc436 = new mthdcls436();
			mthdcls437  mc437 = new mthdcls437();
			mthdcls438  mc438 = new mthdcls438();
			mthdcls439  mc439 = new mthdcls439();
			mthdcls440  mc440 = new mthdcls440();
			mthdcls441  mc441 = new mthdcls441();
			mthdcls442  mc442 = new mthdcls442();
			mthdcls443  mc443 = new mthdcls443();
			mthdcls444  mc444 = new mthdcls444();
			mthdcls445  mc445 = new mthdcls445();
			mthdcls446  mc446 = new mthdcls446();
			mthdcls447  mc447 = new mthdcls447();
			mthdcls448  mc448 = new mthdcls448();
			mthdcls449  mc449 = new mthdcls449();
			mthdcls450  mc450 = new mthdcls450();
			mthdcls451  mc451 = new mthdcls451();
			mthdcls452  mc452 = new mthdcls452();
			mthdcls453  mc453 = new mthdcls453();
			mthdcls454  mc454 = new mthdcls454();
			mthdcls455  mc455 = new mthdcls455();
			mthdcls456  mc456 = new mthdcls456();
			mthdcls457  mc457 = new mthdcls457();
			mthdcls458  mc458 = new mthdcls458();
			mthdcls459  mc459 = new mthdcls459();
			mthdcls460  mc460 = new mthdcls460();
			mthdcls461  mc461 = new mthdcls461();
			mthdcls462  mc462 = new mthdcls462();
			mthdcls463  mc463 = new mthdcls463();
			mthdcls464  mc464 = new mthdcls464();
			mthdcls465  mc465 = new mthdcls465();
			mthdcls466  mc466 = new mthdcls466();
			mthdcls467  mc467 = new mthdcls467();
			mthdcls468  mc468 = new mthdcls468();
			mthdcls469  mc469 = new mthdcls469();
			mthdcls470  mc470 = new mthdcls470();
			mthdcls471  mc471 = new mthdcls471();
			mthdcls472  mc472 = new mthdcls472();
			mthdcls473  mc473 = new mthdcls473();
			mthdcls474  mc474 = new mthdcls474();
			mthdcls475  mc475 = new mthdcls475();
			mthdcls476  mc476 = new mthdcls476();
			mthdcls477  mc477 = new mthdcls477();
			mthdcls478  mc478 = new mthdcls478();
			mthdcls479  mc479 = new mthdcls479();
			mthdcls480  mc480 = new mthdcls480();
			mthdcls481  mc481 = new mthdcls481();
			mthdcls482  mc482 = new mthdcls482();
			mthdcls483  mc483 = new mthdcls483();
			mthdcls484  mc484 = new mthdcls484();
			mthdcls485  mc485 = new mthdcls485();
			mthdcls486  mc486 = new mthdcls486();
			mthdcls487  mc487 = new mthdcls487();
			mthdcls488  mc488 = new mthdcls488();
			mthdcls489  mc489 = new mthdcls489();
			mthdcls490  mc490 = new mthdcls490();
			mthdcls491  mc491 = new mthdcls491();
			mthdcls492  mc492 = new mthdcls492();
			mthdcls493  mc493 = new mthdcls493();
			mthdcls494  mc494 = new mthdcls494();
			mthdcls495  mc495 = new mthdcls495();
			mthdcls496  mc496 = new mthdcls496();
			mthdcls497  mc497 = new mthdcls497();
			mthdcls498  mc498 = new mthdcls498();
			mthdcls499  mc499 = new mthdcls499();
			mthdcls500  mc500 = new mthdcls500();
			mthdcls501  mc501 = new mthdcls501();
			mthdcls502  mc502 = new mthdcls502();
			mthdcls503  mc503 = new mthdcls503();
			mthdcls504  mc504 = new mthdcls504();
			mthdcls505  mc505 = new mthdcls505();
			mthdcls506  mc506 = new mthdcls506();
			mthdcls507  mc507 = new mthdcls507();
			mthdcls508  mc508 = new mthdcls508();
			mthdcls509  mc509 = new mthdcls509();
			mthdcls510  mc510 = new mthdcls510();
			mthdcls511  mc511 = new mthdcls511();
			mthdcls512  mc512 = new mthdcls512();
			mthdcls513  mc513 = new mthdcls513();
			mthdcls514  mc514 = new mthdcls514();
			mthdcls515  mc515 = new mthdcls515();
			mthdcls516  mc516 = new mthdcls516();
			mthdcls517  mc517 = new mthdcls517();
			mthdcls518  mc518 = new mthdcls518();
			mthdcls519  mc519 = new mthdcls519();
			mthdcls520  mc520 = new mthdcls520();
			mthdcls521  mc521 = new mthdcls521();
			mthdcls522  mc522 = new mthdcls522();
			mthdcls523  mc523 = new mthdcls523();
			mthdcls524  mc524 = new mthdcls524();
			mthdcls525  mc525 = new mthdcls525();
			mthdcls526  mc526 = new mthdcls526();
			mthdcls527  mc527 = new mthdcls527();
			mthdcls528  mc528 = new mthdcls528();
			mthdcls529  mc529 = new mthdcls529();
			mthdcls530  mc530 = new mthdcls530();
			mthdcls531  mc531 = new mthdcls531();
			mthdcls532  mc532 = new mthdcls532();
			mthdcls533  mc533 = new mthdcls533();
			mthdcls534  mc534 = new mthdcls534();
			mthdcls535  mc535 = new mthdcls535();
			mthdcls536  mc536 = new mthdcls536();
			mthdcls537  mc537 = new mthdcls537();
			mthdcls538  mc538 = new mthdcls538();
			mthdcls539  mc539 = new mthdcls539();
			mthdcls540  mc540 = new mthdcls540();
			mthdcls541  mc541 = new mthdcls541();
			mthdcls542  mc542 = new mthdcls542();
			mthdcls543  mc543 = new mthdcls543();
			mthdcls544  mc544 = new mthdcls544();
			mthdcls545  mc545 = new mthdcls545();
			mthdcls546  mc546 = new mthdcls546();
			mthdcls547  mc547 = new mthdcls547();
			mthdcls548  mc548 = new mthdcls548();
			mthdcls549  mc549 = new mthdcls549();
			mthdcls550  mc550 = new mthdcls550();
			mthdcls551  mc551 = new mthdcls551();
			mthdcls552  mc552 = new mthdcls552();
			mthdcls553  mc553 = new mthdcls553();
			mthdcls554  mc554 = new mthdcls554();
			mthdcls555  mc555 = new mthdcls555();
			mthdcls556  mc556 = new mthdcls556();
			mthdcls557  mc557 = new mthdcls557();
			mthdcls558  mc558 = new mthdcls558();
			mthdcls559  mc559 = new mthdcls559();
			mthdcls560  mc560 = new mthdcls560();
			mthdcls561  mc561 = new mthdcls561();
			mthdcls562  mc562 = new mthdcls562();
			mthdcls563  mc563 = new mthdcls563();
			mthdcls564  mc564 = new mthdcls564();
			mthdcls565  mc565 = new mthdcls565();
			mthdcls566  mc566 = new mthdcls566();
			mthdcls567  mc567 = new mthdcls567();
			mthdcls568  mc568 = new mthdcls568();
			mthdcls569  mc569 = new mthdcls569();
			mthdcls570  mc570 = new mthdcls570();
			mthdcls571  mc571 = new mthdcls571();
			mthdcls572  mc572 = new mthdcls572();
			mthdcls573  mc573 = new mthdcls573();
			mthdcls574  mc574 = new mthdcls574();
			mthdcls575  mc575 = new mthdcls575();
			mthdcls576  mc576 = new mthdcls576();
			mthdcls577  mc577 = new mthdcls577();
			mthdcls578  mc578 = new mthdcls578();
			mthdcls579  mc579 = new mthdcls579();
			mthdcls580  mc580 = new mthdcls580();
			mthdcls581  mc581 = new mthdcls581();
			mthdcls582  mc582 = new mthdcls582();
			mthdcls583  mc583 = new mthdcls583();
			mthdcls584  mc584 = new mthdcls584();
			mthdcls585  mc585 = new mthdcls585();
			mthdcls586  mc586 = new mthdcls586();
			mthdcls587  mc587 = new mthdcls587();
			mthdcls588  mc588 = new mthdcls588();
			mthdcls589  mc589 = new mthdcls589();
			mthdcls590  mc590 = new mthdcls590();
			mthdcls591  mc591 = new mthdcls591();
			mthdcls592  mc592 = new mthdcls592();
			mthdcls593  mc593 = new mthdcls593();
			mthdcls594  mc594 = new mthdcls594();
			mthdcls595  mc595 = new mthdcls595();
			mthdcls596  mc596 = new mthdcls596();
			mthdcls597  mc597 = new mthdcls597();
			mthdcls598  mc598 = new mthdcls598();
			mthdcls599  mc599 = new mthdcls599();
			mthdcls600  mc600 = new mthdcls600();
			mthdcls601  mc601 = new mthdcls601();
			mthdcls602  mc602 = new mthdcls602();
			mthdcls603  mc603 = new mthdcls603();
			mthdcls604  mc604 = new mthdcls604();
			mthdcls605  mc605 = new mthdcls605();
			mthdcls606  mc606 = new mthdcls606();
			mthdcls607  mc607 = new mthdcls607();
			mthdcls608  mc608 = new mthdcls608();
			mthdcls609  mc609 = new mthdcls609();
			mthdcls610  mc610 = new mthdcls610();
			mthdcls611  mc611 = new mthdcls611();
			mthdcls612  mc612 = new mthdcls612();
			mthdcls613  mc613 = new mthdcls613();
			mthdcls614  mc614 = new mthdcls614();
			mthdcls615  mc615 = new mthdcls615();
			mthdcls616  mc616 = new mthdcls616();
			mthdcls617  mc617 = new mthdcls617();
			mthdcls618  mc618 = new mthdcls618();
			mthdcls619  mc619 = new mthdcls619();
			mthdcls620  mc620 = new mthdcls620();
			mthdcls621  mc621 = new mthdcls621();
			mthdcls622  mc622 = new mthdcls622();
			mthdcls623  mc623 = new mthdcls623();
			mthdcls624  mc624 = new mthdcls624();
			mthdcls625  mc625 = new mthdcls625();
			mthdcls626  mc626 = new mthdcls626();
			mthdcls627  mc627 = new mthdcls627();
			mthdcls628  mc628 = new mthdcls628();
			mthdcls629  mc629 = new mthdcls629();
			mthdcls630  mc630 = new mthdcls630();
			mthdcls631  mc631 = new mthdcls631();
			mthdcls632  mc632 = new mthdcls632();
			mthdcls633  mc633 = new mthdcls633();
			mthdcls634  mc634 = new mthdcls634();
			mthdcls635  mc635 = new mthdcls635();
			mthdcls636  mc636 = new mthdcls636();
			mthdcls637  mc637 = new mthdcls637();
			mthdcls638  mc638 = new mthdcls638();
			mthdcls639  mc639 = new mthdcls639();
			mthdcls640  mc640 = new mthdcls640();
			mthdcls641  mc641 = new mthdcls641();
			mthdcls642  mc642 = new mthdcls642();
			mthdcls643  mc643 = new mthdcls643();
			mthdcls644  mc644 = new mthdcls644();
			mthdcls645  mc645 = new mthdcls645();
			mthdcls646  mc646 = new mthdcls646();
			mthdcls647  mc647 = new mthdcls647();
			mthdcls648  mc648 = new mthdcls648();
			mthdcls649  mc649 = new mthdcls649();
			mthdcls650  mc650 = new mthdcls650();
			mthdcls651  mc651 = new mthdcls651();
			mthdcls652  mc652 = new mthdcls652();
			mthdcls653  mc653 = new mthdcls653();
			mthdcls654  mc654 = new mthdcls654();
			mthdcls655  mc655 = new mthdcls655();
			mthdcls656  mc656 = new mthdcls656();
			mthdcls657  mc657 = new mthdcls657();
			mthdcls658  mc658 = new mthdcls658();
			mthdcls659  mc659 = new mthdcls659();
			mthdcls660  mc660 = new mthdcls660();
			mthdcls661  mc661 = new mthdcls661();
			mthdcls662  mc662 = new mthdcls662();
			mthdcls663  mc663 = new mthdcls663();
			mthdcls664  mc664 = new mthdcls664();
			mthdcls665  mc665 = new mthdcls665();
			mthdcls666  mc666 = new mthdcls666();
			mthdcls667  mc667 = new mthdcls667();
			mthdcls668  mc668 = new mthdcls668();
			mthdcls669  mc669 = new mthdcls669();
			mthdcls670  mc670 = new mthdcls670();
			mthdcls671  mc671 = new mthdcls671();
			mthdcls672  mc672 = new mthdcls672();
			mthdcls673  mc673 = new mthdcls673();
			mthdcls674  mc674 = new mthdcls674();
			mthdcls675  mc675 = new mthdcls675();
			mthdcls676  mc676 = new mthdcls676();
			mthdcls677  mc677 = new mthdcls677();
			mthdcls678  mc678 = new mthdcls678();
			mthdcls679  mc679 = new mthdcls679();
			mthdcls680  mc680 = new mthdcls680();
			mthdcls681  mc681 = new mthdcls681();
			mthdcls682  mc682 = new mthdcls682();
			mthdcls683  mc683 = new mthdcls683();
			mthdcls684  mc684 = new mthdcls684();
			mthdcls685  mc685 = new mthdcls685();
			mthdcls686  mc686 = new mthdcls686();
			mthdcls687  mc687 = new mthdcls687();
			mthdcls688  mc688 = new mthdcls688();
			mthdcls689  mc689 = new mthdcls689();
			mthdcls690  mc690 = new mthdcls690();
			mthdcls691  mc691 = new mthdcls691();
			mthdcls692  mc692 = new mthdcls692();
			mthdcls693  mc693 = new mthdcls693();
			mthdcls694  mc694 = new mthdcls694();
			mthdcls695  mc695 = new mthdcls695();
			mthdcls696  mc696 = new mthdcls696();
			mthdcls697  mc697 = new mthdcls697();
			mthdcls698  mc698 = new mthdcls698();
			mthdcls699  mc699 = new mthdcls699();
			mthdcls700  mc700 = new mthdcls700();
			mthdcls701  mc701 = new mthdcls701();
			mthdcls702  mc702 = new mthdcls702();
			mthdcls703  mc703 = new mthdcls703();
			mthdcls704  mc704 = new mthdcls704();
			mthdcls705  mc705 = new mthdcls705();
			mthdcls706  mc706 = new mthdcls706();
			mthdcls707  mc707 = new mthdcls707();
			mthdcls708  mc708 = new mthdcls708();
			mthdcls709  mc709 = new mthdcls709();
			mthdcls710  mc710 = new mthdcls710();
			mthdcls711  mc711 = new mthdcls711();
			mthdcls712  mc712 = new mthdcls712();
			mthdcls713  mc713 = new mthdcls713();
			mthdcls714  mc714 = new mthdcls714();
			mthdcls715  mc715 = new mthdcls715();
			mthdcls716  mc716 = new mthdcls716();
			mthdcls717  mc717 = new mthdcls717();
			mthdcls718  mc718 = new mthdcls718();
			mthdcls719  mc719 = new mthdcls719();
			mthdcls720  mc720 = new mthdcls720();
			mthdcls721  mc721 = new mthdcls721();
			mthdcls722  mc722 = new mthdcls722();
			mthdcls723  mc723 = new mthdcls723();
			mthdcls724  mc724 = new mthdcls724();
			mthdcls725  mc725 = new mthdcls725();
			mthdcls726  mc726 = new mthdcls726();
			mthdcls727  mc727 = new mthdcls727();
			mthdcls728  mc728 = new mthdcls728();
			mthdcls729  mc729 = new mthdcls729();
			mthdcls730  mc730 = new mthdcls730();
			mthdcls731  mc731 = new mthdcls731();
			mthdcls732  mc732 = new mthdcls732();
			mthdcls733  mc733 = new mthdcls733();
			mthdcls734  mc734 = new mthdcls734();
			mthdcls735  mc735 = new mthdcls735();
			mthdcls736  mc736 = new mthdcls736();
			mthdcls737  mc737 = new mthdcls737();
			mthdcls738  mc738 = new mthdcls738();
			mthdcls739  mc739 = new mthdcls739();
			mthdcls740  mc740 = new mthdcls740();
			mthdcls741  mc741 = new mthdcls741();
			mthdcls742  mc742 = new mthdcls742();
			mthdcls743  mc743 = new mthdcls743();
			mthdcls744  mc744 = new mthdcls744();
			mthdcls745  mc745 = new mthdcls745();
			mthdcls746  mc746 = new mthdcls746();
			mthdcls747  mc747 = new mthdcls747();
			mthdcls748  mc748 = new mthdcls748();
			mthdcls749  mc749 = new mthdcls749();
			mthdcls750  mc750 = new mthdcls750();
			mthdcls751  mc751 = new mthdcls751();
			mthdcls752  mc752 = new mthdcls752();
			mthdcls753  mc753 = new mthdcls753();
			mthdcls754  mc754 = new mthdcls754();
			mthdcls755  mc755 = new mthdcls755();
			mthdcls756  mc756 = new mthdcls756();
			mthdcls757  mc757 = new mthdcls757();
			mthdcls758  mc758 = new mthdcls758();
			mthdcls759  mc759 = new mthdcls759();
			mthdcls760  mc760 = new mthdcls760();
			mthdcls761  mc761 = new mthdcls761();
			mthdcls762  mc762 = new mthdcls762();
			mthdcls763  mc763 = new mthdcls763();
			mthdcls764  mc764 = new mthdcls764();
			mthdcls765  mc765 = new mthdcls765();
			mthdcls766  mc766 = new mthdcls766();
			mthdcls767  mc767 = new mthdcls767();
			mthdcls768  mc768 = new mthdcls768();
			mthdcls769  mc769 = new mthdcls769();
			mthdcls770  mc770 = new mthdcls770();
			mthdcls771  mc771 = new mthdcls771();
			mthdcls772  mc772 = new mthdcls772();
			mthdcls773  mc773 = new mthdcls773();
			mthdcls774  mc774 = new mthdcls774();
			mthdcls775  mc775 = new mthdcls775();
			mthdcls776  mc776 = new mthdcls776();
			mthdcls777  mc777 = new mthdcls777();
			mthdcls778  mc778 = new mthdcls778();
			mthdcls779  mc779 = new mthdcls779();
			mthdcls780  mc780 = new mthdcls780();
			mthdcls781  mc781 = new mthdcls781();
			mthdcls782  mc782 = new mthdcls782();
			mthdcls783  mc783 = new mthdcls783();
			mthdcls784  mc784 = new mthdcls784();
			mthdcls785  mc785 = new mthdcls785();
			mthdcls786  mc786 = new mthdcls786();
			mthdcls787  mc787 = new mthdcls787();
			mthdcls788  mc788 = new mthdcls788();
			mthdcls789  mc789 = new mthdcls789();
			mthdcls790  mc790 = new mthdcls790();
			mthdcls791  mc791 = new mthdcls791();
			mthdcls792  mc792 = new mthdcls792();
			mthdcls793  mc793 = new mthdcls793();
			mthdcls794  mc794 = new mthdcls794();
			mthdcls795  mc795 = new mthdcls795();
			mthdcls796  mc796 = new mthdcls796();
			mthdcls797  mc797 = new mthdcls797();
			mthdcls798  mc798 = new mthdcls798();
			mthdcls799  mc799 = new mthdcls799();
			mthdcls800  mc800 = new mthdcls800();
			mthdcls801  mc801 = new mthdcls801();
			mthdcls802  mc802 = new mthdcls802();
			mthdcls803  mc803 = new mthdcls803();
			mthdcls804  mc804 = new mthdcls804();
			mthdcls805  mc805 = new mthdcls805();
			mthdcls806  mc806 = new mthdcls806();
			mthdcls807  mc807 = new mthdcls807();
			mthdcls808  mc808 = new mthdcls808();
			mthdcls809  mc809 = new mthdcls809();
			mthdcls810  mc810 = new mthdcls810();
			mthdcls811  mc811 = new mthdcls811();
			mthdcls812  mc812 = new mthdcls812();
			mthdcls813  mc813 = new mthdcls813();
			mthdcls814  mc814 = new mthdcls814();
			mthdcls815  mc815 = new mthdcls815();
			mthdcls816  mc816 = new mthdcls816();
			mthdcls817  mc817 = new mthdcls817();
			mthdcls818  mc818 = new mthdcls818();
			mthdcls819  mc819 = new mthdcls819();
			mthdcls820  mc820 = new mthdcls820();
			mthdcls821  mc821 = new mthdcls821();
			mthdcls822  mc822 = new mthdcls822();
			mthdcls823  mc823 = new mthdcls823();
			mthdcls824  mc824 = new mthdcls824();
			mthdcls825  mc825 = new mthdcls825();
			mthdcls826  mc826 = new mthdcls826();
			mthdcls827  mc827 = new mthdcls827();
			mthdcls828  mc828 = new mthdcls828();
			mthdcls829  mc829 = new mthdcls829();
			mthdcls830  mc830 = new mthdcls830();
			mthdcls831  mc831 = new mthdcls831();
			mthdcls832  mc832 = new mthdcls832();
			mthdcls833  mc833 = new mthdcls833();
			mthdcls834  mc834 = new mthdcls834();
			mthdcls835  mc835 = new mthdcls835();
			mthdcls836  mc836 = new mthdcls836();
			mthdcls837  mc837 = new mthdcls837();
			mthdcls838  mc838 = new mthdcls838();
			mthdcls839  mc839 = new mthdcls839();
			mthdcls840  mc840 = new mthdcls840();
			mthdcls841  mc841 = new mthdcls841();
			mthdcls842  mc842 = new mthdcls842();
			mthdcls843  mc843 = new mthdcls843();
			mthdcls844  mc844 = new mthdcls844();
			mthdcls845  mc845 = new mthdcls845();
			mthdcls846  mc846 = new mthdcls846();
			mthdcls847  mc847 = new mthdcls847();
			mthdcls848  mc848 = new mthdcls848();
			mthdcls849  mc849 = new mthdcls849();
			mthdcls850  mc850 = new mthdcls850();
			mthdcls851  mc851 = new mthdcls851();
			mthdcls852  mc852 = new mthdcls852();
			mthdcls853  mc853 = new mthdcls853();
			mthdcls854  mc854 = new mthdcls854();
			mthdcls855  mc855 = new mthdcls855();
			mthdcls856  mc856 = new mthdcls856();
			mthdcls857  mc857 = new mthdcls857();
			mthdcls858  mc858 = new mthdcls858();
			mthdcls859  mc859 = new mthdcls859();
			mthdcls860  mc860 = new mthdcls860();
			mthdcls861  mc861 = new mthdcls861();
			mthdcls862  mc862 = new mthdcls862();
			mthdcls863  mc863 = new mthdcls863();
			mthdcls864  mc864 = new mthdcls864();
			mthdcls865  mc865 = new mthdcls865();
			mthdcls866  mc866 = new mthdcls866();
			mthdcls867  mc867 = new mthdcls867();
			mthdcls868  mc868 = new mthdcls868();
			mthdcls869  mc869 = new mthdcls869();
			mthdcls870  mc870 = new mthdcls870();
			mthdcls871  mc871 = new mthdcls871();
			mthdcls872  mc872 = new mthdcls872();
			mthdcls873  mc873 = new mthdcls873();
			mthdcls874  mc874 = new mthdcls874();
			mthdcls875  mc875 = new mthdcls875();
			mthdcls876  mc876 = new mthdcls876();
			mthdcls877  mc877 = new mthdcls877();
			mthdcls878  mc878 = new mthdcls878();
			mthdcls879  mc879 = new mthdcls879();
			mthdcls880  mc880 = new mthdcls880();
			mthdcls881  mc881 = new mthdcls881();
			mthdcls882  mc882 = new mthdcls882();
			mthdcls883  mc883 = new mthdcls883();
			mthdcls884  mc884 = new mthdcls884();
			mthdcls885  mc885 = new mthdcls885();
			mthdcls886  mc886 = new mthdcls886();
			mthdcls887  mc887 = new mthdcls887();
			mthdcls888  mc888 = new mthdcls888();
			mthdcls889  mc889 = new mthdcls889();
			mthdcls890  mc890 = new mthdcls890();
			mthdcls891  mc891 = new mthdcls891();
			mthdcls892  mc892 = new mthdcls892();
			mthdcls893  mc893 = new mthdcls893();
			mthdcls894  mc894 = new mthdcls894();
			mthdcls895  mc895 = new mthdcls895();
			mthdcls896  mc896 = new mthdcls896();
			mthdcls897  mc897 = new mthdcls897();
			mthdcls898  mc898 = new mthdcls898();
			mthdcls899  mc899 = new mthdcls899();
			mthdcls900  mc900 = new mthdcls900();
			mthdcls901  mc901 = new mthdcls901();
			mthdcls902  mc902 = new mthdcls902();
			mthdcls903  mc903 = new mthdcls903();
			mthdcls904  mc904 = new mthdcls904();
			mthdcls905  mc905 = new mthdcls905();
			mthdcls906  mc906 = new mthdcls906();
			mthdcls907  mc907 = new mthdcls907();
			mthdcls908  mc908 = new mthdcls908();
			mthdcls909  mc909 = new mthdcls909();
			mthdcls910  mc910 = new mthdcls910();
			mthdcls911  mc911 = new mthdcls911();
			mthdcls912  mc912 = new mthdcls912();
			mthdcls913  mc913 = new mthdcls913();
			mthdcls914  mc914 = new mthdcls914();
			mthdcls915  mc915 = new mthdcls915();
			mthdcls916  mc916 = new mthdcls916();
			mthdcls917  mc917 = new mthdcls917();
			mthdcls918  mc918 = new mthdcls918();
			mthdcls919  mc919 = new mthdcls919();
			mthdcls920  mc920 = new mthdcls920();
			mthdcls921  mc921 = new mthdcls921();
			mthdcls922  mc922 = new mthdcls922();
			mthdcls923  mc923 = new mthdcls923();
			mthdcls924  mc924 = new mthdcls924();
			mthdcls925  mc925 = new mthdcls925();
			mthdcls926  mc926 = new mthdcls926();
			mthdcls927  mc927 = new mthdcls927();
			mthdcls928  mc928 = new mthdcls928();
			mthdcls929  mc929 = new mthdcls929();
			mthdcls930  mc930 = new mthdcls930();
			mthdcls931  mc931 = new mthdcls931();
			mthdcls932  mc932 = new mthdcls932();
			mthdcls933  mc933 = new mthdcls933();
			mthdcls934  mc934 = new mthdcls934();
			mthdcls935  mc935 = new mthdcls935();
			mthdcls936  mc936 = new mthdcls936();
			mthdcls937  mc937 = new mthdcls937();
			mthdcls938  mc938 = new mthdcls938();
			mthdcls939  mc939 = new mthdcls939();
			mthdcls940  mc940 = new mthdcls940();
			mthdcls941  mc941 = new mthdcls941();
			mthdcls942  mc942 = new mthdcls942();
			mthdcls943  mc943 = new mthdcls943();
			mthdcls944  mc944 = new mthdcls944();
			mthdcls945  mc945 = new mthdcls945();
			mthdcls946  mc946 = new mthdcls946();
			mthdcls947  mc947 = new mthdcls947();
			mthdcls948  mc948 = new mthdcls948();
			mthdcls949  mc949 = new mthdcls949();
			mthdcls950  mc950 = new mthdcls950();
			mthdcls951  mc951 = new mthdcls951();
			mthdcls952  mc952 = new mthdcls952();
			mthdcls953  mc953 = new mthdcls953();
			mthdcls954  mc954 = new mthdcls954();
			mthdcls955  mc955 = new mthdcls955();
			mthdcls956  mc956 = new mthdcls956();
			mthdcls957  mc957 = new mthdcls957();
			mthdcls958  mc958 = new mthdcls958();
			mthdcls959  mc959 = new mthdcls959();
			mthdcls960  mc960 = new mthdcls960();
			mthdcls961  mc961 = new mthdcls961();
			mthdcls962  mc962 = new mthdcls962();
			mthdcls963  mc963 = new mthdcls963();
			mthdcls964  mc964 = new mthdcls964();
			mthdcls965  mc965 = new mthdcls965();
			mthdcls966  mc966 = new mthdcls966();
			mthdcls967  mc967 = new mthdcls967();
			mthdcls968  mc968 = new mthdcls968();
			mthdcls969  mc969 = new mthdcls969();
			mthdcls970  mc970 = new mthdcls970();
			mthdcls971  mc971 = new mthdcls971();
			mthdcls972  mc972 = new mthdcls972();
			mthdcls973  mc973 = new mthdcls973();
			mthdcls974  mc974 = new mthdcls974();
			mthdcls975  mc975 = new mthdcls975();
			mthdcls976  mc976 = new mthdcls976();
			mthdcls977  mc977 = new mthdcls977();
			mthdcls978  mc978 = new mthdcls978();
			mthdcls979  mc979 = new mthdcls979();
			mthdcls980  mc980 = new mthdcls980();
			mthdcls981  mc981 = new mthdcls981();
			mthdcls982  mc982 = new mthdcls982();
			mthdcls983  mc983 = new mthdcls983();
			mthdcls984  mc984 = new mthdcls984();
			mthdcls985  mc985 = new mthdcls985();
			mthdcls986  mc986 = new mthdcls986();
			mthdcls987  mc987 = new mthdcls987();
			mthdcls988  mc988 = new mthdcls988();
			mthdcls989  mc989 = new mthdcls989();
			mthdcls990  mc990 = new mthdcls990();
			mthdcls991  mc991 = new mthdcls991();
			mthdcls992  mc992 = new mthdcls992();
			mthdcls993  mc993 = new mthdcls993();
			mthdcls994  mc994 = new mthdcls994();
			mthdcls995  mc995 = new mthdcls995();
			mthdcls996  mc996 = new mthdcls996();
			mthdcls997  mc997 = new mthdcls997();
			mthdcls998  mc998 = new mthdcls998();
			mthdcls999  mc999 = new mthdcls999();
			#endregion

			for (cnt = 0; cnt < 4; cnt++)
            {
                /* clean the CPU and filesystem cache */
                sum += polluteDataCache();
                retVal &= FlushInstructionCache(procHandle, IntPtr.Zero, UIntPtr.Zero);
                QueryPerformanceCounter(ref tsc_before);

                if (cnt < 3)
                {
                    sum++;
                }
                else
                {
					#region firstmeasurement
					var828 = mc0.method0(var955,var313);
					var711 = mc1.method1(var862,var753);
					var403 = mc2.method2(var959,var212);
					var388 = mc3.method3(var94,var940);
					var676 = mc4.method4(var981,var546);
					var467 = mc5.method5(var971,var904);
					var555 = mc6.method6(var852,var167);
					var3 = mc7.method7(var794,var658);
					var79 = mc8.method8(var425,var726);
					var4 = mc9.method9(var757,var986);
					var720 = mc10.method10(var26,var359);
					var975 = mc11.method11(var788,var634);
					var917 = mc12.method12(var204,var554);
					var977 = mc13.method13(var233,var674);
					var630 = mc14.method14(var108,var934);
					var650 = mc15.method15(var621,var611);
					var679 = mc16.method16(var336,var785);
					var351 = mc17.method17(var297,var366);
					var770 = mc18.method18(var120,var512);
					var971 = mc19.method19(var491,var369);
					var383 = mc20.method20(var145,var286);
					var383 = mc21.method21(var240,var258);
					var677 = mc22.method22(var421,var694);
					var797 = mc23.method23(var691,var248);
					var486 = mc24.method24(var863,var916);
					var485 = mc25.method25(var821,var947);
					var409 = mc26.method26(var363,var518);
					var257 = mc27.method27(var642,var410);
					var659 = mc28.method28(var49,var632);
					var906 = mc29.method29(var203,var208);
					var149 = mc30.method30(var543,var144);
					var19 = mc31.method31(var626,var88);
					var741 = mc32.method32(var829,var25);
					var628 = mc33.method33(var789,var522);
					var593 = mc34.method34(var483,var451);
					var942 = mc35.method35(var719,var556);
					var858 = mc36.method36(var733,var301);
					var579 = mc37.method37(var337,var471);
					var467 = mc38.method38(var119,var294);
					var176 = mc39.method39(var598,var480);
					var186 = mc40.method40(var647,var333);
					var416 = mc41.method41(var242,var793);
					var681 = mc42.method42(var155,var732);
					var621 = mc43.method43(var535,var755);
					var29 = mc44.method44(var309,var968);
					var946 = mc45.method45(var149,var144);
					var700 = mc46.method46(var925,var851);
					var702 = mc47.method47(var0,var367);
					var44 = mc48.method48(var582,var874);
					var158 = mc49.method49(var150,var493);
					var26 = mc50.method50(var453,var51);
					var261 = mc51.method51(var795,var309);
					var415 = mc52.method52(var874,var850);
					var99 = mc53.method53(var281,var41);
					var471 = mc54.method54(var41,var260);
					var576 = mc55.method55(var257,var616);
					var150 = mc56.method56(var839,var646);
					var825 = mc57.method57(var75,var181);
					var420 = mc58.method58(var959,var116);
					var3 = mc59.method59(var321,var783);
					var901 = mc60.method60(var533,var307);
					var187 = mc61.method61(var806,var644);
					var185 = mc62.method62(var613,var92);
					var456 = mc63.method63(var757,var155);
					var777 = mc64.method64(var498,var989);
					var937 = mc65.method65(var733,var280);
					var769 = mc66.method66(var612,var51);
					var537 = mc67.method67(var747,var856);
					var67 = mc68.method68(var920,var73);
					var922 = mc69.method69(var969,var972);
					var269 = mc70.method70(var41,var46);
					var288 = mc71.method71(var481,var911);
					var439 = mc72.method72(var950,var838);
					var265 = mc73.method73(var540,var677);
					var486 = mc74.method74(var785,var807);
					var735 = mc75.method75(var451,var389);
					var456 = mc76.method76(var822,var628);
					var818 = mc77.method77(var773,var817);
					var996 = mc78.method78(var69,var997);
					var282 = mc79.method79(var112,var590);
					var470 = mc80.method80(var579,var165);
					var447 = mc81.method81(var413,var126);
					var514 = mc82.method82(var664,var235);
					var369 = mc83.method83(var952,var711);
					var153 = mc84.method84(var949,var796);
					var907 = mc85.method85(var619,var518);
					var322 = mc86.method86(var325,var800);
					var791 = mc87.method87(var586,var39);
					var866 = mc88.method88(var62,var426);
					var47 = mc89.method89(var273,var333);
					var663 = mc90.method90(var593,var275);
					var145 = mc91.method91(var778,var877);
					var860 = mc92.method92(var373,var26);
					var452 = mc93.method93(var237,var998);
					var96 = mc94.method94(var618,var790);
					var559 = mc95.method95(var674,var1);
					var128 = mc96.method96(var741,var773);
					var949 = mc97.method97(var299,var854);
					var553 = mc98.method98(var587,var361);
					var247 = mc99.method99(var271,var972);
					var372 = mc100.method100(var271,var542);
					var922 = mc101.method101(var819,var337);
					var845 = mc102.method102(var924,var241);
					var272 = mc103.method103(var12,var699);
					var180 = mc104.method104(var265,var537);
					var824 = mc105.method105(var75,var932);
					var739 = mc106.method106(var337,var431);
					var204 = mc107.method107(var499,var120);
					var936 = mc108.method108(var599,var453);
					var288 = mc109.method109(var50,var32);
					var171 = mc110.method110(var339,var326);
					var35 = mc111.method111(var840,var674);
					var422 = mc112.method112(var699,var221);
					var245 = mc113.method113(var470,var10);
					var222 = mc114.method114(var979,var773);
					var840 = mc115.method115(var939,var928);
					var984 = mc116.method116(var400,var291);
					var40 = mc117.method117(var115,var815);
					var733 = mc118.method118(var489,var378);
					var957 = mc119.method119(var826,var773);
					var349 = mc120.method120(var122,var314);
					var771 = mc121.method121(var48,var283);
					var919 = mc122.method122(var24,var793);
					var933 = mc123.method123(var320,var640);
					var418 = mc124.method124(var696,var11);
					var99 = mc125.method125(var665,var211);
					var792 = mc126.method126(var655,var748);
					var959 = mc127.method127(var607,var620);
					var719 = mc128.method128(var475,var929);
					var36 = mc129.method129(var477,var707);
					var292 = mc130.method130(var614,var830);
					var693 = mc131.method131(var373,var166);
					var163 = mc132.method132(var427,var109);
					var444 = mc133.method133(var94,var941);
					var455 = mc134.method134(var613,var242);
					var803 = mc135.method135(var866,var474);
					var726 = mc136.method136(var499,var96);
					var365 = mc137.method137(var546,var66);
					var865 = mc138.method138(var665,var502);
					var105 = mc139.method139(var232,var674);
					var170 = mc140.method140(var3,var313);
					var792 = mc141.method141(var518,var663);
					var147 = mc142.method142(var307,var868);
					var498 = mc143.method143(var925,var768);
					var211 = mc144.method144(var419,var826);
					var452 = mc145.method145(var300,var336);
					var285 = mc146.method146(var202,var234);
					var418 = mc147.method147(var630,var982);
					var439 = mc148.method148(var777,var424);
					var445 = mc149.method149(var839,var131);
					var805 = mc150.method150(var896,var325);
					var968 = mc151.method151(var768,var831);
					var162 = mc152.method152(var536,var408);
					var369 = mc153.method153(var882,var315);
					var658 = mc154.method154(var793,var90);
					var315 = mc155.method155(var94,var655);
					var683 = mc156.method156(var468,var772);
					var748 = mc157.method157(var793,var794);
					var535 = mc158.method158(var314,var657);
					var645 = mc159.method159(var687,var371);
					var350 = mc160.method160(var184,var224);
					var336 = mc161.method161(var123,var461);
					var174 = mc162.method162(var974,var822);
					var847 = mc163.method163(var878,var620);
					var682 = mc164.method164(var511,var702);
					var474 = mc165.method165(var569,var104);
					var191 = mc166.method166(var632,var266);
					var709 = mc167.method167(var646,var181);
					var870 = mc168.method168(var173,var495);
					var24 = mc169.method169(var612,var807);
					var75 = mc170.method170(var582,var224);
					var717 = mc171.method171(var917,var30);
					var690 = mc172.method172(var941,var548);
					var1 = mc173.method173(var478,var639);
					var234 = mc174.method174(var70,var947);
					var99 = mc175.method175(var287,var691);
					var556 = mc176.method176(var539,var637);
					var249 = mc177.method177(var83,var811);
					var230 = mc178.method178(var6,var882);
					var799 = mc179.method179(var226,var862);
					var737 = mc180.method180(var921,var240);
					var536 = mc181.method181(var944,var273);
					var633 = mc182.method182(var106,var681);
					var684 = mc183.method183(var59,var562);
					var775 = mc184.method184(var616,var207);
					var313 = mc185.method185(var830,var672);
					var154 = mc186.method186(var13,var592);
					var450 = mc187.method187(var857,var688);
					var190 = mc188.method188(var521,var56);
					var614 = mc189.method189(var798,var773);
					var969 = mc190.method190(var553,var338);
					var723 = mc191.method191(var328,var705);
					var679 = mc192.method192(var256,var288);
					var111 = mc193.method193(var49,var376);
					var886 = mc194.method194(var286,var764);
					var222 = mc195.method195(var277,var989);
					var985 = mc196.method196(var124,var163);
					var897 = mc197.method197(var724,var721);
					var401 = mc198.method198(var481,var479);
					var904 = mc199.method199(var757,var687);
					var336 = mc200.method200(var808,var701);
					var845 = mc201.method201(var390,var472);
					var998 = mc202.method202(var607,var13);
					var198 = mc203.method203(var776,var655);
					var354 = mc204.method204(var362,var146);
					var77 = mc205.method205(var585,var432);
					var831 = mc206.method206(var804,var305);
					var377 = mc207.method207(var110,var306);
					var120 = mc208.method208(var69,var192);
					var38 = mc209.method209(var334,var791);
					var318 = mc210.method210(var325,var718);
					var531 = mc211.method211(var502,var767);
					var734 = mc212.method212(var84,var863);
					var929 = mc213.method213(var415,var259);
					var863 = mc214.method214(var985,var37);
					var29 = mc215.method215(var643,var145);
					var574 = mc216.method216(var817,var565);
					var747 = mc217.method217(var941,var958);
					var962 = mc218.method218(var851,var747);
					var135 = mc219.method219(var32,var926);
					var395 = mc220.method220(var189,var790);
					var501 = mc221.method221(var759,var772);
					var590 = mc222.method222(var544,var851);
					var883 = mc223.method223(var336,var472);
					var897 = mc224.method224(var488,var124);
					var674 = mc225.method225(var195,var901);
					var270 = mc226.method226(var870,var639);
					var30 = mc227.method227(var966,var235);
					var410 = mc228.method228(var468,var82);
					var474 = mc229.method229(var727,var456);
					var939 = mc230.method230(var130,var350);
					var524 = mc231.method231(var549,var729);
					var534 = mc232.method232(var523,var887);
					var122 = mc233.method233(var461,var817);
					var156 = mc234.method234(var880,var904);
					var767 = mc235.method235(var137,var107);
					var124 = mc236.method236(var465,var658);
					var747 = mc237.method237(var983,var744);
					var568 = mc238.method238(var149,var545);
					var529 = mc239.method239(var563,var5);
					var336 = mc240.method240(var619,var162);
					var474 = mc241.method241(var471,var433);
					var541 = mc242.method242(var804,var198);
					var385 = mc243.method243(var978,var587);
					var559 = mc244.method244(var456,var726);
					var536 = mc245.method245(var718,var634);
					var295 = mc246.method246(var704,var674);
					var320 = mc247.method247(var266,var185);
					var769 = mc248.method248(var395,var305);
					var112 = mc249.method249(var423,var725);
					var811 = mc250.method250(var929,var602);
					var631 = mc251.method251(var945,var119);
					var207 = mc252.method252(var285,var607);
					var145 = mc253.method253(var498,var666);
					var962 = mc254.method254(var252,var536);
					var629 = mc255.method255(var853,var102);
					var590 = mc256.method256(var598,var121);
					var10 = mc257.method257(var97,var439);
					var821 = mc258.method258(var599,var923);
					var516 = mc259.method259(var859,var785);
					var258 = mc260.method260(var320,var482);
					var213 = mc261.method261(var807,var552);
					var534 = mc262.method262(var506,var556);
					var607 = mc263.method263(var361,var759);
					var285 = mc264.method264(var712,var323);
					var641 = mc265.method265(var770,var103);
					var822 = mc266.method266(var301,var739);
					var83 = mc267.method267(var42,var220);
					var648 = mc268.method268(var338,var440);
					var108 = mc269.method269(var276,var242);
					var746 = mc270.method270(var755,var594);
					var956 = mc271.method271(var580,var804);
					var170 = mc272.method272(var587,var34);
					var196 = mc273.method273(var65,var958);
					var330 = mc274.method274(var118,var632);
					var529 = mc275.method275(var118,var953);
					var61 = mc276.method276(var965,var752);
					var234 = mc277.method277(var267,var29);
					var17 = mc278.method278(var83,var644);
					var529 = mc279.method279(var705,var36);
					var990 = mc280.method280(var322,var320);
					var780 = mc281.method281(var3,var647);
					var688 = mc282.method282(var550,var43);
					var87 = mc283.method283(var942,var940);
					var490 = mc284.method284(var29,var578);
					var8 = mc285.method285(var798,var269);
					var526 = mc286.method286(var470,var388);
					var510 = mc287.method287(var922,var287);
					var291 = mc288.method288(var327,var641);
					var988 = mc289.method289(var505,var865);
					var731 = mc290.method290(var436,var166);
					var519 = mc291.method291(var934,var922);
					var858 = mc292.method292(var715,var927);
					var361 = mc293.method293(var909,var669);
					var581 = mc294.method294(var77,var280);
					var54 = mc295.method295(var862,var635);
					var73 = mc296.method296(var876,var981);
					var811 = mc297.method297(var542,var539);
					var600 = mc298.method298(var252,var219);
					var214 = mc299.method299(var47,var566);
					var126 = mc300.method300(var870,var355);
					var855 = mc301.method301(var454,var318);
					var977 = mc302.method302(var394,var755);
					var605 = mc303.method303(var496,var445);
					var314 = mc304.method304(var343,var950);
					var711 = mc305.method305(var722,var533);
					var599 = mc306.method306(var543,var272);
					var679 = mc307.method307(var120,var407);
					var314 = mc308.method308(var96,var824);
					var256 = mc309.method309(var301,var491);
					var491 = mc310.method310(var513,var730);
					var582 = mc311.method311(var597,var303);
					var735 = mc312.method312(var47,var893);
					var880 = mc313.method313(var570,var608);
					var303 = mc314.method314(var4,var75);
					var801 = mc315.method315(var613,var31);
					var113 = mc316.method316(var418,var724);
					var551 = mc317.method317(var650,var882);
					var109 = mc318.method318(var512,var231);
					var310 = mc319.method319(var21,var533);
					var643 = mc320.method320(var270,var506);
					var72 = mc321.method321(var788,var455);
					var931 = mc322.method322(var715,var395);
					var223 = mc323.method323(var816,var633);
					var859 = mc324.method324(var37,var986);
					var33 = mc325.method325(var498,var234);
					var12 = mc326.method326(var157,var176);
					var398 = mc327.method327(var23,var971);
					var735 = mc328.method328(var17,var163);
					var72 = mc329.method329(var525,var652);
					var811 = mc330.method330(var786,var550);
					var486 = mc331.method331(var93,var711);
					var49 = mc332.method332(var817,var632);
					var914 = mc333.method333(var551,var321);
					var698 = mc334.method334(var164,var894);
					var73 = mc335.method335(var486,var819);
					var940 = mc336.method336(var786,var991);
					var195 = mc337.method337(var524,var162);
					var846 = mc338.method338(var701,var414);
					var571 = mc339.method339(var122,var80);
					var585 = mc340.method340(var706,var656);
					var25 = mc341.method341(var794,var478);
					var962 = mc342.method342(var930,var371);
					var347 = mc343.method343(var403,var189);
					var792 = mc344.method344(var471,var268);
					var991 = mc345.method345(var507,var190);
					var278 = mc346.method346(var8,var55);
					var9 = mc347.method347(var415,var432);
					var792 = mc348.method348(var334,var291);
					var375 = mc349.method349(var433,var980);
					var64 = mc350.method350(var931,var694);
					var576 = mc351.method351(var650,var50);
					var610 = mc352.method352(var362,var470);
					var627 = mc353.method353(var639,var914);
					var914 = mc354.method354(var422,var540);
					var860 = mc355.method355(var777,var212);
					var341 = mc356.method356(var456,var558);
					var916 = mc357.method357(var538,var348);
					var369 = mc358.method358(var500,var763);
					var369 = mc359.method359(var794,var570);
					var597 = mc360.method360(var220,var391);
					var607 = mc361.method361(var373,var994);
					var38 = mc362.method362(var302,var39);
					var110 = mc363.method363(var598,var151);
					var832 = mc364.method364(var68,var276);
					var694 = mc365.method365(var31,var108);
					var486 = mc366.method366(var782,var542);
					var38 = mc367.method367(var417,var370);
					var317 = mc368.method368(var387,var782);
					var300 = mc369.method369(var223,var125);
					var337 = mc370.method370(var686,var660);
					var815 = mc371.method371(var677,var729);
					var190 = mc372.method372(var3,var611);
					var651 = mc373.method373(var519,var256);
					var886 = mc374.method374(var473,var923);
					var146 = mc375.method375(var408,var850);
					var16 = mc376.method376(var302,var855);
					var483 = mc377.method377(var170,var148);
					var519 = mc378.method378(var160,var792);
					var844 = mc379.method379(var463,var664);
					var892 = mc380.method380(var372,var31);
					var35 = mc381.method381(var830,var520);
					var644 = mc382.method382(var529,var677);
					var29 = mc383.method383(var210,var245);
					var187 = mc384.method384(var854,var362);
					var513 = mc385.method385(var441,var971);
					var315 = mc386.method386(var887,var445);
					var487 = mc387.method387(var624,var130);
					var272 = mc388.method388(var925,var143);
					var664 = mc389.method389(var798,var680);
					var403 = mc390.method390(var491,var977);
					var4 = mc391.method391(var927,var998);
					var410 = mc392.method392(var699,var633);
					var719 = mc393.method393(var385,var391);
					var231 = mc394.method394(var530,var2);
					var695 = mc395.method395(var876,var321);
					var586 = mc396.method396(var442,var650);
					var616 = mc397.method397(var254,var31);
					var101 = mc398.method398(var233,var746);
					var742 = mc399.method399(var922,var14);
					var787 = mc400.method400(var477,var716);
					var960 = mc401.method401(var841,var234);
					var749 = mc402.method402(var895,var754);
					var177 = mc403.method403(var953,var332);
					var796 = mc404.method404(var371,var467);
					var753 = mc405.method405(var813,var879);
					var854 = mc406.method406(var642,var722);
					var690 = mc407.method407(var688,var241);
					var662 = mc408.method408(var922,var378);
					var729 = mc409.method409(var766,var5);
					var928 = mc410.method410(var42,var174);
					var145 = mc411.method411(var754,var284);
					var272 = mc412.method412(var599,var333);
					var827 = mc413.method413(var516,var555);
					var410 = mc414.method414(var948,var830);
					var753 = mc415.method415(var196,var945);
					var998 = mc416.method416(var528,var126);
					var254 = mc417.method417(var309,var389);
					var173 = mc418.method418(var132,var946);
					var714 = mc419.method419(var91,var731);
					var533 = mc420.method420(var821,var53);
					var399 = mc421.method421(var916,var933);
					var835 = mc422.method422(var365,var723);
					var140 = mc423.method423(var653,var545);
					var735 = mc424.method424(var785,var474);
					var589 = mc425.method425(var894,var982);
					var837 = mc426.method426(var262,var552);
					var633 = mc427.method427(var805,var916);
					var601 = mc428.method428(var764,var910);
					var388 = mc429.method429(var658,var13);
					var819 = mc430.method430(var109,var92);
					var270 = mc431.method431(var268,var636);
					var593 = mc432.method432(var999,var117);
					var70 = mc433.method433(var454,var316);
					var482 = mc434.method434(var639,var336);
					var626 = mc435.method435(var692,var111);
					var320 = mc436.method436(var258,var953);
					var838 = mc437.method437(var144,var148);
					var178 = mc438.method438(var489,var513);
					var962 = mc439.method439(var428,var698);
					var695 = mc440.method440(var546,var603);
					var560 = mc441.method441(var156,var827);
					var125 = mc442.method442(var120,var151);
					var104 = mc443.method443(var486,var338);
					var642 = mc444.method444(var756,var246);
					var935 = mc445.method445(var36,var436);
					var470 = mc446.method446(var864,var872);
					var200 = mc447.method447(var107,var671);
					var351 = mc448.method448(var806,var251);
					var421 = mc449.method449(var242,var290);
					var925 = mc450.method450(var991,var430);
					var831 = mc451.method451(var674,var878);
					var453 = mc452.method452(var484,var758);
					var318 = mc453.method453(var868,var637);
					var861 = mc454.method454(var195,var899);
					var650 = mc455.method455(var326,var951);
					var261 = mc456.method456(var158,var720);
					var985 = mc457.method457(var106,var654);
					var331 = mc458.method458(var469,var495);
					var156 = mc459.method459(var906,var926);
					var95 = mc460.method460(var291,var778);
					var767 = mc461.method461(var270,var907);
					var724 = mc462.method462(var20,var486);
					var983 = mc463.method463(var989,var463);
					var412 = mc464.method464(var942,var168);
					var569 = mc465.method465(var417,var970);
					var380 = mc466.method466(var418,var979);
					var241 = mc467.method467(var671,var869);
					var995 = mc468.method468(var642,var594);
					var389 = mc469.method469(var301,var774);
					var739 = mc470.method470(var936,var911);
					var676 = mc471.method471(var312,var991);
					var95 = mc472.method472(var400,var796);
					var81 = mc473.method473(var425,var271);
					var681 = mc474.method474(var215,var847);
					var475 = mc475.method475(var30,var975);
					var257 = mc476.method476(var425,var825);
					var979 = mc477.method477(var622,var345);
					var815 = mc478.method478(var400,var471);
					var780 = mc479.method479(var795,var124);
					var359 = mc480.method480(var326,var127);
					var313 = mc481.method481(var111,var966);
					var54 = mc482.method482(var966,var934);
					var695 = mc483.method483(var929,var708);
					var286 = mc484.method484(var300,var494);
					var355 = mc485.method485(var87,var232);
					var364 = mc486.method486(var64,var964);
					var291 = mc487.method487(var490,var384);
					var49 = mc488.method488(var913,var369);
					var514 = mc489.method489(var905,var50);
					var692 = mc490.method490(var563,var985);
					var294 = mc491.method491(var362,var575);
					var823 = mc492.method492(var727,var274);
					var140 = mc493.method493(var597,var825);
					var790 = mc494.method494(var879,var284);
					var723 = mc495.method495(var315,var842);
					var993 = mc496.method496(var512,var564);
					var637 = mc497.method497(var790,var437);
					var694 = mc498.method498(var664,var358);
					var169 = mc499.method499(var628,var694);
					var199 = mc500.method500(var62,var242);
					var180 = mc501.method501(var998,var885);
					var504 = mc502.method502(var924,var69);
					var158 = mc503.method503(var368,var321);
					var512 = mc504.method504(var902,var972);
					var727 = mc505.method505(var637,var229);
					var724 = mc506.method506(var430,var491);
					var69 = mc507.method507(var442,var702);
					var269 = mc508.method508(var469,var606);
					var791 = mc509.method509(var692,var27);
					var532 = mc510.method510(var398,var327);
					var768 = mc511.method511(var570,var409);
					var738 = mc512.method512(var296,var131);
					var35 = mc513.method513(var317,var439);
					var465 = mc514.method514(var341,var908);
					var113 = mc515.method515(var575,var194);
					var156 = mc516.method516(var227,var40);
					var428 = mc517.method517(var298,var283);
					var752 = mc518.method518(var2,var135);
					var928 = mc519.method519(var560,var742);
					var215 = mc520.method520(var497,var664);
					var304 = mc521.method521(var358,var199);
					var469 = mc522.method522(var286,var284);
					var735 = mc523.method523(var160,var931);
					var474 = mc524.method524(var574,var152);
					var968 = mc525.method525(var676,var265);
					var755 = mc526.method526(var375,var753);
					var869 = mc527.method527(var943,var680);
					var267 = mc528.method528(var367,var753);
					var177 = mc529.method529(var849,var747);
					var251 = mc530.method530(var294,var688);
					var839 = mc531.method531(var743,var843);
					var361 = mc532.method532(var677,var557);
					var102 = mc533.method533(var533,var937);
					var885 = mc534.method534(var413,var69);
					var179 = mc535.method535(var885,var905);
					var224 = mc536.method536(var833,var924);
					var77 = mc537.method537(var818,var914);
					var336 = mc538.method538(var5,var677);
					var854 = mc539.method539(var297,var932);
					var291 = mc540.method540(var944,var785);
					var418 = mc541.method541(var369,var575);
					var21 = mc542.method542(var828,var295);
					var356 = mc543.method543(var0,var225);
					var805 = mc544.method544(var600,var118);
					var940 = mc545.method545(var627,var706);
					var809 = mc546.method546(var310,var334);
					var395 = mc547.method547(var537,var782);
					var77 = mc548.method548(var592,var791);
					var736 = mc549.method549(var886,var850);
					var226 = mc550.method550(var590,var596);
					var980 = mc551.method551(var610,var171);
					var881 = mc552.method552(var587,var65);
					var428 = mc553.method553(var36,var168);
					var469 = mc554.method554(var149,var910);
					var214 = mc555.method555(var4,var407);
					var329 = mc556.method556(var455,var820);
					var221 = mc557.method557(var245,var83);
					var358 = mc558.method558(var368,var33);
					var928 = mc559.method559(var682,var444);
					var731 = mc560.method560(var479,var391);
					var771 = mc561.method561(var770,var348);
					var735 = mc562.method562(var527,var828);
					var512 = mc563.method563(var553,var423);
					var746 = mc564.method564(var592,var234);
					var990 = mc565.method565(var757,var633);
					var443 = mc566.method566(var655,var923);
					var802 = mc567.method567(var626,var981);
					var667 = mc568.method568(var822,var684);
					var766 = mc569.method569(var134,var463);
					var170 = mc570.method570(var48,var542);
					var76 = mc571.method571(var555,var78);
					var145 = mc572.method572(var789,var830);
					var912 = mc573.method573(var842,var827);
					var378 = mc574.method574(var90,var916);
					var697 = mc575.method575(var49,var720);
					var434 = mc576.method576(var678,var657);
					var654 = mc577.method577(var825,var155);
					var959 = mc578.method578(var798,var443);
					var577 = mc579.method579(var905,var145);
					var716 = mc580.method580(var413,var629);
					var491 = mc581.method581(var391,var284);
					var250 = mc582.method582(var400,var243);
					var346 = mc583.method583(var345,var275);
					var6 = mc584.method584(var697,var998);
					var964 = mc585.method585(var460,var469);
					var306 = mc586.method586(var765,var483);
					var34 = mc587.method587(var435,var71);
					var309 = mc588.method588(var550,var722);
					var262 = mc589.method589(var800,var509);
					var116 = mc590.method590(var436,var32);
					var951 = mc591.method591(var930,var875);
					var56 = mc592.method592(var319,var708);
					var700 = mc593.method593(var677,var308);
					var546 = mc594.method594(var835,var966);
					var570 = mc595.method595(var664,var849);
					var34 = mc596.method596(var584,var831);
					var987 = mc597.method597(var56,var175);
					var356 = mc598.method598(var126,var868);
					var473 = mc599.method599(var583,var661);
					var598 = mc600.method600(var950,var872);
					var532 = mc601.method601(var604,var311);
					var894 = mc602.method602(var755,var893);
					var929 = mc603.method603(var451,var552);
					var116 = mc604.method604(var15,var157);
					var595 = mc605.method605(var963,var952);
					var965 = mc606.method606(var352,var324);
					var279 = mc607.method607(var691,var302);
					var536 = mc608.method608(var503,var250);
					var341 = mc609.method609(var969,var476);
					var509 = mc610.method610(var631,var938);
					var245 = mc611.method611(var625,var544);
					var840 = mc612.method612(var302,var625);
					var390 = mc613.method613(var504,var209);
					var959 = mc614.method614(var974,var219);
					var485 = mc615.method615(var77,var257);
					var969 = mc616.method616(var419,var337);
					var662 = mc617.method617(var727,var493);
					var775 = mc618.method618(var481,var764);
					var561 = mc619.method619(var284,var114);
					var263 = mc620.method620(var712,var931);
					var90 = mc621.method621(var293,var272);
					var518 = mc622.method622(var132,var990);
					var359 = mc623.method623(var538,var632);
					var106 = mc624.method624(var390,var314);
					var246 = mc625.method625(var43,var462);
					var191 = mc626.method626(var804,var767);
					var837 = mc627.method627(var429,var732);
					var124 = mc628.method628(var95,var397);
					var384 = mc629.method629(var166,var734);
					var518 = mc630.method630(var821,var795);
					var459 = mc631.method631(var945,var568);
					var660 = mc632.method632(var561,var889);
					var423 = mc633.method633(var735,var647);
					var193 = mc634.method634(var804,var68);
					var285 = mc635.method635(var594,var118);
					var301 = mc636.method636(var474,var764);
					var629 = mc637.method637(var914,var328);
					var102 = mc638.method638(var781,var29);
					var320 = mc639.method639(var26,var789);
					var789 = mc640.method640(var47,var19);
					var43 = mc641.method641(var56,var457);
					var544 = mc642.method642(var616,var264);
					var878 = mc643.method643(var531,var405);
					var396 = mc644.method644(var945,var998);
					var146 = mc645.method645(var784,var474);
					var228 = mc646.method646(var137,var960);
					var684 = mc647.method647(var210,var701);
					var98 = mc648.method648(var509,var746);
					var156 = mc649.method649(var783,var944);
					var535 = mc650.method650(var552,var367);
					var651 = mc651.method651(var87,var990);
					var833 = mc652.method652(var355,var996);
					var34 = mc653.method653(var870,var698);
					var94 = mc654.method654(var586,var29);
					var843 = mc655.method655(var578,var998);
					var58 = mc656.method656(var794,var307);
					var872 = mc657.method657(var141,var961);
					var813 = mc658.method658(var511,var876);
					var511 = mc659.method659(var479,var874);
					var578 = mc660.method660(var785,var248);
					var741 = mc661.method661(var244,var447);
					var510 = mc662.method662(var125,var548);
					var321 = mc663.method663(var479,var258);
					var554 = mc664.method664(var996,var333);
					var309 = mc665.method665(var338,var109);
					var598 = mc666.method666(var130,var815);
					var932 = mc667.method667(var245,var531);
					var393 = mc668.method668(var882,var27);
					var817 = mc669.method669(var177,var447);
					var173 = mc670.method670(var369,var654);
					var448 = mc671.method671(var969,var68);
					var495 = mc672.method672(var713,var216);
					var628 = mc673.method673(var98,var182);
					var143 = mc674.method674(var31,var663);
					var185 = mc675.method675(var341,var430);
					var340 = mc676.method676(var269,var136);
					var885 = mc677.method677(var913,var127);
					var834 = mc678.method678(var62,var591);
					var361 = mc679.method679(var219,var847);
					var476 = mc680.method680(var747,var814);
					var903 = mc681.method681(var233,var154);
					var534 = mc682.method682(var842,var11);
					var432 = mc683.method683(var122,var854);
					var408 = mc684.method684(var251,var719);
					var395 = mc685.method685(var217,var762);
					var951 = mc686.method686(var186,var490);
					var498 = mc687.method687(var258,var480);
					var762 = mc688.method688(var59,var790);
					var582 = mc689.method689(var343,var302);
					var144 = mc690.method690(var457,var81);
					var290 = mc691.method691(var256,var92);
					var645 = mc692.method692(var753,var538);
					var474 = mc693.method693(var51,var715);
					var89 = mc694.method694(var820,var885);
					var951 = mc695.method695(var169,var255);
					var138 = mc696.method696(var124,var22);
					var198 = mc697.method697(var648,var83);
					var447 = mc698.method698(var542,var733);
					var942 = mc699.method699(var173,var410);
					var412 = mc700.method700(var88,var713);
					var20 = mc701.method701(var434,var231);
					var641 = mc702.method702(var172,var519);
					var103 = mc703.method703(var19,var224);
					var301 = mc704.method704(var640,var35);
					var1 = mc705.method705(var106,var19);
					var516 = mc706.method706(var938,var145);
					var965 = mc707.method707(var604,var967);
					var179 = mc708.method708(var423,var119);
					var806 = mc709.method709(var506,var944);
					var835 = mc710.method710(var153,var847);
					var474 = mc711.method711(var942,var625);
					var416 = mc712.method712(var466,var850);
					var218 = mc713.method713(var53,var152);
					var596 = mc714.method714(var74,var800);
					var147 = mc715.method715(var793,var951);
					var58 = mc716.method716(var481,var615);
					var50 = mc717.method717(var87,var287);
					var912 = mc718.method718(var452,var297);
					var104 = mc719.method719(var623,var597);
					var13 = mc720.method720(var555,var596);
					var354 = mc721.method721(var538,var135);
					var387 = mc722.method722(var655,var859);
					var964 = mc723.method723(var169,var131);
					var595 = mc724.method724(var455,var155);
					var41 = mc725.method725(var478,var791);
					var608 = mc726.method726(var657,var45);
					var403 = mc727.method727(var621,var452);
					var880 = mc728.method728(var955,var82);
					var457 = mc729.method729(var296,var863);
					var625 = mc730.method730(var831,var458);
					var355 = mc731.method731(var510,var638);
					var732 = mc732.method732(var85,var363);
					var432 = mc733.method733(var572,var311);
					var667 = mc734.method734(var305,var61);
					var763 = mc735.method735(var996,var174);
					var531 = mc736.method736(var839,var809);
					var523 = mc737.method737(var294,var354);
					var830 = mc738.method738(var310,var938);
					var575 = mc739.method739(var893,var491);
					var460 = mc740.method740(var121,var57);
					var785 = mc741.method741(var22,var299);
					var61 = mc742.method742(var155,var191);
					var421 = mc743.method743(var147,var71);
					var538 = mc744.method744(var80,var355);
					var545 = mc745.method745(var247,var283);
					var741 = mc746.method746(var696,var476);
					var375 = mc747.method747(var648,var423);
					var375 = mc748.method748(var222,var210);
					var750 = mc749.method749(var955,var307);
					var328 = mc750.method750(var610,var983);
					var763 = mc751.method751(var645,var356);
					var136 = mc752.method752(var598,var290);
					var839 = mc753.method753(var944,var677);
					var466 = mc754.method754(var840,var801);
					var928 = mc755.method755(var97,var205);
					var900 = mc756.method756(var891,var737);
					var142 = mc757.method757(var406,var639);
					var431 = mc758.method758(var970,var94);
					var535 = mc759.method759(var420,var68);
					var652 = mc760.method760(var749,var819);
					var436 = mc761.method761(var872,var180);
					var357 = mc762.method762(var6,var383);
					var165 = mc763.method763(var503,var679);
					var418 = mc764.method764(var876,var770);
					var716 = mc765.method765(var21,var788);
					var55 = mc766.method766(var917,var726);
					var894 = mc767.method767(var508,var582);
					var638 = mc768.method768(var726,var197);
					var554 = mc769.method769(var200,var895);
					var514 = mc770.method770(var632,var26);
					var735 = mc771.method771(var23,var331);
					var928 = mc772.method772(var290,var653);
					var631 = mc773.method773(var809,var603);
					var965 = mc774.method774(var608,var815);
					var785 = mc775.method775(var849,var33);
					var693 = mc776.method776(var690,var673);
					var764 = mc777.method777(var264,var750);
					var85 = mc778.method778(var904,var401);
					var30 = mc779.method779(var910,var432);
					var705 = mc780.method780(var167,var770);
					var824 = mc781.method781(var958,var191);
					var471 = mc782.method782(var273,var204);
					var24 = mc783.method783(var888,var845);
					var720 = mc784.method784(var376,var503);
					var798 = mc785.method785(var837,var962);
					var826 = mc786.method786(var657,var536);
					var420 = mc787.method787(var485,var827);
					var875 = mc788.method788(var418,var312);
					var708 = mc789.method789(var401,var407);
					var192 = mc790.method790(var947,var652);
					var47 = mc791.method791(var631,var955);
					var338 = mc792.method792(var130,var477);
					var53 = mc793.method793(var960,var410);
					var622 = mc794.method794(var480,var53);
					var772 = mc795.method795(var723,var258);
					var167 = mc796.method796(var882,var648);
					var696 = mc797.method797(var180,var256);
					var460 = mc798.method798(var74,var529);
					var394 = mc799.method799(var364,var614);
					var985 = mc800.method800(var143,var775);
					var309 = mc801.method801(var298,var436);
					var867 = mc802.method802(var374,var573);
					var264 = mc803.method803(var601,var175);
					var688 = mc804.method804(var346,var755);
					var479 = mc805.method805(var705,var228);
					var473 = mc806.method806(var287,var529);
					var792 = mc807.method807(var308,var820);
					var94 = mc808.method808(var4,var570);
					var113 = mc809.method809(var319,var6);
					var689 = mc810.method810(var136,var488);
					var697 = mc811.method811(var614,var807);
					var0 = mc812.method812(var478,var324);
					var862 = mc813.method813(var804,var230);
					var944 = mc814.method814(var594,var301);
					var552 = mc815.method815(var857,var929);
					var865 = mc816.method816(var898,var502);
					var995 = mc817.method817(var963,var52);
					var988 = mc818.method818(var77,var789);
					var500 = mc819.method819(var703,var999);
					var256 = mc820.method820(var390,var785);
					var790 = mc821.method821(var633,var142);
					var734 = mc822.method822(var561,var409);
					var923 = mc823.method823(var688,var201);
					var159 = mc824.method824(var805,var237);
					var553 = mc825.method825(var809,var156);
					var918 = mc826.method826(var714,var932);
					var303 = mc827.method827(var489,var15);
					var307 = mc828.method828(var761,var883);
					var183 = mc829.method829(var840,var4);
					var691 = mc830.method830(var546,var677);
					var541 = mc831.method831(var458,var869);
					var301 = mc832.method832(var618,var587);
					var972 = mc833.method833(var677,var80);
					var82 = mc834.method834(var684,var812);
					var612 = mc835.method835(var127,var803);
					var95 = mc836.method836(var939,var6);
					var300 = mc837.method837(var127,var840);
					var626 = mc838.method838(var409,var605);
					var623 = mc839.method839(var949,var469);
					var55 = mc840.method840(var36,var545);
					var751 = mc841.method841(var539,var301);
					var414 = mc842.method842(var617,var953);
					var892 = mc843.method843(var12,var641);
					var23 = mc844.method844(var207,var129);
					var46 = mc845.method845(var139,var614);
					var73 = mc846.method846(var826,var627);
					var477 = mc847.method847(var986,var878);
					var287 = mc848.method848(var485,var571);
					var418 = mc849.method849(var279,var661);
					var483 = mc850.method850(var334,var714);
					var229 = mc851.method851(var409,var112);
					var765 = mc852.method852(var660,var244);
					var928 = mc853.method853(var45,var412);
					var612 = mc854.method854(var350,var247);
					var689 = mc855.method855(var492,var805);
					var636 = mc856.method856(var26,var194);
					var412 = mc857.method857(var581,var816);
					var432 = mc858.method858(var573,var749);
					var937 = mc859.method859(var238,var79);
					var925 = mc860.method860(var473,var109);
					var457 = mc861.method861(var139,var141);
					var648 = mc862.method862(var593,var417);
					var191 = mc863.method863(var911,var88);
					var689 = mc864.method864(var965,var763);
					var139 = mc865.method865(var240,var175);
					var549 = mc866.method866(var351,var343);
					var42 = mc867.method867(var609,var623);
					var501 = mc868.method868(var493,var758);
					var466 = mc869.method869(var809,var929);
					var696 = mc870.method870(var63,var728);
					var574 = mc871.method871(var130,var537);
					var847 = mc872.method872(var516,var317);
					var154 = mc873.method873(var834,var524);
					var381 = mc874.method874(var981,var907);
					var626 = mc875.method875(var237,var550);
					var9 = mc876.method876(var703,var778);
					var33 = mc877.method877(var33,var550);
					var208 = mc878.method878(var84,var211);
					var520 = mc879.method879(var585,var315);
					var229 = mc880.method880(var259,var869);
					var993 = mc881.method881(var284,var474);
					var540 = mc882.method882(var579,var442);
					var762 = mc883.method883(var304,var776);
					var569 = mc884.method884(var519,var735);
					var121 = mc885.method885(var111,var128);
					var342 = mc886.method886(var762,var923);
					var429 = mc887.method887(var260,var900);
					var728 = mc888.method888(var257,var45);
					var688 = mc889.method889(var349,var331);
					var408 = mc890.method890(var456,var476);
					var466 = mc891.method891(var336,var536);
					var732 = mc892.method892(var383,var109);
					var282 = mc893.method893(var534,var305);
					var413 = mc894.method894(var973,var288);
					var113 = mc895.method895(var42,var57);
					var785 = mc896.method896(var574,var211);
					var958 = mc897.method897(var652,var121);
					var894 = mc898.method898(var955,var972);
					var754 = mc899.method899(var969,var706);
					var646 = mc900.method900(var292,var641);
					var833 = mc901.method901(var245,var814);
					var813 = mc902.method902(var215,var202);
					var776 = mc903.method903(var410,var945);
					var313 = mc904.method904(var828,var718);
					var120 = mc905.method905(var332,var387);
					var868 = mc906.method906(var228,var758);
					var570 = mc907.method907(var372,var872);
					var547 = mc908.method908(var707,var729);
					var66 = mc909.method909(var236,var881);
					var422 = mc910.method910(var581,var747);
					var418 = mc911.method911(var534,var182);
					var461 = mc912.method912(var372,var432);
					var106 = mc913.method913(var485,var111);
					var540 = mc914.method914(var529,var334);
					var732 = mc915.method915(var80,var847);
					var586 = mc916.method916(var150,var259);
					var495 = mc917.method917(var795,var62);
					var84 = mc918.method918(var260,var799);
					var18 = mc919.method919(var372,var628);
					var985 = mc920.method920(var389,var758);
					var271 = mc921.method921(var321,var599);
					var623 = mc922.method922(var472,var407);
					var200 = mc923.method923(var573,var264);
					var734 = mc924.method924(var856,var843);
					var150 = mc925.method925(var770,var859);
					var410 = mc926.method926(var481,var231);
					var113 = mc927.method927(var742,var798);
					var921 = mc928.method928(var530,var427);
					var227 = mc929.method929(var417,var259);
					var247 = mc930.method930(var513,var232);
					var503 = mc931.method931(var157,var108);
					var529 = mc932.method932(var523,var176);
					var92 = mc933.method933(var44,var298);
					var783 = mc934.method934(var313,var325);
					var221 = mc935.method935(var623,var124);
					var993 = mc936.method936(var661,var895);
					var887 = mc937.method937(var305,var669);
					var69 = mc938.method938(var443,var268);
					var608 = mc939.method939(var205,var704);
					var603 = mc940.method940(var292,var842);
					var585 = mc941.method941(var617,var427);
					var198 = mc942.method942(var488,var10);
					var85 = mc943.method943(var254,var252);
					var886 = mc944.method944(var340,var607);
					var491 = mc945.method945(var471,var109);
					var603 = mc946.method946(var196,var427);
					var504 = mc947.method947(var651,var352);
					var632 = mc948.method948(var53,var378);
					var729 = mc949.method949(var836,var449);
					var136 = mc950.method950(var95,var899);
					var407 = mc951.method951(var864,var660);
					var934 = mc952.method952(var265,var142);
					var145 = mc953.method953(var109,var854);
					var248 = mc954.method954(var805,var222);
					var790 = mc955.method955(var440,var748);
					var626 = mc956.method956(var810,var420);
					var338 = mc957.method957(var53,var665);
					var394 = mc958.method958(var796,var341);
					var827 = mc959.method959(var830,var867);
					var288 = mc960.method960(var87,var738);
					var325 = mc961.method961(var675,var507);
					var353 = mc962.method962(var199,var81);
					var137 = mc963.method963(var923,var650);
					var314 = mc964.method964(var242,var505);
					var161 = mc965.method965(var67,var679);
					var115 = mc966.method966(var933,var786);
					var427 = mc967.method967(var729,var111);
					var415 = mc968.method968(var403,var590);
					var152 = mc969.method969(var290,var74);
					var759 = mc970.method970(var151,var508);
					var355 = mc971.method971(var301,var795);
					var596 = mc972.method972(var563,var134);
					var949 = mc973.method973(var608,var278);
					var847 = mc974.method974(var849,var661);
					var162 = mc975.method975(var733,var637);
					var860 = mc976.method976(var271,var269);
					var551 = mc977.method977(var799,var584);
					var818 = mc978.method978(var450,var867);
					var566 = mc979.method979(var742,var41);
					var419 = mc980.method980(var653,var433);
					var488 = mc981.method981(var882,var876);
					var556 = mc982.method982(var716,var206);
					var146 = mc983.method983(var696,var627);
					var207 = mc984.method984(var567,var491);
					var193 = mc985.method985(var415,var820);
					var280 = mc986.method986(var857,var653);
					var143 = mc987.method987(var65,var44);
					var855 = mc988.method988(var171,var295);
					var251 = mc989.method989(var959,var527);
					var150 = mc990.method990(var922,var157);
					var561 = mc991.method991(var0,var868);
					var422 = mc992.method992(var837,var150);
					var384 = mc993.method993(var420,var403);
					var187 = mc994.method994(var168,var567);
					var284 = mc995.method995(var410,var231);
					var193 = mc996.method996(var547,var571);
					var442 = mc997.method997(var706,var405);
					var722 = mc998.method998(var661,var795);
					var686 = mc999.method999(var604,var724);
					#endregion
				}
                QueryPerformanceCounter(ref tsc_after);
                time1 = tsc_after - tsc_before - min;
                sum++;
            }



			for (cnt = 0; cnt < 6; cnt++)
            {
                /* clean the CPU and filesystem cache */
                sum += polluteDataCache();
                retVal &= FlushInstructionCache(procHandle, IntPtr.Zero, UIntPtr.Zero);
                QueryPerformanceCounter(ref tsc_before);

                if (cnt < 3)
                {
                    sum++;
                }
                else
                {
					#region repeatedmeasurements
					var841 = mc0.method0(var727, var809);
					var174 = mc1.method1(var525, var603);
					var350 = mc2.method2(var795, var154);
					var179 = mc3.method3(var854, var594);
					var523 = mc4.method4(var40, var798);
					var760 = mc5.method5(var41, var79);
					var696 = mc6.method6(var533, var779);
					var695 = mc7.method7(var238, var556);
					var814 = mc8.method8(var459, var358);
					var392 = mc9.method9(var45, var32);
					var934 = mc10.method10(var615, var77);
					var537 = mc11.method11(var30, var599);
					var23 = mc12.method12(var530, var563);
					var344 = mc13.method13(var400, var626);
					var709 = mc14.method14(var811, var428);
					var27 = mc15.method15(var423, var239);
					var255 = mc16.method16(var90, var327);
					var212 = mc17.method17(var679, var132);
					var757 = mc18.method18(var226, var650);
					var272 = mc19.method19(var144, var925);
					var580 = mc20.method20(var820, var231);
					var810 = mc21.method21(var779, var228);
					var885 = mc22.method22(var712, var612);
					var771 = mc23.method23(var337, var802);
					var824 = mc24.method24(var606, var205);
					var567 = mc25.method25(var16, var105);
					var798 = mc26.method26(var588, var809);
					var86 = mc27.method27(var248, var120);
					var452 = mc28.method28(var113, var383);
					var266 = mc29.method29(var758, var802);
					var714 = mc30.method30(var311, var917);
					var791 = mc31.method31(var6, var598);
					var802 = mc32.method32(var102, var605);
					var860 = mc33.method33(var10, var318);
					var440 = mc34.method34(var667, var281);
					var241 = mc35.method35(var964, var559);
					var680 = mc36.method36(var643, var843);
					var383 = mc37.method37(var513, var342);
					var211 = mc38.method38(var269, var902);
					var440 = mc39.method39(var803, var180);
					var425 = mc40.method40(var782, var106);
					var752 = mc41.method41(var761, var19);
					var362 = mc42.method42(var156, var324);
					var964 = mc43.method43(var603, var457);
					var424 = mc44.method44(var154, var745);
					var425 = mc45.method45(var572, var905);
					var908 = mc46.method46(var183, var211);
					var942 = mc47.method47(var463, var577);
					var376 = mc48.method48(var931, var204);
					var165 = mc49.method49(var29, var987);
					var235 = mc50.method50(var646, var777);
					var641 = mc51.method51(var257, var552);
					var893 = mc52.method52(var285, var922);
					var855 = mc53.method53(var668, var58);
					var650 = mc54.method54(var496, var432);
					var900 = mc55.method55(var920, var936);
					var965 = mc56.method56(var279, var64);
					var737 = mc57.method57(var410, var816);
					var945 = mc58.method58(var779, var4);
					var464 = mc59.method59(var495, var208);
					var125 = mc60.method60(var77, var234);
					var469 = mc61.method61(var295, var545);
					var807 = mc62.method62(var927, var722);
					var844 = mc63.method63(var505, var636);
					var940 = mc64.method64(var629, var119);
					var473 = mc65.method65(var531, var646);
					var632 = mc66.method66(var597, var927);
					var740 = mc67.method67(var669, var821);
					var861 = mc68.method68(var158, var411);
					var308 = mc69.method69(var346, var711);
					var745 = mc70.method70(var965, var563);
					var77 = mc71.method71(var350, var31);
					var117 = mc72.method72(var20, var377);
					var959 = mc73.method73(var368, var273);
					var303 = mc74.method74(var367, var352);
					var324 = mc75.method75(var67, var589);
					var954 = mc76.method76(var786, var367);
					var695 = mc77.method77(var118, var783);
					var463 = mc78.method78(var160, var513);
					var156 = mc79.method79(var119, var263);
					var428 = mc80.method80(var786, var550);
					var762 = mc81.method81(var475, var231);
					var333 = mc82.method82(var572, var277);
					var794 = mc83.method83(var406, var942);
					var692 = mc84.method84(var845, var230);
					var231 = mc85.method85(var622, var886);
					var357 = mc86.method86(var701, var644);
					var254 = mc87.method87(var189, var82);
					var283 = mc88.method88(var958, var415);
					var800 = mc89.method89(var601, var118);
					var698 = mc90.method90(var544, var743);
					var582 = mc91.method91(var553, var426);
					var581 = mc92.method92(var457, var137);
					var120 = mc93.method93(var702, var181);
					var231 = mc94.method94(var252, var141);
					var112 = mc95.method95(var506, var35);
					var500 = mc96.method96(var504, var744);
					var713 = mc97.method97(var555, var0);
					var565 = mc98.method98(var883, var42);
					var967 = mc99.method99(var209, var49);
					var650 = mc100.method100(var875, var435);
					var156 = mc101.method101(var92, var224);
					var710 = mc102.method102(var439, var704);
					var117 = mc103.method103(var724, var586);
					var385 = mc104.method104(var852, var956);
					var931 = mc105.method105(var699, var188);
					var516 = mc106.method106(var399, var915);
					var448 = mc107.method107(var590, var551);
					var468 = mc108.method108(var822, var109);
					var586 = mc109.method109(var489, var328);
					var715 = mc110.method110(var142, var753);
					var20 = mc111.method111(var395, var115);
					var795 = mc112.method112(var378, var296);
					var210 = mc113.method113(var413, var318);
					var518 = mc114.method114(var100, var588);
					var296 = mc115.method115(var122, var3);
					var532 = mc116.method116(var742, var774);
					var456 = mc117.method117(var477, var881);
					var334 = mc118.method118(var507, var122);
					var415 = mc119.method119(var761, var977);
					var429 = mc120.method120(var331, var143);
					var494 = mc121.method121(var704, var406);
					var67 = mc122.method122(var285, var264);
					var660 = mc123.method123(var808, var695);
					var656 = mc124.method124(var773, var625);
					var459 = mc125.method125(var971, var709);
					var217 = mc126.method126(var960, var700);
					var693 = mc127.method127(var825, var512);
					var899 = mc128.method128(var383, var998);
					var258 = mc129.method129(var315, var988);
					var48 = mc130.method130(var510, var114);
					var635 = mc131.method131(var401, var717);
					var661 = mc132.method132(var744, var475);
					var129 = mc133.method133(var325, var413);
					var786 = mc134.method134(var571, var42);
					var80 = mc135.method135(var630, var964);
					var982 = mc136.method136(var950, var509);
					var863 = mc137.method137(var99, var772);
					var928 = mc138.method138(var918, var216);
					var507 = mc139.method139(var92, var986);
					var745 = mc140.method140(var322, var810);
					var134 = mc141.method141(var335, var394);
					var909 = mc142.method142(var85, var731);
					var544 = mc143.method143(var828, var6);
					var726 = mc144.method144(var267, var451);
					var836 = mc145.method145(var594, var53);
					var583 = mc146.method146(var980, var167);
					var491 = mc147.method147(var165, var329);
					var243 = mc148.method148(var725, var700);
					var980 = mc149.method149(var300, var6);
					var808 = mc150.method150(var576, var13);
					var930 = mc151.method151(var300, var318);
					var687 = mc152.method152(var518, var119);
					var206 = mc153.method153(var486, var577);
					var380 = mc154.method154(var1, var782);
					var17 = mc155.method155(var698, var769);
					var528 = mc156.method156(var202, var218);
					var236 = mc157.method157(var207, var86);
					var178 = mc158.method158(var168, var308);
					var880 = mc159.method159(var834, var16);
					var707 = mc160.method160(var391, var965);
					var524 = mc161.method161(var58, var251);
					var626 = mc162.method162(var724, var484);
					var433 = mc163.method163(var138, var824);
					var525 = mc164.method164(var381, var761);
					var930 = mc165.method165(var283, var79);
					var151 = mc166.method166(var74, var417);
					var819 = mc167.method167(var145, var283);
					var299 = mc168.method168(var417, var611);
					var489 = mc169.method169(var871, var48);
					var692 = mc170.method170(var962, var33);
					var686 = mc171.method171(var68, var661);
					var51 = mc172.method172(var999, var240);
					var852 = mc173.method173(var734, var618);
					var618 = mc174.method174(var453, var785);
					var398 = mc175.method175(var90, var923);
					var786 = mc176.method176(var760, var557);
					var819 = mc177.method177(var9, var785);
					var324 = mc178.method178(var745, var357);
					var278 = mc179.method179(var456, var397);
					var199 = mc180.method180(var626, var484);
					var632 = mc181.method181(var699, var519);
					var206 = mc182.method182(var71, var595);
					var362 = mc183.method183(var840, var359);
					var292 = mc184.method184(var390, var517);
					var597 = mc185.method185(var810, var359);
					var959 = mc186.method186(var554, var59);
					var332 = mc187.method187(var32, var473);
					var848 = mc188.method188(var65, var477);
					var400 = mc189.method189(var986, var548);
					var454 = mc190.method190(var980, var403);
					var877 = mc191.method191(var12, var374);
					var325 = mc192.method192(var227, var936);
					var187 = mc193.method193(var588, var730);
					var964 = mc194.method194(var232, var701);
					var224 = mc195.method195(var786, var535);
					var937 = mc196.method196(var259, var267);
					var956 = mc197.method197(var292, var907);
					var942 = mc198.method198(var219, var223);
					var607 = mc199.method199(var620, var325);
					var193 = mc200.method200(var978, var135);
					var407 = mc201.method201(var774, var109);
					var395 = mc202.method202(var59, var689);
					var292 = mc203.method203(var811, var275);
					var422 = mc204.method204(var699, var286);
					var102 = mc205.method205(var40, var125);
					var530 = mc206.method206(var629, var841);
					var869 = mc207.method207(var780, var661);
					var355 = mc208.method208(var476, var844);
					var995 = mc209.method209(var102, var902);
					var979 = mc210.method210(var265, var538);
					var643 = mc211.method211(var376, var312);
					var308 = mc212.method212(var264, var946);
					var598 = mc213.method213(var184, var661);
					var4 = mc214.method214(var308, var417);
					var398 = mc215.method215(var176, var630);
					var552 = mc216.method216(var466, var374);
					var227 = mc217.method217(var505, var717);
					var345 = mc218.method218(var927, var439);
					var491 = mc219.method219(var31, var461);
					var801 = mc220.method220(var130, var113);
					var90 = mc221.method221(var107, var149);
					var270 = mc222.method222(var114, var282);
					var887 = mc223.method223(var926, var409);
					var573 = mc224.method224(var64, var254);
					var614 = mc225.method225(var364, var62);
					var315 = mc226.method226(var427, var36);
					var352 = mc227.method227(var964, var640);
					var101 = mc228.method228(var848, var152);
					var448 = mc229.method229(var535, var226);
					var42 = mc230.method230(var193, var982);
					var59 = mc231.method231(var671, var774);
					var88 = mc232.method232(var940, var53);
					var802 = mc233.method233(var34, var114);
					var314 = mc234.method234(var125, var429);
					var21 = mc235.method235(var263, var865);
					var616 = mc236.method236(var497, var775);
					var991 = mc237.method237(var956, var804);
					var419 = mc238.method238(var607, var148);
					var54 = mc239.method239(var418, var333);
					var61 = mc240.method240(var330, var60);
					var479 = mc241.method241(var853, var812);
					var95 = mc242.method242(var447, var634);
					var232 = mc243.method243(var351, var499);
					var446 = mc244.method244(var818, var651);
					var44 = mc245.method245(var396, var159);
					var220 = mc246.method246(var494, var700);
					var98 = mc247.method247(var29, var202);
					var165 = mc248.method248(var712, var133);
					var502 = mc249.method249(var205, var859);
					var679 = mc250.method250(var640, var305);
					var820 = mc251.method251(var451, var535);
					var668 = mc252.method252(var496, var473);
					var384 = mc253.method253(var625, var103);
					var644 = mc254.method254(var121, var796);
					var677 = mc255.method255(var962, var753);
					var638 = mc256.method256(var707, var474);
					var645 = mc257.method257(var848, var559);
					var653 = mc258.method258(var421, var25);
					var239 = mc259.method259(var27, var318);
					var144 = mc260.method260(var598, var974);
					var250 = mc261.method261(var607, var247);
					var854 = mc262.method262(var324, var21);
					var974 = mc263.method263(var82, var643);
					var520 = mc264.method264(var513, var20);
					var54 = mc265.method265(var249, var469);
					var549 = mc266.method266(var744, var687);
					var893 = mc267.method267(var474, var886);
					var715 = mc268.method268(var80, var665);
					var55 = mc269.method269(var213, var204);
					var680 = mc270.method270(var343, var474);
					var498 = mc271.method271(var302, var981);
					var582 = mc272.method272(var130, var101);
					var741 = mc273.method273(var427, var492);
					var204 = mc274.method274(var894, var20);
					var580 = mc275.method275(var171, var961);
					var844 = mc276.method276(var572, var755);
					var969 = mc277.method277(var26, var823);
					var638 = mc278.method278(var800, var124);
					var475 = mc279.method279(var947, var625);
					var665 = mc280.method280(var723, var222);
					var280 = mc281.method281(var546, var589);
					var438 = mc282.method282(var626, var493);
					var439 = mc283.method283(var883, var287);
					var625 = mc284.method284(var976, var989);
					var717 = mc285.method285(var866, var650);
					var248 = mc286.method286(var914, var956);
					var190 = mc287.method287(var107, var587);
					var538 = mc288.method288(var956, var121);
					var194 = mc289.method289(var952, var713);
					var543 = mc290.method290(var956, var637);
					var662 = mc291.method291(var857, var140);
					var866 = mc292.method292(var227, var905);
					var302 = mc293.method293(var713, var520);
					var713 = mc294.method294(var929, var615);
					var564 = mc295.method295(var862, var439);
					var284 = mc296.method296(var681, var679);
					var930 = mc297.method297(var523, var233);
					var82 = mc298.method298(var709, var86);
					var560 = mc299.method299(var422, var406);
					var722 = mc300.method300(var211, var721);
					var190 = mc301.method301(var725, var363);
					var574 = mc302.method302(var696, var360);
					var424 = mc303.method303(var855, var427);
					var365 = mc304.method304(var566, var235);
					var26 = mc305.method305(var667, var874);
					var504 = mc306.method306(var829, var870);
					var560 = mc307.method307(var771, var545);
					var990 = mc308.method308(var331, var234);
					var447 = mc309.method309(var936, var494);
					var566 = mc310.method310(var170, var866);
					var480 = mc311.method311(var447, var286);
					var154 = mc312.method312(var146, var694);
					var589 = mc313.method313(var897, var987);
					var934 = mc314.method314(var455, var811);
					var118 = mc315.method315(var158, var978);
					var242 = mc316.method316(var750, var474);
					var639 = mc317.method317(var623, var927);
					var840 = mc318.method318(var551, var344);
					var240 = mc319.method319(var742, var439);
					var208 = mc320.method320(var427, var2);
					var771 = mc321.method321(var526, var867);
					var493 = mc322.method322(var910, var754);
					var117 = mc323.method323(var867, var688);
					var631 = mc324.method324(var754, var355);
					var231 = mc325.method325(var937, var843);
					var704 = mc326.method326(var439, var986);
					var994 = mc327.method327(var704, var497);
					var286 = mc328.method328(var138, var168);
					var94 = mc329.method329(var953, var580);
					var792 = mc330.method330(var244, var391);
					var576 = mc331.method331(var721, var208);
					var356 = mc332.method332(var180, var99);
					var580 = mc333.method333(var180, var314);
					var273 = mc334.method334(var803, var764);
					var479 = mc335.method335(var934, var126);
					var641 = mc336.method336(var702, var382);
					var249 = mc337.method337(var286, var161);
					var646 = mc338.method338(var964, var35);
					var425 = mc339.method339(var49, var318);
					var510 = mc340.method340(var313, var811);
					var174 = mc341.method341(var936, var553);
					var415 = mc342.method342(var827, var989);
					var876 = mc343.method343(var296, var810);
					var202 = mc344.method344(var2, var56);
					var736 = mc345.method345(var708, var542);
					var850 = mc346.method346(var322, var103);
					var743 = mc347.method347(var45, var635);
					var70 = mc348.method348(var479, var433);
					var217 = mc349.method349(var640, var168);
					var793 = mc350.method350(var529, var190);
					var223 = mc351.method351(var284, var369);
					var112 = mc352.method352(var271, var747);
					var27 = mc353.method353(var770, var391);
					var275 = mc354.method354(var319, var598);
					var639 = mc355.method355(var204, var650);
					var91 = mc356.method356(var167, var531);
					var817 = mc357.method357(var785, var881);
					var524 = mc358.method358(var981, var123);
					var587 = mc359.method359(var890, var566);
					var441 = mc360.method360(var143, var80);
					var962 = mc361.method361(var105, var904);
					var535 = mc362.method362(var883, var403);
					var416 = mc363.method363(var532, var57);
					var450 = mc364.method364(var683, var790);
					var285 = mc365.method365(var957, var164);
					var110 = mc366.method366(var88, var356);
					var845 = mc367.method367(var327, var73);
					var727 = mc368.method368(var649, var449);
					var227 = mc369.method369(var118, var379);
					var834 = mc370.method370(var228, var867);
					var330 = mc371.method371(var495, var712);
					var941 = mc372.method372(var591, var528);
					var312 = mc373.method373(var681, var40);
					var539 = mc374.method374(var3, var810);
					var685 = mc375.method375(var490, var711);
					var153 = mc376.method376(var874, var532);
					var895 = mc377.method377(var469, var510);
					var732 = mc378.method378(var212, var275);
					var749 = mc379.method379(var467, var392);
					var963 = mc380.method380(var943, var354);
					var91 = mc381.method381(var734, var491);
					var518 = mc382.method382(var447, var872);
					var105 = mc383.method383(var795, var246);
					var10 = mc384.method384(var235, var556);
					var460 = mc385.method385(var375, var816);
					var341 = mc386.method386(var515, var373);
					var699 = mc387.method387(var760, var725);
					var415 = mc388.method388(var890, var874);
					var776 = mc389.method389(var596, var3);
					var194 = mc390.method390(var493, var718);
					var423 = mc391.method391(var517, var435);
					var30 = mc392.method392(var304, var446);
					var349 = mc393.method393(var309, var674);
					var370 = mc394.method394(var638, var500);
					var832 = mc395.method395(var135, var744);
					var95 = mc396.method396(var841, var337);
					var499 = mc397.method397(var153, var463);
					var197 = mc398.method398(var470, var224);
					var931 = mc399.method399(var573, var299);
					var460 = mc400.method400(var213, var0);
					var522 = mc401.method401(var795, var120);
					var875 = mc402.method402(var371, var734);
					var723 = mc403.method403(var325, var631);
					var721 = mc404.method404(var499, var177);
					var874 = mc405.method405(var546, var296);
					var527 = mc406.method406(var945, var665);
					var942 = mc407.method407(var203, var297);
					var543 = mc408.method408(var980, var492);
					var196 = mc409.method409(var627, var396);
					var559 = mc410.method410(var659, var569);
					var723 = mc411.method411(var24, var677);
					var953 = mc412.method412(var870, var461);
					var626 = mc413.method413(var285, var838);
					var216 = mc414.method414(var149, var175);
					var395 = mc415.method415(var296, var855);
					var920 = mc416.method416(var216, var977);
					var28 = mc417.method417(var304, var176);
					var739 = mc418.method418(var801, var644);
					var277 = mc419.method419(var498, var117);
					var167 = mc420.method420(var5, var909);
					var108 = mc421.method421(var437, var487);
					var415 = mc422.method422(var571, var323);
					var781 = mc423.method423(var578, var690);
					var376 = mc424.method424(var311, var967);
					var637 = mc425.method425(var638, var26);
					var557 = mc426.method426(var742, var336);
					var215 = mc427.method427(var697, var509);
					var228 = mc428.method428(var554, var750);
					var460 = mc429.method429(var285, var536);
					var262 = mc430.method430(var381, var546);
					var680 = mc431.method431(var47, var595);
					var461 = mc432.method432(var904, var181);
					var538 = mc433.method433(var756, var269);
					var298 = mc434.method434(var177, var880);
					var761 = mc435.method435(var331, var794);
					var947 = mc436.method436(var185, var50);
					var183 = mc437.method437(var991, var962);
					var855 = mc438.method438(var785, var458);
					var229 = mc439.method439(var60, var842);
					var25 = mc440.method440(var510, var389);
					var785 = mc441.method441(var24, var308);
					var392 = mc442.method442(var199, var430);
					var205 = mc443.method443(var305, var843);
					var78 = mc444.method444(var372, var691);
					var152 = mc445.method445(var223, var735);
					var653 = mc446.method446(var443, var95);
					var521 = mc447.method447(var400, var443);
					var510 = mc448.method448(var751, var992);
					var761 = mc449.method449(var655, var739);
					var202 = mc450.method450(var262, var473);
					var975 = mc451.method451(var232, var912);
					var191 = mc452.method452(var925, var486);
					var727 = mc453.method453(var538, var596);
					var140 = mc454.method454(var504, var89);
					var529 = mc455.method455(var783, var548);
					var451 = mc456.method456(var104, var793);
					var697 = mc457.method457(var573, var321);
					var639 = mc458.method458(var763, var37);
					var413 = mc459.method459(var552, var112);
					var117 = mc460.method460(var466, var712);
					var703 = mc461.method461(var667, var709);
					var703 = mc462.method462(var573, var282);
					var161 = mc463.method463(var369, var674);
					var283 = mc464.method464(var549, var650);
					var397 = mc465.method465(var947, var79);
					var803 = mc466.method466(var747, var714);
					var579 = mc467.method467(var208, var543);
					var621 = mc468.method468(var735, var549);
					var770 = mc469.method469(var308, var523);
					var209 = mc470.method470(var617, var642);
					var324 = mc471.method471(var358, var863);
					var312 = mc472.method472(var591, var854);
					var692 = mc473.method473(var582, var704);
					var745 = mc474.method474(var704, var390);
					var213 = mc475.method475(var488, var30);
					var699 = mc476.method476(var903, var214);
					var266 = mc477.method477(var104, var28);
					var902 = mc478.method478(var499, var824);
					var388 = mc479.method479(var345, var803);
					var397 = mc480.method480(var111, var718);
					var590 = mc481.method481(var579, var665);
					var929 = mc482.method482(var579, var158);
					var436 = mc483.method483(var908, var916);
					var379 = mc484.method484(var899, var533);
					var447 = mc485.method485(var474, var179);
					var640 = mc486.method486(var122, var911);
					var160 = mc487.method487(var425, var505);
					var126 = mc488.method488(var98, var898);
					var51 = mc489.method489(var744, var692);
					var934 = mc490.method490(var732, var432);
					var418 = mc491.method491(var783, var665);
					var324 = mc492.method492(var845, var171);
					var942 = mc493.method493(var738, var308);
					var389 = mc494.method494(var577, var992);
					var53 = mc495.method495(var841, var599);
					var902 = mc496.method496(var803, var600);
					var772 = mc497.method497(var643, var652);
					var868 = mc498.method498(var664, var679);
					var300 = mc499.method499(var807, var913);
					var340 = mc500.method500(var83, var408);
					var215 = mc501.method501(var698, var600);
					var526 = mc502.method502(var802, var907);
					var479 = mc503.method503(var606, var875);
					var277 = mc504.method504(var837, var521);
					var139 = mc505.method505(var516, var772);
					var636 = mc506.method506(var461, var419);
					var598 = mc507.method507(var244, var830);
					var352 = mc508.method508(var851, var323);
					var216 = mc509.method509(var719, var183);
					var139 = mc510.method510(var522, var938);
					var691 = mc511.method511(var336, var863);
					var31 = mc512.method512(var552, var55);
					var852 = mc513.method513(var536, var68);
					var963 = mc514.method514(var440, var384);
					var2 = mc515.method515(var527, var813);
					var300 = mc516.method516(var16, var340);
					var462 = mc517.method517(var580, var624);
					var774 = mc518.method518(var818, var144);
					var717 = mc519.method519(var878, var834);
					var568 = mc520.method520(var973, var746);
					var55 = mc521.method521(var943, var537);
					var912 = mc522.method522(var836, var452);
					var518 = mc523.method523(var611, var703);
					var472 = mc524.method524(var619, var120);
					var956 = mc525.method525(var17, var619);
					var55 = mc526.method526(var534, var706);
					var606 = mc527.method527(var337, var885);
					var614 = mc528.method528(var165, var775);
					var883 = mc529.method529(var747, var799);
					var951 = mc530.method530(var194, var100);
					var536 = mc531.method531(var240, var832);
					var596 = mc532.method532(var343, var319);
					var428 = mc533.method533(var985, var908);
					var757 = mc534.method534(var765, var309);
					var734 = mc535.method535(var124, var695);
					var10 = mc536.method536(var609, var42);
					var261 = mc537.method537(var969, var79);
					var883 = mc538.method538(var373, var873);
					var209 = mc539.method539(var814, var111);
					var941 = mc540.method540(var568, var516);
					var23 = mc541.method541(var533, var703);
					var945 = mc542.method542(var707, var310);
					var386 = mc543.method543(var831, var321);
					var609 = mc544.method544(var445, var492);
					var445 = mc545.method545(var636, var258);
					var1 = mc546.method546(var240, var291);
					var566 = mc547.method547(var68, var636);
					var858 = mc548.method548(var382, var678);
					var76 = mc549.method549(var3, var537);
					var886 = mc550.method550(var889, var33);
					var933 = mc551.method551(var596, var663);
					var298 = mc552.method552(var311, var273);
					var864 = mc553.method553(var97, var866);
					var694 = mc554.method554(var769, var318);
					var475 = mc555.method555(var192, var333);
					var220 = mc556.method556(var501, var695);
					var796 = mc557.method557(var206, var276);
					var224 = mc558.method558(var51, var535);
					var583 = mc559.method559(var427, var870);
					var404 = mc560.method560(var634, var433);
					var445 = mc561.method561(var289, var965);
					var627 = mc562.method562(var839, var127);
					var16 = mc563.method563(var252, var303);
					var37 = mc564.method564(var500, var545);
					var495 = mc565.method565(var359, var792);
					var411 = mc566.method566(var806, var847);
					var95 = mc567.method567(var649, var132);
					var132 = mc568.method568(var252, var455);
					var587 = mc569.method569(var643, var630);
					var36 = mc570.method570(var458, var184);
					var256 = mc571.method571(var612, var794);
					var828 = mc572.method572(var194, var223);
					var822 = mc573.method573(var116, var400);
					var921 = mc574.method574(var413, var654);
					var599 = mc575.method575(var147, var73);
					var144 = mc576.method576(var972, var596);
					var948 = mc577.method577(var939, var797);
					var833 = mc578.method578(var945, var449);
					var176 = mc579.method579(var833, var495);
					var137 = mc580.method580(var432, var616);
					var304 = mc581.method581(var900, var851);
					var381 = mc582.method582(var623, var845);
					var945 = mc583.method583(var347, var286);
					var648 = mc584.method584(var439, var210);
					var898 = mc585.method585(var155, var852);
					var299 = mc586.method586(var187, var802);
					var278 = mc587.method587(var753, var148);
					var493 = mc588.method588(var603, var842);
					var879 = mc589.method589(var356, var760);
					var413 = mc590.method590(var204, var348);
					var277 = mc591.method591(var475, var829);
					var752 = mc592.method592(var482, var203);
					var755 = mc593.method593(var444, var295);
					var774 = mc594.method594(var956, var169);
					var318 = mc595.method595(var194, var790);
					var303 = mc596.method596(var230, var103);
					var570 = mc597.method597(var820, var73);
					var82 = mc598.method598(var932, var84);
					var338 = mc599.method599(var829, var70);
					var98 = mc600.method600(var898, var420);
					var90 = mc601.method601(var501, var52);
					var511 = mc602.method602(var691, var270);
					var892 = mc603.method603(var704, var364);
					var548 = mc604.method604(var69, var84);
					var232 = mc605.method605(var457, var680);
					var66 = mc606.method606(var560, var519);
					var504 = mc607.method607(var50, var285);
					var661 = mc608.method608(var514, var784);
					var257 = mc609.method609(var776, var422);
					var318 = mc610.method610(var61, var211);
					var311 = mc611.method611(var50, var79);
					var746 = mc612.method612(var705, var872);
					var936 = mc613.method613(var860, var513);
					var724 = mc614.method614(var742, var710);
					var599 = mc615.method615(var520, var535);
					var411 = mc616.method616(var567, var148);
					var826 = mc617.method617(var562, var406);
					var752 = mc618.method618(var37, var687);
					var108 = mc619.method619(var40, var421);
					var305 = mc620.method620(var806, var818);
					var333 = mc621.method621(var31, var190);
					var640 = mc622.method622(var805, var358);
					var485 = mc623.method623(var712, var922);
					var268 = mc624.method624(var498, var412);
					var692 = mc625.method625(var941, var643);
					var533 = mc626.method626(var624, var827);
					var675 = mc627.method627(var217, var354);
					var117 = mc628.method628(var511, var242);
					var878 = mc629.method629(var279, var859);
					var438 = mc630.method630(var941, var346);
					var387 = mc631.method631(var224, var937);
					var245 = mc632.method632(var225, var330);
					var18 = mc633.method633(var657, var876);
					var2 = mc634.method634(var787, var740);
					var473 = mc635.method635(var609, var207);
					var288 = mc636.method636(var241, var794);
					var808 = mc637.method637(var828, var180);
					var983 = mc638.method638(var364, var460);
					var431 = mc639.method639(var109, var93);
					var945 = mc640.method640(var415, var474);
					var340 = mc641.method641(var828, var835);
					var920 = mc642.method642(var480, var757);
					var938 = mc643.method643(var83, var734);
					var354 = mc644.method644(var291, var829);
					var18 = mc645.method645(var846, var37);
					var371 = mc646.method646(var753, var50);
					var811 = mc647.method647(var769, var185);
					var914 = mc648.method648(var23, var466);
					var6 = mc649.method649(var559, var388);
					var17 = mc650.method650(var764, var467);
					var392 = mc651.method651(var935, var922);
					var521 = mc652.method652(var710, var958);
					var722 = mc653.method653(var626, var572);
					var836 = mc654.method654(var535, var190);
					var983 = mc655.method655(var39, var642);
					var266 = mc656.method656(var959, var897);
					var453 = mc657.method657(var871, var720);
					var76 = mc658.method658(var181, var947);
					var82 = mc659.method659(var404, var905);
					var313 = mc660.method660(var210, var522);
					var35 = mc661.method661(var312, var510);
					var897 = mc662.method662(var819, var101);
					var846 = mc663.method663(var978, var203);
					var770 = mc664.method664(var411, var855);
					var597 = mc665.method665(var939, var48);
					var109 = mc666.method666(var732, var75);
					var383 = mc667.method667(var601, var654);
					var74 = mc668.method668(var807, var241);
					var432 = mc669.method669(var79, var424);
					var25 = mc670.method670(var702, var609);
					var111 = mc671.method671(var743, var423);
					var801 = mc672.method672(var424, var679);
					var593 = mc673.method673(var43, var990);
					var533 = mc674.method674(var533, var884);
					var513 = mc675.method675(var851, var217);
					var645 = mc676.method676(var269, var939);
					var515 = mc677.method677(var2, var980);
					var879 = mc678.method678(var611, var601);
					var410 = mc679.method679(var291, var888);
					var709 = mc680.method680(var472, var139);
					var508 = mc681.method681(var802, var988);
					var670 = mc682.method682(var237, var527);
					var342 = mc683.method683(var745, var721);
					var403 = mc684.method684(var840, var793);
					var560 = mc685.method685(var381, var621);
					var774 = mc686.method686(var463, var206);
					var831 = mc687.method687(var140, var190);
					var715 = mc688.method688(var552, var562);
					var100 = mc689.method689(var309, var755);
					var752 = mc690.method690(var564, var897);
					var337 = mc691.method691(var847, var321);
					var587 = mc692.method692(var692, var740);
					var323 = mc693.method693(var132, var230);
					var443 = mc694.method694(var182, var62);
					var108 = mc695.method695(var374, var811);
					var264 = mc696.method696(var327, var48);
					var500 = mc697.method697(var101, var536);
					var136 = mc698.method698(var145, var575);
					var801 = mc699.method699(var660, var480);
					var400 = mc700.method700(var977, var497);
					var203 = mc701.method701(var209, var515);
					var278 = mc702.method702(var221, var777);
					var684 = mc703.method703(var185, var569);
					var356 = mc704.method704(var446, var414);
					var706 = mc705.method705(var729, var604);
					var54 = mc706.method706(var569, var977);
					var760 = mc707.method707(var440, var828);
					var354 = mc708.method708(var775, var66);
					var693 = mc709.method709(var128, var332);
					var43 = mc710.method710(var366, var915);
					var55 = mc711.method711(var138, var563);
					var873 = mc712.method712(var996, var767);
					var356 = mc713.method713(var378, var770);
					var756 = mc714.method714(var694, var350);
					var287 = mc715.method715(var59, var272);
					var181 = mc716.method716(var360, var78);
					var881 = mc717.method717(var673, var327);
					var437 = mc718.method718(var34, var62);
					var442 = mc719.method719(var65, var646);
					var642 = mc720.method720(var282, var453);
					var421 = mc721.method721(var305, var414);
					var812 = mc722.method722(var662, var96);
					var126 = mc723.method723(var646, var456);
					var423 = mc724.method724(var694, var491);
					var95 = mc725.method725(var86, var113);
					var390 = mc726.method726(var320, var713);
					var624 = mc727.method727(var628, var481);
					var690 = mc728.method728(var760, var912);
					var493 = mc729.method729(var749, var723);
					var751 = mc730.method730(var210, var899);
					var641 = mc731.method731(var709, var921);
					var347 = mc732.method732(var62, var203);
					var254 = mc733.method733(var201, var946);
					var882 = mc734.method734(var860, var646);
					var454 = mc735.method735(var253, var191);
					var637 = mc736.method736(var676, var121);
					var568 = mc737.method737(var692, var341);
					var895 = mc738.method738(var431, var382);
					var811 = mc739.method739(var711, var384);
					var67 = mc740.method740(var749, var458);
					var841 = mc741.method741(var925, var700);
					var574 = mc742.method742(var562, var47);
					var37 = mc743.method743(var842, var895);
					var475 = mc744.method744(var713, var198);
					var144 = mc745.method745(var931, var287);
					var586 = mc746.method746(var259, var378);
					var101 = mc747.method747(var782, var365);
					var656 = mc748.method748(var1, var752);
					var57 = mc749.method749(var715, var9);
					var347 = mc750.method750(var785, var14);
					var166 = mc751.method751(var412, var305);
					var470 = mc752.method752(var168, var662);
					var502 = mc753.method753(var522, var965);
					var604 = mc754.method754(var377, var298);
					var20 = mc755.method755(var786, var327);
					var684 = mc756.method756(var894, var678);
					var324 = mc757.method757(var96, var702);
					var36 = mc758.method758(var281, var735);
					var292 = mc759.method759(var429, var619);
					var230 = mc760.method760(var406, var899);
					var545 = mc761.method761(var514, var876);
					var290 = mc762.method762(var97, var415);
					var178 = mc763.method763(var358, var604);
					var602 = mc764.method764(var692, var580);
					var53 = mc765.method765(var4, var79);
					var328 = mc766.method766(var375, var265);
					var459 = mc767.method767(var628, var96);
					var778 = mc768.method768(var940, var885);
					var469 = mc769.method769(var651, var535);
					var15 = mc770.method770(var372, var752);
					var483 = mc771.method771(var143, var918);
					var362 = mc772.method772(var912, var797);
					var245 = mc773.method773(var15, var706);
					var998 = mc774.method774(var309, var628);
					var218 = mc775.method775(var695, var999);
					var923 = mc776.method776(var96, var396);
					var265 = mc777.method777(var640, var893);
					var603 = mc778.method778(var857, var653);
					var416 = mc779.method779(var402, var596);
					var513 = mc780.method780(var378, var300);
					var170 = mc781.method781(var162, var652);
					var605 = mc782.method782(var293, var64);
					var361 = mc783.method783(var357, var5);
					var155 = mc784.method784(var232, var978);
					var0 = mc785.method785(var819, var735);
					var492 = mc786.method786(var921, var286);
					var469 = mc787.method787(var67, var54);
					var21 = mc788.method788(var637, var72);
					var582 = mc789.method789(var320, var491);
					var312 = mc790.method790(var69, var847);
					var435 = mc791.method791(var887, var10);
					var550 = mc792.method792(var766, var330);
					var627 = mc793.method793(var399, var960);
					var507 = mc794.method794(var2, var809);
					var927 = mc795.method795(var198, var585);
					var871 = mc796.method796(var965, var785);
					var71 = mc797.method797(var95, var911);
					var283 = mc798.method798(var444, var530);
					var864 = mc799.method799(var282, var152);
					var101 = mc800.method800(var838, var963);
					var436 = mc801.method801(var961, var396);
					var497 = mc802.method802(var153, var422);
					var51 = mc803.method803(var801, var233);
					var863 = mc804.method804(var526, var136);
					var215 = mc805.method805(var373, var155);
					var770 = mc806.method806(var577, var106);
					var207 = mc807.method807(var300, var168);
					var390 = mc808.method808(var474, var105);
					var410 = mc809.method809(var474, var490);
					var512 = mc810.method810(var397, var344);
					var278 = mc811.method811(var826, var166);
					var97 = mc812.method812(var980, var866);
					var594 = mc813.method813(var553, var42);
					var814 = mc814.method814(var294, var859);
					var577 = mc815.method815(var771, var927);
					var520 = mc816.method816(var809, var338);
					var120 = mc817.method817(var390, var791);
					var640 = mc818.method818(var703, var494);
					var684 = mc819.method819(var610, var795);
					var299 = mc820.method820(var517, var287);
					var827 = mc821.method821(var498, var758);
					var418 = mc822.method822(var569, var667);
					var558 = mc823.method823(var443, var446);
					var635 = mc824.method824(var961, var238);
					var986 = mc825.method825(var817, var508);
					var528 = mc826.method826(var686, var980);
					var421 = mc827.method827(var800, var678);
					var191 = mc828.method828(var995, var109);
					var516 = mc829.method829(var780, var68);
					var748 = mc830.method830(var527, var313);
					var308 = mc831.method831(var150, var106);
					var407 = mc832.method832(var853, var56);
					var872 = mc833.method833(var760, var262);
					var398 = mc834.method834(var834, var829);
					var917 = mc835.method835(var319, var711);
					var600 = mc836.method836(var644, var594);
					var977 = mc837.method837(var903, var541);
					var47 = mc838.method838(var771, var203);
					var979 = mc839.method839(var677, var391);
					var350 = mc840.method840(var564, var512);
					var795 = mc841.method841(var797, var181);
					var47 = mc842.method842(var800, var131);
					var320 = mc843.method843(var666, var105);
					var908 = mc844.method844(var883, var92);
					var2 = mc845.method845(var517, var258);
					var631 = mc846.method846(var419, var791);
					var130 = mc847.method847(var838, var389);
					var717 = mc848.method848(var183, var15);
					var518 = mc849.method849(var510, var969);
					var59 = mc850.method850(var607, var721);
					var735 = mc851.method851(var205, var654);
					var354 = mc852.method852(var515, var742);
					var827 = mc853.method853(var399, var61);
					var79 = mc854.method854(var180, var853);
					var463 = mc855.method855(var138, var514);
					var823 = mc856.method856(var863, var756);
					var685 = mc857.method857(var468, var708);
					var332 = mc858.method858(var742, var843);
					var776 = mc859.method859(var589, var142);
					var826 = mc860.method860(var531, var57);
					var304 = mc861.method861(var921, var605);
					var25 = mc862.method862(var727, var29);
					var628 = mc863.method863(var863, var3);
					var434 = mc864.method864(var767, var663);
					var105 = mc865.method865(var661, var130);
					var56 = mc866.method866(var975, var340);
					var238 = mc867.method867(var928, var367);
					var142 = mc868.method868(var527, var550);
					var416 = mc869.method869(var814, var600);
					var629 = mc870.method870(var627, var485);
					var114 = mc871.method871(var963, var395);
					var626 = mc872.method872(var311, var517);
					var748 = mc873.method873(var801, var8);
					var457 = mc874.method874(var848, var523);
					var517 = mc875.method875(var757, var101);
					var566 = mc876.method876(var804, var192);
					var426 = mc877.method877(var962, var989);
					var513 = mc878.method878(var199, var46);
					var943 = mc879.method879(var341, var525);
					var979 = mc880.method880(var713, var209);
					var281 = mc881.method881(var826, var855);
					var545 = mc882.method882(var586, var244);
					var145 = mc883.method883(var347, var560);
					var564 = mc884.method884(var251, var782);
					var914 = mc885.method885(var275, var939);
					var854 = mc886.method886(var942, var481);
					var606 = mc887.method887(var75, var288);
					var620 = mc888.method888(var915, var417);
					var204 = mc889.method889(var287, var108);
					var850 = mc890.method890(var40, var67);
					var372 = mc891.method891(var400, var241);
					var443 = mc892.method892(var205, var66);
					var609 = mc893.method893(var242, var818);
					var246 = mc894.method894(var623, var323);
					var585 = mc895.method895(var350, var673);
					var368 = mc896.method896(var598, var782);
					var841 = mc897.method897(var656, var232);
					var675 = mc898.method898(var939, var646);
					var836 = mc899.method899(var881, var585);
					var411 = mc900.method900(var340, var519);
					var634 = mc901.method901(var902, var529);
					var313 = mc902.method902(var940, var928);
					var196 = mc903.method903(var563, var601);
					var570 = mc904.method904(var256, var160);
					var639 = mc905.method905(var950, var843);
					var613 = mc906.method906(var681, var268);
					var581 = mc907.method907(var322, var701);
					var696 = mc908.method908(var509, var520);
					var432 = mc909.method909(var469, var870);
					var927 = mc910.method910(var502, var277);
					var869 = mc911.method911(var45, var640);
					var247 = mc912.method912(var990, var463);
					var684 = mc913.method913(var634, var506);
					var60 = mc914.method914(var686, var329);
					var201 = mc915.method915(var519, var954);
					var535 = mc916.method916(var165, var419);
					var213 = mc917.method917(var366, var10);
					var658 = mc918.method918(var908, var62);
					var650 = mc919.method919(var589, var262);
					var282 = mc920.method920(var323, var477);
					var243 = mc921.method921(var562, var57);
					var541 = mc922.method922(var884, var927);
					var959 = mc923.method923(var119, var996);
					var307 = mc924.method924(var448, var262);
					var55 = mc925.method925(var214, var312);
					var43 = mc926.method926(var788, var447);
					var869 = mc927.method927(var843, var207);
					var588 = mc928.method928(var603, var25);
					var34 = mc929.method929(var306, var988);
					var99 = mc930.method930(var363, var63);
					var504 = mc931.method931(var564, var637);
					var199 = mc932.method932(var612, var423);
					var273 = mc933.method933(var986, var207);
					var910 = mc934.method934(var747, var718);
					var549 = mc935.method935(var370, var159);
					var422 = mc936.method936(var54, var883);
					var28 = mc937.method937(var343, var600);
					var163 = mc938.method938(var918, var260);
					var972 = mc939.method939(var678, var924);
					var857 = mc940.method940(var928, var460);
					var653 = mc941.method941(var972, var912);
					var86 = mc942.method942(var559, var730);
					var712 = mc943.method943(var684, var55);
					var889 = mc944.method944(var989, var904);
					var418 = mc945.method945(var525, var242);
					var43 = mc946.method946(var669, var343);
					var52 = mc947.method947(var355, var382);
					var130 = mc948.method948(var170, var902);
					var410 = mc949.method949(var531, var652);
					var551 = mc950.method950(var639, var882);
					var711 = mc951.method951(var589, var931);
					var317 = mc952.method952(var921, var842);
					var299 = mc953.method953(var24, var128);
					var115 = mc954.method954(var752, var710);
					var830 = mc955.method955(var672, var961);
					var470 = mc956.method956(var993, var15);
					var850 = mc957.method957(var440, var26);
					var372 = mc958.method958(var218, var46);
					var749 = mc959.method959(var63, var40);
					var595 = mc960.method960(var164, var716);
					var430 = mc961.method961(var688, var556);
					var939 = mc962.method962(var137, var278);
					var74 = mc963.method963(var745, var563);
					var772 = mc964.method964(var50, var653);
					var493 = mc965.method965(var611, var329);
					var9 = mc966.method966(var912, var123);
					var153 = mc967.method967(var346, var491);
					var56 = mc968.method968(var387, var922);
					var451 = mc969.method969(var22, var32);
					var992 = mc970.method970(var179, var643);
					var768 = mc971.method971(var553, var460);
					var355 = mc972.method972(var65, var98);
					var217 = mc973.method973(var218, var343);
					var951 = mc974.method974(var558, var870);
					var862 = mc975.method975(var503, var949);
					var969 = mc976.method976(var985, var296);
					var595 = mc977.method977(var726, var31);
					var48 = mc978.method978(var415, var521);
					var947 = mc979.method979(var877, var227);
					var200 = mc980.method980(var873, var38);
					var60 = mc981.method981(var855, var402);
					var611 = mc982.method982(var214, var179);
					var791 = mc983.method983(var989, var662);
					var359 = mc984.method984(var24, var616);
					var528 = mc985.method985(var427, var314);
					var442 = mc986.method986(var640, var865);
					var974 = mc987.method987(var573, var795);
					var831 = mc988.method988(var119, var140);
					var582 = mc989.method989(var913, var150);
					var849 = mc990.method990(var141, var885);
					var306 = mc991.method991(var228, var556);
					var984 = mc992.method992(var927, var942);
					var341 = mc993.method993(var434, var189);
					var507 = mc994.method994(var328, var120);
					var321 = mc995.method995(var21, var930);
					var200 = mc996.method996(var929, var275);
					var939 = mc997.method997(var34, var726);
					var378 = mc998.method998(var59, var987);
					var732 = mc999.method999(var832, var298);
					#endregion
				}
                QueryPerformanceCounter(ref tsc_after);
                if (cnt == 3)
                    time2 = tsc_after - tsc_before - min;
                if (cnt == 4)
                    time3 = tsc_after - tsc_before - min;
                if (cnt == 5)
                    time4 = tsc_after - tsc_before - min;
                sum++;
            }


			#region write_results
			tempfile.WriteLine("var"+0+ "=" + var0);
			tempfile.WriteLine("var"+1+ "=" + var1);
			tempfile.WriteLine("var"+2+ "=" + var2);
			tempfile.WriteLine("var"+3+ "=" + var3);
			tempfile.WriteLine("var"+4+ "=" + var4);
			tempfile.WriteLine("var"+5+ "=" + var5);
			tempfile.WriteLine("var"+6+ "=" + var6);
			tempfile.WriteLine("var"+7+ "=" + var7);
			tempfile.WriteLine("var"+8+ "=" + var8);
			tempfile.WriteLine("var"+9+ "=" + var9);
			tempfile.WriteLine("var"+10+ "=" + var10);
			tempfile.WriteLine("var"+11+ "=" + var11);
			tempfile.WriteLine("var"+12+ "=" + var12);
			tempfile.WriteLine("var"+13+ "=" + var13);
			tempfile.WriteLine("var"+14+ "=" + var14);
			tempfile.WriteLine("var"+15+ "=" + var15);
			tempfile.WriteLine("var"+16+ "=" + var16);
			tempfile.WriteLine("var"+17+ "=" + var17);
			tempfile.WriteLine("var"+18+ "=" + var18);
			tempfile.WriteLine("var"+19+ "=" + var19);
			tempfile.WriteLine("var"+20+ "=" + var20);
			tempfile.WriteLine("var"+21+ "=" + var21);
			tempfile.WriteLine("var"+22+ "=" + var22);
			tempfile.WriteLine("var"+23+ "=" + var23);
			tempfile.WriteLine("var"+24+ "=" + var24);
			tempfile.WriteLine("var"+25+ "=" + var25);
			tempfile.WriteLine("var"+26+ "=" + var26);
			tempfile.WriteLine("var"+27+ "=" + var27);
			tempfile.WriteLine("var"+28+ "=" + var28);
			tempfile.WriteLine("var"+29+ "=" + var29);
			tempfile.WriteLine("var"+30+ "=" + var30);
			tempfile.WriteLine("var"+31+ "=" + var31);
			tempfile.WriteLine("var"+32+ "=" + var32);
			tempfile.WriteLine("var"+33+ "=" + var33);
			tempfile.WriteLine("var"+34+ "=" + var34);
			tempfile.WriteLine("var"+35+ "=" + var35);
			tempfile.WriteLine("var"+36+ "=" + var36);
			tempfile.WriteLine("var"+37+ "=" + var37);
			tempfile.WriteLine("var"+38+ "=" + var38);
			tempfile.WriteLine("var"+39+ "=" + var39);
			tempfile.WriteLine("var"+40+ "=" + var40);
			tempfile.WriteLine("var"+41+ "=" + var41);
			tempfile.WriteLine("var"+42+ "=" + var42);
			tempfile.WriteLine("var"+43+ "=" + var43);
			tempfile.WriteLine("var"+44+ "=" + var44);
			tempfile.WriteLine("var"+45+ "=" + var45);
			tempfile.WriteLine("var"+46+ "=" + var46);
			tempfile.WriteLine("var"+47+ "=" + var47);
			tempfile.WriteLine("var"+48+ "=" + var48);
			tempfile.WriteLine("var"+49+ "=" + var49);
			tempfile.WriteLine("var"+50+ "=" + var50);
			tempfile.WriteLine("var"+51+ "=" + var51);
			tempfile.WriteLine("var"+52+ "=" + var52);
			tempfile.WriteLine("var"+53+ "=" + var53);
			tempfile.WriteLine("var"+54+ "=" + var54);
			tempfile.WriteLine("var"+55+ "=" + var55);
			tempfile.WriteLine("var"+56+ "=" + var56);
			tempfile.WriteLine("var"+57+ "=" + var57);
			tempfile.WriteLine("var"+58+ "=" + var58);
			tempfile.WriteLine("var"+59+ "=" + var59);
			tempfile.WriteLine("var"+60+ "=" + var60);
			tempfile.WriteLine("var"+61+ "=" + var61);
			tempfile.WriteLine("var"+62+ "=" + var62);
			tempfile.WriteLine("var"+63+ "=" + var63);
			tempfile.WriteLine("var"+64+ "=" + var64);
			tempfile.WriteLine("var"+65+ "=" + var65);
			tempfile.WriteLine("var"+66+ "=" + var66);
			tempfile.WriteLine("var"+67+ "=" + var67);
			tempfile.WriteLine("var"+68+ "=" + var68);
			tempfile.WriteLine("var"+69+ "=" + var69);
			tempfile.WriteLine("var"+70+ "=" + var70);
			tempfile.WriteLine("var"+71+ "=" + var71);
			tempfile.WriteLine("var"+72+ "=" + var72);
			tempfile.WriteLine("var"+73+ "=" + var73);
			tempfile.WriteLine("var"+74+ "=" + var74);
			tempfile.WriteLine("var"+75+ "=" + var75);
			tempfile.WriteLine("var"+76+ "=" + var76);
			tempfile.WriteLine("var"+77+ "=" + var77);
			tempfile.WriteLine("var"+78+ "=" + var78);
			tempfile.WriteLine("var"+79+ "=" + var79);
			tempfile.WriteLine("var"+80+ "=" + var80);
			tempfile.WriteLine("var"+81+ "=" + var81);
			tempfile.WriteLine("var"+82+ "=" + var82);
			tempfile.WriteLine("var"+83+ "=" + var83);
			tempfile.WriteLine("var"+84+ "=" + var84);
			tempfile.WriteLine("var"+85+ "=" + var85);
			tempfile.WriteLine("var"+86+ "=" + var86);
			tempfile.WriteLine("var"+87+ "=" + var87);
			tempfile.WriteLine("var"+88+ "=" + var88);
			tempfile.WriteLine("var"+89+ "=" + var89);
			tempfile.WriteLine("var"+90+ "=" + var90);
			tempfile.WriteLine("var"+91+ "=" + var91);
			tempfile.WriteLine("var"+92+ "=" + var92);
			tempfile.WriteLine("var"+93+ "=" + var93);
			tempfile.WriteLine("var"+94+ "=" + var94);
			tempfile.WriteLine("var"+95+ "=" + var95);
			tempfile.WriteLine("var"+96+ "=" + var96);
			tempfile.WriteLine("var"+97+ "=" + var97);
			tempfile.WriteLine("var"+98+ "=" + var98);
			tempfile.WriteLine("var"+99+ "=" + var99);
			tempfile.WriteLine("var"+100+ "=" + var100);
			tempfile.WriteLine("var"+101+ "=" + var101);
			tempfile.WriteLine("var"+102+ "=" + var102);
			tempfile.WriteLine("var"+103+ "=" + var103);
			tempfile.WriteLine("var"+104+ "=" + var104);
			tempfile.WriteLine("var"+105+ "=" + var105);
			tempfile.WriteLine("var"+106+ "=" + var106);
			tempfile.WriteLine("var"+107+ "=" + var107);
			tempfile.WriteLine("var"+108+ "=" + var108);
			tempfile.WriteLine("var"+109+ "=" + var109);
			tempfile.WriteLine("var"+110+ "=" + var110);
			tempfile.WriteLine("var"+111+ "=" + var111);
			tempfile.WriteLine("var"+112+ "=" + var112);
			tempfile.WriteLine("var"+113+ "=" + var113);
			tempfile.WriteLine("var"+114+ "=" + var114);
			tempfile.WriteLine("var"+115+ "=" + var115);
			tempfile.WriteLine("var"+116+ "=" + var116);
			tempfile.WriteLine("var"+117+ "=" + var117);
			tempfile.WriteLine("var"+118+ "=" + var118);
			tempfile.WriteLine("var"+119+ "=" + var119);
			tempfile.WriteLine("var"+120+ "=" + var120);
			tempfile.WriteLine("var"+121+ "=" + var121);
			tempfile.WriteLine("var"+122+ "=" + var122);
			tempfile.WriteLine("var"+123+ "=" + var123);
			tempfile.WriteLine("var"+124+ "=" + var124);
			tempfile.WriteLine("var"+125+ "=" + var125);
			tempfile.WriteLine("var"+126+ "=" + var126);
			tempfile.WriteLine("var"+127+ "=" + var127);
			tempfile.WriteLine("var"+128+ "=" + var128);
			tempfile.WriteLine("var"+129+ "=" + var129);
			tempfile.WriteLine("var"+130+ "=" + var130);
			tempfile.WriteLine("var"+131+ "=" + var131);
			tempfile.WriteLine("var"+132+ "=" + var132);
			tempfile.WriteLine("var"+133+ "=" + var133);
			tempfile.WriteLine("var"+134+ "=" + var134);
			tempfile.WriteLine("var"+135+ "=" + var135);
			tempfile.WriteLine("var"+136+ "=" + var136);
			tempfile.WriteLine("var"+137+ "=" + var137);
			tempfile.WriteLine("var"+138+ "=" + var138);
			tempfile.WriteLine("var"+139+ "=" + var139);
			tempfile.WriteLine("var"+140+ "=" + var140);
			tempfile.WriteLine("var"+141+ "=" + var141);
			tempfile.WriteLine("var"+142+ "=" + var142);
			tempfile.WriteLine("var"+143+ "=" + var143);
			tempfile.WriteLine("var"+144+ "=" + var144);
			tempfile.WriteLine("var"+145+ "=" + var145);
			tempfile.WriteLine("var"+146+ "=" + var146);
			tempfile.WriteLine("var"+147+ "=" + var147);
			tempfile.WriteLine("var"+148+ "=" + var148);
			tempfile.WriteLine("var"+149+ "=" + var149);
			tempfile.WriteLine("var"+150+ "=" + var150);
			tempfile.WriteLine("var"+151+ "=" + var151);
			tempfile.WriteLine("var"+152+ "=" + var152);
			tempfile.WriteLine("var"+153+ "=" + var153);
			tempfile.WriteLine("var"+154+ "=" + var154);
			tempfile.WriteLine("var"+155+ "=" + var155);
			tempfile.WriteLine("var"+156+ "=" + var156);
			tempfile.WriteLine("var"+157+ "=" + var157);
			tempfile.WriteLine("var"+158+ "=" + var158);
			tempfile.WriteLine("var"+159+ "=" + var159);
			tempfile.WriteLine("var"+160+ "=" + var160);
			tempfile.WriteLine("var"+161+ "=" + var161);
			tempfile.WriteLine("var"+162+ "=" + var162);
			tempfile.WriteLine("var"+163+ "=" + var163);
			tempfile.WriteLine("var"+164+ "=" + var164);
			tempfile.WriteLine("var"+165+ "=" + var165);
			tempfile.WriteLine("var"+166+ "=" + var166);
			tempfile.WriteLine("var"+167+ "=" + var167);
			tempfile.WriteLine("var"+168+ "=" + var168);
			tempfile.WriteLine("var"+169+ "=" + var169);
			tempfile.WriteLine("var"+170+ "=" + var170);
			tempfile.WriteLine("var"+171+ "=" + var171);
			tempfile.WriteLine("var"+172+ "=" + var172);
			tempfile.WriteLine("var"+173+ "=" + var173);
			tempfile.WriteLine("var"+174+ "=" + var174);
			tempfile.WriteLine("var"+175+ "=" + var175);
			tempfile.WriteLine("var"+176+ "=" + var176);
			tempfile.WriteLine("var"+177+ "=" + var177);
			tempfile.WriteLine("var"+178+ "=" + var178);
			tempfile.WriteLine("var"+179+ "=" + var179);
			tempfile.WriteLine("var"+180+ "=" + var180);
			tempfile.WriteLine("var"+181+ "=" + var181);
			tempfile.WriteLine("var"+182+ "=" + var182);
			tempfile.WriteLine("var"+183+ "=" + var183);
			tempfile.WriteLine("var"+184+ "=" + var184);
			tempfile.WriteLine("var"+185+ "=" + var185);
			tempfile.WriteLine("var"+186+ "=" + var186);
			tempfile.WriteLine("var"+187+ "=" + var187);
			tempfile.WriteLine("var"+188+ "=" + var188);
			tempfile.WriteLine("var"+189+ "=" + var189);
			tempfile.WriteLine("var"+190+ "=" + var190);
			tempfile.WriteLine("var"+191+ "=" + var191);
			tempfile.WriteLine("var"+192+ "=" + var192);
			tempfile.WriteLine("var"+193+ "=" + var193);
			tempfile.WriteLine("var"+194+ "=" + var194);
			tempfile.WriteLine("var"+195+ "=" + var195);
			tempfile.WriteLine("var"+196+ "=" + var196);
			tempfile.WriteLine("var"+197+ "=" + var197);
			tempfile.WriteLine("var"+198+ "=" + var198);
			tempfile.WriteLine("var"+199+ "=" + var199);
			tempfile.WriteLine("var"+200+ "=" + var200);
			tempfile.WriteLine("var"+201+ "=" + var201);
			tempfile.WriteLine("var"+202+ "=" + var202);
			tempfile.WriteLine("var"+203+ "=" + var203);
			tempfile.WriteLine("var"+204+ "=" + var204);
			tempfile.WriteLine("var"+205+ "=" + var205);
			tempfile.WriteLine("var"+206+ "=" + var206);
			tempfile.WriteLine("var"+207+ "=" + var207);
			tempfile.WriteLine("var"+208+ "=" + var208);
			tempfile.WriteLine("var"+209+ "=" + var209);
			tempfile.WriteLine("var"+210+ "=" + var210);
			tempfile.WriteLine("var"+211+ "=" + var211);
			tempfile.WriteLine("var"+212+ "=" + var212);
			tempfile.WriteLine("var"+213+ "=" + var213);
			tempfile.WriteLine("var"+214+ "=" + var214);
			tempfile.WriteLine("var"+215+ "=" + var215);
			tempfile.WriteLine("var"+216+ "=" + var216);
			tempfile.WriteLine("var"+217+ "=" + var217);
			tempfile.WriteLine("var"+218+ "=" + var218);
			tempfile.WriteLine("var"+219+ "=" + var219);
			tempfile.WriteLine("var"+220+ "=" + var220);
			tempfile.WriteLine("var"+221+ "=" + var221);
			tempfile.WriteLine("var"+222+ "=" + var222);
			tempfile.WriteLine("var"+223+ "=" + var223);
			tempfile.WriteLine("var"+224+ "=" + var224);
			tempfile.WriteLine("var"+225+ "=" + var225);
			tempfile.WriteLine("var"+226+ "=" + var226);
			tempfile.WriteLine("var"+227+ "=" + var227);
			tempfile.WriteLine("var"+228+ "=" + var228);
			tempfile.WriteLine("var"+229+ "=" + var229);
			tempfile.WriteLine("var"+230+ "=" + var230);
			tempfile.WriteLine("var"+231+ "=" + var231);
			tempfile.WriteLine("var"+232+ "=" + var232);
			tempfile.WriteLine("var"+233+ "=" + var233);
			tempfile.WriteLine("var"+234+ "=" + var234);
			tempfile.WriteLine("var"+235+ "=" + var235);
			tempfile.WriteLine("var"+236+ "=" + var236);
			tempfile.WriteLine("var"+237+ "=" + var237);
			tempfile.WriteLine("var"+238+ "=" + var238);
			tempfile.WriteLine("var"+239+ "=" + var239);
			tempfile.WriteLine("var"+240+ "=" + var240);
			tempfile.WriteLine("var"+241+ "=" + var241);
			tempfile.WriteLine("var"+242+ "=" + var242);
			tempfile.WriteLine("var"+243+ "=" + var243);
			tempfile.WriteLine("var"+244+ "=" + var244);
			tempfile.WriteLine("var"+245+ "=" + var245);
			tempfile.WriteLine("var"+246+ "=" + var246);
			tempfile.WriteLine("var"+247+ "=" + var247);
			tempfile.WriteLine("var"+248+ "=" + var248);
			tempfile.WriteLine("var"+249+ "=" + var249);
			tempfile.WriteLine("var"+250+ "=" + var250);
			tempfile.WriteLine("var"+251+ "=" + var251);
			tempfile.WriteLine("var"+252+ "=" + var252);
			tempfile.WriteLine("var"+253+ "=" + var253);
			tempfile.WriteLine("var"+254+ "=" + var254);
			tempfile.WriteLine("var"+255+ "=" + var255);
			tempfile.WriteLine("var"+256+ "=" + var256);
			tempfile.WriteLine("var"+257+ "=" + var257);
			tempfile.WriteLine("var"+258+ "=" + var258);
			tempfile.WriteLine("var"+259+ "=" + var259);
			tempfile.WriteLine("var"+260+ "=" + var260);
			tempfile.WriteLine("var"+261+ "=" + var261);
			tempfile.WriteLine("var"+262+ "=" + var262);
			tempfile.WriteLine("var"+263+ "=" + var263);
			tempfile.WriteLine("var"+264+ "=" + var264);
			tempfile.WriteLine("var"+265+ "=" + var265);
			tempfile.WriteLine("var"+266+ "=" + var266);
			tempfile.WriteLine("var"+267+ "=" + var267);
			tempfile.WriteLine("var"+268+ "=" + var268);
			tempfile.WriteLine("var"+269+ "=" + var269);
			tempfile.WriteLine("var"+270+ "=" + var270);
			tempfile.WriteLine("var"+271+ "=" + var271);
			tempfile.WriteLine("var"+272+ "=" + var272);
			tempfile.WriteLine("var"+273+ "=" + var273);
			tempfile.WriteLine("var"+274+ "=" + var274);
			tempfile.WriteLine("var"+275+ "=" + var275);
			tempfile.WriteLine("var"+276+ "=" + var276);
			tempfile.WriteLine("var"+277+ "=" + var277);
			tempfile.WriteLine("var"+278+ "=" + var278);
			tempfile.WriteLine("var"+279+ "=" + var279);
			tempfile.WriteLine("var"+280+ "=" + var280);
			tempfile.WriteLine("var"+281+ "=" + var281);
			tempfile.WriteLine("var"+282+ "=" + var282);
			tempfile.WriteLine("var"+283+ "=" + var283);
			tempfile.WriteLine("var"+284+ "=" + var284);
			tempfile.WriteLine("var"+285+ "=" + var285);
			tempfile.WriteLine("var"+286+ "=" + var286);
			tempfile.WriteLine("var"+287+ "=" + var287);
			tempfile.WriteLine("var"+288+ "=" + var288);
			tempfile.WriteLine("var"+289+ "=" + var289);
			tempfile.WriteLine("var"+290+ "=" + var290);
			tempfile.WriteLine("var"+291+ "=" + var291);
			tempfile.WriteLine("var"+292+ "=" + var292);
			tempfile.WriteLine("var"+293+ "=" + var293);
			tempfile.WriteLine("var"+294+ "=" + var294);
			tempfile.WriteLine("var"+295+ "=" + var295);
			tempfile.WriteLine("var"+296+ "=" + var296);
			tempfile.WriteLine("var"+297+ "=" + var297);
			tempfile.WriteLine("var"+298+ "=" + var298);
			tempfile.WriteLine("var"+299+ "=" + var299);
			tempfile.WriteLine("var"+300+ "=" + var300);
			tempfile.WriteLine("var"+301+ "=" + var301);
			tempfile.WriteLine("var"+302+ "=" + var302);
			tempfile.WriteLine("var"+303+ "=" + var303);
			tempfile.WriteLine("var"+304+ "=" + var304);
			tempfile.WriteLine("var"+305+ "=" + var305);
			tempfile.WriteLine("var"+306+ "=" + var306);
			tempfile.WriteLine("var"+307+ "=" + var307);
			tempfile.WriteLine("var"+308+ "=" + var308);
			tempfile.WriteLine("var"+309+ "=" + var309);
			tempfile.WriteLine("var"+310+ "=" + var310);
			tempfile.WriteLine("var"+311+ "=" + var311);
			tempfile.WriteLine("var"+312+ "=" + var312);
			tempfile.WriteLine("var"+313+ "=" + var313);
			tempfile.WriteLine("var"+314+ "=" + var314);
			tempfile.WriteLine("var"+315+ "=" + var315);
			tempfile.WriteLine("var"+316+ "=" + var316);
			tempfile.WriteLine("var"+317+ "=" + var317);
			tempfile.WriteLine("var"+318+ "=" + var318);
			tempfile.WriteLine("var"+319+ "=" + var319);
			tempfile.WriteLine("var"+320+ "=" + var320);
			tempfile.WriteLine("var"+321+ "=" + var321);
			tempfile.WriteLine("var"+322+ "=" + var322);
			tempfile.WriteLine("var"+323+ "=" + var323);
			tempfile.WriteLine("var"+324+ "=" + var324);
			tempfile.WriteLine("var"+325+ "=" + var325);
			tempfile.WriteLine("var"+326+ "=" + var326);
			tempfile.WriteLine("var"+327+ "=" + var327);
			tempfile.WriteLine("var"+328+ "=" + var328);
			tempfile.WriteLine("var"+329+ "=" + var329);
			tempfile.WriteLine("var"+330+ "=" + var330);
			tempfile.WriteLine("var"+331+ "=" + var331);
			tempfile.WriteLine("var"+332+ "=" + var332);
			tempfile.WriteLine("var"+333+ "=" + var333);
			tempfile.WriteLine("var"+334+ "=" + var334);
			tempfile.WriteLine("var"+335+ "=" + var335);
			tempfile.WriteLine("var"+336+ "=" + var336);
			tempfile.WriteLine("var"+337+ "=" + var337);
			tempfile.WriteLine("var"+338+ "=" + var338);
			tempfile.WriteLine("var"+339+ "=" + var339);
			tempfile.WriteLine("var"+340+ "=" + var340);
			tempfile.WriteLine("var"+341+ "=" + var341);
			tempfile.WriteLine("var"+342+ "=" + var342);
			tempfile.WriteLine("var"+343+ "=" + var343);
			tempfile.WriteLine("var"+344+ "=" + var344);
			tempfile.WriteLine("var"+345+ "=" + var345);
			tempfile.WriteLine("var"+346+ "=" + var346);
			tempfile.WriteLine("var"+347+ "=" + var347);
			tempfile.WriteLine("var"+348+ "=" + var348);
			tempfile.WriteLine("var"+349+ "=" + var349);
			tempfile.WriteLine("var"+350+ "=" + var350);
			tempfile.WriteLine("var"+351+ "=" + var351);
			tempfile.WriteLine("var"+352+ "=" + var352);
			tempfile.WriteLine("var"+353+ "=" + var353);
			tempfile.WriteLine("var"+354+ "=" + var354);
			tempfile.WriteLine("var"+355+ "=" + var355);
			tempfile.WriteLine("var"+356+ "=" + var356);
			tempfile.WriteLine("var"+357+ "=" + var357);
			tempfile.WriteLine("var"+358+ "=" + var358);
			tempfile.WriteLine("var"+359+ "=" + var359);
			tempfile.WriteLine("var"+360+ "=" + var360);
			tempfile.WriteLine("var"+361+ "=" + var361);
			tempfile.WriteLine("var"+362+ "=" + var362);
			tempfile.WriteLine("var"+363+ "=" + var363);
			tempfile.WriteLine("var"+364+ "=" + var364);
			tempfile.WriteLine("var"+365+ "=" + var365);
			tempfile.WriteLine("var"+366+ "=" + var366);
			tempfile.WriteLine("var"+367+ "=" + var367);
			tempfile.WriteLine("var"+368+ "=" + var368);
			tempfile.WriteLine("var"+369+ "=" + var369);
			tempfile.WriteLine("var"+370+ "=" + var370);
			tempfile.WriteLine("var"+371+ "=" + var371);
			tempfile.WriteLine("var"+372+ "=" + var372);
			tempfile.WriteLine("var"+373+ "=" + var373);
			tempfile.WriteLine("var"+374+ "=" + var374);
			tempfile.WriteLine("var"+375+ "=" + var375);
			tempfile.WriteLine("var"+376+ "=" + var376);
			tempfile.WriteLine("var"+377+ "=" + var377);
			tempfile.WriteLine("var"+378+ "=" + var378);
			tempfile.WriteLine("var"+379+ "=" + var379);
			tempfile.WriteLine("var"+380+ "=" + var380);
			tempfile.WriteLine("var"+381+ "=" + var381);
			tempfile.WriteLine("var"+382+ "=" + var382);
			tempfile.WriteLine("var"+383+ "=" + var383);
			tempfile.WriteLine("var"+384+ "=" + var384);
			tempfile.WriteLine("var"+385+ "=" + var385);
			tempfile.WriteLine("var"+386+ "=" + var386);
			tempfile.WriteLine("var"+387+ "=" + var387);
			tempfile.WriteLine("var"+388+ "=" + var388);
			tempfile.WriteLine("var"+389+ "=" + var389);
			tempfile.WriteLine("var"+390+ "=" + var390);
			tempfile.WriteLine("var"+391+ "=" + var391);
			tempfile.WriteLine("var"+392+ "=" + var392);
			tempfile.WriteLine("var"+393+ "=" + var393);
			tempfile.WriteLine("var"+394+ "=" + var394);
			tempfile.WriteLine("var"+395+ "=" + var395);
			tempfile.WriteLine("var"+396+ "=" + var396);
			tempfile.WriteLine("var"+397+ "=" + var397);
			tempfile.WriteLine("var"+398+ "=" + var398);
			tempfile.WriteLine("var"+399+ "=" + var399);
			tempfile.WriteLine("var"+400+ "=" + var400);
			tempfile.WriteLine("var"+401+ "=" + var401);
			tempfile.WriteLine("var"+402+ "=" + var402);
			tempfile.WriteLine("var"+403+ "=" + var403);
			tempfile.WriteLine("var"+404+ "=" + var404);
			tempfile.WriteLine("var"+405+ "=" + var405);
			tempfile.WriteLine("var"+406+ "=" + var406);
			tempfile.WriteLine("var"+407+ "=" + var407);
			tempfile.WriteLine("var"+408+ "=" + var408);
			tempfile.WriteLine("var"+409+ "=" + var409);
			tempfile.WriteLine("var"+410+ "=" + var410);
			tempfile.WriteLine("var"+411+ "=" + var411);
			tempfile.WriteLine("var"+412+ "=" + var412);
			tempfile.WriteLine("var"+413+ "=" + var413);
			tempfile.WriteLine("var"+414+ "=" + var414);
			tempfile.WriteLine("var"+415+ "=" + var415);
			tempfile.WriteLine("var"+416+ "=" + var416);
			tempfile.WriteLine("var"+417+ "=" + var417);
			tempfile.WriteLine("var"+418+ "=" + var418);
			tempfile.WriteLine("var"+419+ "=" + var419);
			tempfile.WriteLine("var"+420+ "=" + var420);
			tempfile.WriteLine("var"+421+ "=" + var421);
			tempfile.WriteLine("var"+422+ "=" + var422);
			tempfile.WriteLine("var"+423+ "=" + var423);
			tempfile.WriteLine("var"+424+ "=" + var424);
			tempfile.WriteLine("var"+425+ "=" + var425);
			tempfile.WriteLine("var"+426+ "=" + var426);
			tempfile.WriteLine("var"+427+ "=" + var427);
			tempfile.WriteLine("var"+428+ "=" + var428);
			tempfile.WriteLine("var"+429+ "=" + var429);
			tempfile.WriteLine("var"+430+ "=" + var430);
			tempfile.WriteLine("var"+431+ "=" + var431);
			tempfile.WriteLine("var"+432+ "=" + var432);
			tempfile.WriteLine("var"+433+ "=" + var433);
			tempfile.WriteLine("var"+434+ "=" + var434);
			tempfile.WriteLine("var"+435+ "=" + var435);
			tempfile.WriteLine("var"+436+ "=" + var436);
			tempfile.WriteLine("var"+437+ "=" + var437);
			tempfile.WriteLine("var"+438+ "=" + var438);
			tempfile.WriteLine("var"+439+ "=" + var439);
			tempfile.WriteLine("var"+440+ "=" + var440);
			tempfile.WriteLine("var"+441+ "=" + var441);
			tempfile.WriteLine("var"+442+ "=" + var442);
			tempfile.WriteLine("var"+443+ "=" + var443);
			tempfile.WriteLine("var"+444+ "=" + var444);
			tempfile.WriteLine("var"+445+ "=" + var445);
			tempfile.WriteLine("var"+446+ "=" + var446);
			tempfile.WriteLine("var"+447+ "=" + var447);
			tempfile.WriteLine("var"+448+ "=" + var448);
			tempfile.WriteLine("var"+449+ "=" + var449);
			tempfile.WriteLine("var"+450+ "=" + var450);
			tempfile.WriteLine("var"+451+ "=" + var451);
			tempfile.WriteLine("var"+452+ "=" + var452);
			tempfile.WriteLine("var"+453+ "=" + var453);
			tempfile.WriteLine("var"+454+ "=" + var454);
			tempfile.WriteLine("var"+455+ "=" + var455);
			tempfile.WriteLine("var"+456+ "=" + var456);
			tempfile.WriteLine("var"+457+ "=" + var457);
			tempfile.WriteLine("var"+458+ "=" + var458);
			tempfile.WriteLine("var"+459+ "=" + var459);
			tempfile.WriteLine("var"+460+ "=" + var460);
			tempfile.WriteLine("var"+461+ "=" + var461);
			tempfile.WriteLine("var"+462+ "=" + var462);
			tempfile.WriteLine("var"+463+ "=" + var463);
			tempfile.WriteLine("var"+464+ "=" + var464);
			tempfile.WriteLine("var"+465+ "=" + var465);
			tempfile.WriteLine("var"+466+ "=" + var466);
			tempfile.WriteLine("var"+467+ "=" + var467);
			tempfile.WriteLine("var"+468+ "=" + var468);
			tempfile.WriteLine("var"+469+ "=" + var469);
			tempfile.WriteLine("var"+470+ "=" + var470);
			tempfile.WriteLine("var"+471+ "=" + var471);
			tempfile.WriteLine("var"+472+ "=" + var472);
			tempfile.WriteLine("var"+473+ "=" + var473);
			tempfile.WriteLine("var"+474+ "=" + var474);
			tempfile.WriteLine("var"+475+ "=" + var475);
			tempfile.WriteLine("var"+476+ "=" + var476);
			tempfile.WriteLine("var"+477+ "=" + var477);
			tempfile.WriteLine("var"+478+ "=" + var478);
			tempfile.WriteLine("var"+479+ "=" + var479);
			tempfile.WriteLine("var"+480+ "=" + var480);
			tempfile.WriteLine("var"+481+ "=" + var481);
			tempfile.WriteLine("var"+482+ "=" + var482);
			tempfile.WriteLine("var"+483+ "=" + var483);
			tempfile.WriteLine("var"+484+ "=" + var484);
			tempfile.WriteLine("var"+485+ "=" + var485);
			tempfile.WriteLine("var"+486+ "=" + var486);
			tempfile.WriteLine("var"+487+ "=" + var487);
			tempfile.WriteLine("var"+488+ "=" + var488);
			tempfile.WriteLine("var"+489+ "=" + var489);
			tempfile.WriteLine("var"+490+ "=" + var490);
			tempfile.WriteLine("var"+491+ "=" + var491);
			tempfile.WriteLine("var"+492+ "=" + var492);
			tempfile.WriteLine("var"+493+ "=" + var493);
			tempfile.WriteLine("var"+494+ "=" + var494);
			tempfile.WriteLine("var"+495+ "=" + var495);
			tempfile.WriteLine("var"+496+ "=" + var496);
			tempfile.WriteLine("var"+497+ "=" + var497);
			tempfile.WriteLine("var"+498+ "=" + var498);
			tempfile.WriteLine("var"+499+ "=" + var499);
			tempfile.WriteLine("var"+500+ "=" + var500);
			tempfile.WriteLine("var"+501+ "=" + var501);
			tempfile.WriteLine("var"+502+ "=" + var502);
			tempfile.WriteLine("var"+503+ "=" + var503);
			tempfile.WriteLine("var"+504+ "=" + var504);
			tempfile.WriteLine("var"+505+ "=" + var505);
			tempfile.WriteLine("var"+506+ "=" + var506);
			tempfile.WriteLine("var"+507+ "=" + var507);
			tempfile.WriteLine("var"+508+ "=" + var508);
			tempfile.WriteLine("var"+509+ "=" + var509);
			tempfile.WriteLine("var"+510+ "=" + var510);
			tempfile.WriteLine("var"+511+ "=" + var511);
			tempfile.WriteLine("var"+512+ "=" + var512);
			tempfile.WriteLine("var"+513+ "=" + var513);
			tempfile.WriteLine("var"+514+ "=" + var514);
			tempfile.WriteLine("var"+515+ "=" + var515);
			tempfile.WriteLine("var"+516+ "=" + var516);
			tempfile.WriteLine("var"+517+ "=" + var517);
			tempfile.WriteLine("var"+518+ "=" + var518);
			tempfile.WriteLine("var"+519+ "=" + var519);
			tempfile.WriteLine("var"+520+ "=" + var520);
			tempfile.WriteLine("var"+521+ "=" + var521);
			tempfile.WriteLine("var"+522+ "=" + var522);
			tempfile.WriteLine("var"+523+ "=" + var523);
			tempfile.WriteLine("var"+524+ "=" + var524);
			tempfile.WriteLine("var"+525+ "=" + var525);
			tempfile.WriteLine("var"+526+ "=" + var526);
			tempfile.WriteLine("var"+527+ "=" + var527);
			tempfile.WriteLine("var"+528+ "=" + var528);
			tempfile.WriteLine("var"+529+ "=" + var529);
			tempfile.WriteLine("var"+530+ "=" + var530);
			tempfile.WriteLine("var"+531+ "=" + var531);
			tempfile.WriteLine("var"+532+ "=" + var532);
			tempfile.WriteLine("var"+533+ "=" + var533);
			tempfile.WriteLine("var"+534+ "=" + var534);
			tempfile.WriteLine("var"+535+ "=" + var535);
			tempfile.WriteLine("var"+536+ "=" + var536);
			tempfile.WriteLine("var"+537+ "=" + var537);
			tempfile.WriteLine("var"+538+ "=" + var538);
			tempfile.WriteLine("var"+539+ "=" + var539);
			tempfile.WriteLine("var"+540+ "=" + var540);
			tempfile.WriteLine("var"+541+ "=" + var541);
			tempfile.WriteLine("var"+542+ "=" + var542);
			tempfile.WriteLine("var"+543+ "=" + var543);
			tempfile.WriteLine("var"+544+ "=" + var544);
			tempfile.WriteLine("var"+545+ "=" + var545);
			tempfile.WriteLine("var"+546+ "=" + var546);
			tempfile.WriteLine("var"+547+ "=" + var547);
			tempfile.WriteLine("var"+548+ "=" + var548);
			tempfile.WriteLine("var"+549+ "=" + var549);
			tempfile.WriteLine("var"+550+ "=" + var550);
			tempfile.WriteLine("var"+551+ "=" + var551);
			tempfile.WriteLine("var"+552+ "=" + var552);
			tempfile.WriteLine("var"+553+ "=" + var553);
			tempfile.WriteLine("var"+554+ "=" + var554);
			tempfile.WriteLine("var"+555+ "=" + var555);
			tempfile.WriteLine("var"+556+ "=" + var556);
			tempfile.WriteLine("var"+557+ "=" + var557);
			tempfile.WriteLine("var"+558+ "=" + var558);
			tempfile.WriteLine("var"+559+ "=" + var559);
			tempfile.WriteLine("var"+560+ "=" + var560);
			tempfile.WriteLine("var"+561+ "=" + var561);
			tempfile.WriteLine("var"+562+ "=" + var562);
			tempfile.WriteLine("var"+563+ "=" + var563);
			tempfile.WriteLine("var"+564+ "=" + var564);
			tempfile.WriteLine("var"+565+ "=" + var565);
			tempfile.WriteLine("var"+566+ "=" + var566);
			tempfile.WriteLine("var"+567+ "=" + var567);
			tempfile.WriteLine("var"+568+ "=" + var568);
			tempfile.WriteLine("var"+569+ "=" + var569);
			tempfile.WriteLine("var"+570+ "=" + var570);
			tempfile.WriteLine("var"+571+ "=" + var571);
			tempfile.WriteLine("var"+572+ "=" + var572);
			tempfile.WriteLine("var"+573+ "=" + var573);
			tempfile.WriteLine("var"+574+ "=" + var574);
			tempfile.WriteLine("var"+575+ "=" + var575);
			tempfile.WriteLine("var"+576+ "=" + var576);
			tempfile.WriteLine("var"+577+ "=" + var577);
			tempfile.WriteLine("var"+578+ "=" + var578);
			tempfile.WriteLine("var"+579+ "=" + var579);
			tempfile.WriteLine("var"+580+ "=" + var580);
			tempfile.WriteLine("var"+581+ "=" + var581);
			tempfile.WriteLine("var"+582+ "=" + var582);
			tempfile.WriteLine("var"+583+ "=" + var583);
			tempfile.WriteLine("var"+584+ "=" + var584);
			tempfile.WriteLine("var"+585+ "=" + var585);
			tempfile.WriteLine("var"+586+ "=" + var586);
			tempfile.WriteLine("var"+587+ "=" + var587);
			tempfile.WriteLine("var"+588+ "=" + var588);
			tempfile.WriteLine("var"+589+ "=" + var589);
			tempfile.WriteLine("var"+590+ "=" + var590);
			tempfile.WriteLine("var"+591+ "=" + var591);
			tempfile.WriteLine("var"+592+ "=" + var592);
			tempfile.WriteLine("var"+593+ "=" + var593);
			tempfile.WriteLine("var"+594+ "=" + var594);
			tempfile.WriteLine("var"+595+ "=" + var595);
			tempfile.WriteLine("var"+596+ "=" + var596);
			tempfile.WriteLine("var"+597+ "=" + var597);
			tempfile.WriteLine("var"+598+ "=" + var598);
			tempfile.WriteLine("var"+599+ "=" + var599);
			tempfile.WriteLine("var"+600+ "=" + var600);
			tempfile.WriteLine("var"+601+ "=" + var601);
			tempfile.WriteLine("var"+602+ "=" + var602);
			tempfile.WriteLine("var"+603+ "=" + var603);
			tempfile.WriteLine("var"+604+ "=" + var604);
			tempfile.WriteLine("var"+605+ "=" + var605);
			tempfile.WriteLine("var"+606+ "=" + var606);
			tempfile.WriteLine("var"+607+ "=" + var607);
			tempfile.WriteLine("var"+608+ "=" + var608);
			tempfile.WriteLine("var"+609+ "=" + var609);
			tempfile.WriteLine("var"+610+ "=" + var610);
			tempfile.WriteLine("var"+611+ "=" + var611);
			tempfile.WriteLine("var"+612+ "=" + var612);
			tempfile.WriteLine("var"+613+ "=" + var613);
			tempfile.WriteLine("var"+614+ "=" + var614);
			tempfile.WriteLine("var"+615+ "=" + var615);
			tempfile.WriteLine("var"+616+ "=" + var616);
			tempfile.WriteLine("var"+617+ "=" + var617);
			tempfile.WriteLine("var"+618+ "=" + var618);
			tempfile.WriteLine("var"+619+ "=" + var619);
			tempfile.WriteLine("var"+620+ "=" + var620);
			tempfile.WriteLine("var"+621+ "=" + var621);
			tempfile.WriteLine("var"+622+ "=" + var622);
			tempfile.WriteLine("var"+623+ "=" + var623);
			tempfile.WriteLine("var"+624+ "=" + var624);
			tempfile.WriteLine("var"+625+ "=" + var625);
			tempfile.WriteLine("var"+626+ "=" + var626);
			tempfile.WriteLine("var"+627+ "=" + var627);
			tempfile.WriteLine("var"+628+ "=" + var628);
			tempfile.WriteLine("var"+629+ "=" + var629);
			tempfile.WriteLine("var"+630+ "=" + var630);
			tempfile.WriteLine("var"+631+ "=" + var631);
			tempfile.WriteLine("var"+632+ "=" + var632);
			tempfile.WriteLine("var"+633+ "=" + var633);
			tempfile.WriteLine("var"+634+ "=" + var634);
			tempfile.WriteLine("var"+635+ "=" + var635);
			tempfile.WriteLine("var"+636+ "=" + var636);
			tempfile.WriteLine("var"+637+ "=" + var637);
			tempfile.WriteLine("var"+638+ "=" + var638);
			tempfile.WriteLine("var"+639+ "=" + var639);
			tempfile.WriteLine("var"+640+ "=" + var640);
			tempfile.WriteLine("var"+641+ "=" + var641);
			tempfile.WriteLine("var"+642+ "=" + var642);
			tempfile.WriteLine("var"+643+ "=" + var643);
			tempfile.WriteLine("var"+644+ "=" + var644);
			tempfile.WriteLine("var"+645+ "=" + var645);
			tempfile.WriteLine("var"+646+ "=" + var646);
			tempfile.WriteLine("var"+647+ "=" + var647);
			tempfile.WriteLine("var"+648+ "=" + var648);
			tempfile.WriteLine("var"+649+ "=" + var649);
			tempfile.WriteLine("var"+650+ "=" + var650);
			tempfile.WriteLine("var"+651+ "=" + var651);
			tempfile.WriteLine("var"+652+ "=" + var652);
			tempfile.WriteLine("var"+653+ "=" + var653);
			tempfile.WriteLine("var"+654+ "=" + var654);
			tempfile.WriteLine("var"+655+ "=" + var655);
			tempfile.WriteLine("var"+656+ "=" + var656);
			tempfile.WriteLine("var"+657+ "=" + var657);
			tempfile.WriteLine("var"+658+ "=" + var658);
			tempfile.WriteLine("var"+659+ "=" + var659);
			tempfile.WriteLine("var"+660+ "=" + var660);
			tempfile.WriteLine("var"+661+ "=" + var661);
			tempfile.WriteLine("var"+662+ "=" + var662);
			tempfile.WriteLine("var"+663+ "=" + var663);
			tempfile.WriteLine("var"+664+ "=" + var664);
			tempfile.WriteLine("var"+665+ "=" + var665);
			tempfile.WriteLine("var"+666+ "=" + var666);
			tempfile.WriteLine("var"+667+ "=" + var667);
			tempfile.WriteLine("var"+668+ "=" + var668);
			tempfile.WriteLine("var"+669+ "=" + var669);
			tempfile.WriteLine("var"+670+ "=" + var670);
			tempfile.WriteLine("var"+671+ "=" + var671);
			tempfile.WriteLine("var"+672+ "=" + var672);
			tempfile.WriteLine("var"+673+ "=" + var673);
			tempfile.WriteLine("var"+674+ "=" + var674);
			tempfile.WriteLine("var"+675+ "=" + var675);
			tempfile.WriteLine("var"+676+ "=" + var676);
			tempfile.WriteLine("var"+677+ "=" + var677);
			tempfile.WriteLine("var"+678+ "=" + var678);
			tempfile.WriteLine("var"+679+ "=" + var679);
			tempfile.WriteLine("var"+680+ "=" + var680);
			tempfile.WriteLine("var"+681+ "=" + var681);
			tempfile.WriteLine("var"+682+ "=" + var682);
			tempfile.WriteLine("var"+683+ "=" + var683);
			tempfile.WriteLine("var"+684+ "=" + var684);
			tempfile.WriteLine("var"+685+ "=" + var685);
			tempfile.WriteLine("var"+686+ "=" + var686);
			tempfile.WriteLine("var"+687+ "=" + var687);
			tempfile.WriteLine("var"+688+ "=" + var688);
			tempfile.WriteLine("var"+689+ "=" + var689);
			tempfile.WriteLine("var"+690+ "=" + var690);
			tempfile.WriteLine("var"+691+ "=" + var691);
			tempfile.WriteLine("var"+692+ "=" + var692);
			tempfile.WriteLine("var"+693+ "=" + var693);
			tempfile.WriteLine("var"+694+ "=" + var694);
			tempfile.WriteLine("var"+695+ "=" + var695);
			tempfile.WriteLine("var"+696+ "=" + var696);
			tempfile.WriteLine("var"+697+ "=" + var697);
			tempfile.WriteLine("var"+698+ "=" + var698);
			tempfile.WriteLine("var"+699+ "=" + var699);
			tempfile.WriteLine("var"+700+ "=" + var700);
			tempfile.WriteLine("var"+701+ "=" + var701);
			tempfile.WriteLine("var"+702+ "=" + var702);
			tempfile.WriteLine("var"+703+ "=" + var703);
			tempfile.WriteLine("var"+704+ "=" + var704);
			tempfile.WriteLine("var"+705+ "=" + var705);
			tempfile.WriteLine("var"+706+ "=" + var706);
			tempfile.WriteLine("var"+707+ "=" + var707);
			tempfile.WriteLine("var"+708+ "=" + var708);
			tempfile.WriteLine("var"+709+ "=" + var709);
			tempfile.WriteLine("var"+710+ "=" + var710);
			tempfile.WriteLine("var"+711+ "=" + var711);
			tempfile.WriteLine("var"+712+ "=" + var712);
			tempfile.WriteLine("var"+713+ "=" + var713);
			tempfile.WriteLine("var"+714+ "=" + var714);
			tempfile.WriteLine("var"+715+ "=" + var715);
			tempfile.WriteLine("var"+716+ "=" + var716);
			tempfile.WriteLine("var"+717+ "=" + var717);
			tempfile.WriteLine("var"+718+ "=" + var718);
			tempfile.WriteLine("var"+719+ "=" + var719);
			tempfile.WriteLine("var"+720+ "=" + var720);
			tempfile.WriteLine("var"+721+ "=" + var721);
			tempfile.WriteLine("var"+722+ "=" + var722);
			tempfile.WriteLine("var"+723+ "=" + var723);
			tempfile.WriteLine("var"+724+ "=" + var724);
			tempfile.WriteLine("var"+725+ "=" + var725);
			tempfile.WriteLine("var"+726+ "=" + var726);
			tempfile.WriteLine("var"+727+ "=" + var727);
			tempfile.WriteLine("var"+728+ "=" + var728);
			tempfile.WriteLine("var"+729+ "=" + var729);
			tempfile.WriteLine("var"+730+ "=" + var730);
			tempfile.WriteLine("var"+731+ "=" + var731);
			tempfile.WriteLine("var"+732+ "=" + var732);
			tempfile.WriteLine("var"+733+ "=" + var733);
			tempfile.WriteLine("var"+734+ "=" + var734);
			tempfile.WriteLine("var"+735+ "=" + var735);
			tempfile.WriteLine("var"+736+ "=" + var736);
			tempfile.WriteLine("var"+737+ "=" + var737);
			tempfile.WriteLine("var"+738+ "=" + var738);
			tempfile.WriteLine("var"+739+ "=" + var739);
			tempfile.WriteLine("var"+740+ "=" + var740);
			tempfile.WriteLine("var"+741+ "=" + var741);
			tempfile.WriteLine("var"+742+ "=" + var742);
			tempfile.WriteLine("var"+743+ "=" + var743);
			tempfile.WriteLine("var"+744+ "=" + var744);
			tempfile.WriteLine("var"+745+ "=" + var745);
			tempfile.WriteLine("var"+746+ "=" + var746);
			tempfile.WriteLine("var"+747+ "=" + var747);
			tempfile.WriteLine("var"+748+ "=" + var748);
			tempfile.WriteLine("var"+749+ "=" + var749);
			tempfile.WriteLine("var"+750+ "=" + var750);
			tempfile.WriteLine("var"+751+ "=" + var751);
			tempfile.WriteLine("var"+752+ "=" + var752);
			tempfile.WriteLine("var"+753+ "=" + var753);
			tempfile.WriteLine("var"+754+ "=" + var754);
			tempfile.WriteLine("var"+755+ "=" + var755);
			tempfile.WriteLine("var"+756+ "=" + var756);
			tempfile.WriteLine("var"+757+ "=" + var757);
			tempfile.WriteLine("var"+758+ "=" + var758);
			tempfile.WriteLine("var"+759+ "=" + var759);
			tempfile.WriteLine("var"+760+ "=" + var760);
			tempfile.WriteLine("var"+761+ "=" + var761);
			tempfile.WriteLine("var"+762+ "=" + var762);
			tempfile.WriteLine("var"+763+ "=" + var763);
			tempfile.WriteLine("var"+764+ "=" + var764);
			tempfile.WriteLine("var"+765+ "=" + var765);
			tempfile.WriteLine("var"+766+ "=" + var766);
			tempfile.WriteLine("var"+767+ "=" + var767);
			tempfile.WriteLine("var"+768+ "=" + var768);
			tempfile.WriteLine("var"+769+ "=" + var769);
			tempfile.WriteLine("var"+770+ "=" + var770);
			tempfile.WriteLine("var"+771+ "=" + var771);
			tempfile.WriteLine("var"+772+ "=" + var772);
			tempfile.WriteLine("var"+773+ "=" + var773);
			tempfile.WriteLine("var"+774+ "=" + var774);
			tempfile.WriteLine("var"+775+ "=" + var775);
			tempfile.WriteLine("var"+776+ "=" + var776);
			tempfile.WriteLine("var"+777+ "=" + var777);
			tempfile.WriteLine("var"+778+ "=" + var778);
			tempfile.WriteLine("var"+779+ "=" + var779);
			tempfile.WriteLine("var"+780+ "=" + var780);
			tempfile.WriteLine("var"+781+ "=" + var781);
			tempfile.WriteLine("var"+782+ "=" + var782);
			tempfile.WriteLine("var"+783+ "=" + var783);
			tempfile.WriteLine("var"+784+ "=" + var784);
			tempfile.WriteLine("var"+785+ "=" + var785);
			tempfile.WriteLine("var"+786+ "=" + var786);
			tempfile.WriteLine("var"+787+ "=" + var787);
			tempfile.WriteLine("var"+788+ "=" + var788);
			tempfile.WriteLine("var"+789+ "=" + var789);
			tempfile.WriteLine("var"+790+ "=" + var790);
			tempfile.WriteLine("var"+791+ "=" + var791);
			tempfile.WriteLine("var"+792+ "=" + var792);
			tempfile.WriteLine("var"+793+ "=" + var793);
			tempfile.WriteLine("var"+794+ "=" + var794);
			tempfile.WriteLine("var"+795+ "=" + var795);
			tempfile.WriteLine("var"+796+ "=" + var796);
			tempfile.WriteLine("var"+797+ "=" + var797);
			tempfile.WriteLine("var"+798+ "=" + var798);
			tempfile.WriteLine("var"+799+ "=" + var799);
			tempfile.WriteLine("var"+800+ "=" + var800);
			tempfile.WriteLine("var"+801+ "=" + var801);
			tempfile.WriteLine("var"+802+ "=" + var802);
			tempfile.WriteLine("var"+803+ "=" + var803);
			tempfile.WriteLine("var"+804+ "=" + var804);
			tempfile.WriteLine("var"+805+ "=" + var805);
			tempfile.WriteLine("var"+806+ "=" + var806);
			tempfile.WriteLine("var"+807+ "=" + var807);
			tempfile.WriteLine("var"+808+ "=" + var808);
			tempfile.WriteLine("var"+809+ "=" + var809);
			tempfile.WriteLine("var"+810+ "=" + var810);
			tempfile.WriteLine("var"+811+ "=" + var811);
			tempfile.WriteLine("var"+812+ "=" + var812);
			tempfile.WriteLine("var"+813+ "=" + var813);
			tempfile.WriteLine("var"+814+ "=" + var814);
			tempfile.WriteLine("var"+815+ "=" + var815);
			tempfile.WriteLine("var"+816+ "=" + var816);
			tempfile.WriteLine("var"+817+ "=" + var817);
			tempfile.WriteLine("var"+818+ "=" + var818);
			tempfile.WriteLine("var"+819+ "=" + var819);
			tempfile.WriteLine("var"+820+ "=" + var820);
			tempfile.WriteLine("var"+821+ "=" + var821);
			tempfile.WriteLine("var"+822+ "=" + var822);
			tempfile.WriteLine("var"+823+ "=" + var823);
			tempfile.WriteLine("var"+824+ "=" + var824);
			tempfile.WriteLine("var"+825+ "=" + var825);
			tempfile.WriteLine("var"+826+ "=" + var826);
			tempfile.WriteLine("var"+827+ "=" + var827);
			tempfile.WriteLine("var"+828+ "=" + var828);
			tempfile.WriteLine("var"+829+ "=" + var829);
			tempfile.WriteLine("var"+830+ "=" + var830);
			tempfile.WriteLine("var"+831+ "=" + var831);
			tempfile.WriteLine("var"+832+ "=" + var832);
			tempfile.WriteLine("var"+833+ "=" + var833);
			tempfile.WriteLine("var"+834+ "=" + var834);
			tempfile.WriteLine("var"+835+ "=" + var835);
			tempfile.WriteLine("var"+836+ "=" + var836);
			tempfile.WriteLine("var"+837+ "=" + var837);
			tempfile.WriteLine("var"+838+ "=" + var838);
			tempfile.WriteLine("var"+839+ "=" + var839);
			tempfile.WriteLine("var"+840+ "=" + var840);
			tempfile.WriteLine("var"+841+ "=" + var841);
			tempfile.WriteLine("var"+842+ "=" + var842);
			tempfile.WriteLine("var"+843+ "=" + var843);
			tempfile.WriteLine("var"+844+ "=" + var844);
			tempfile.WriteLine("var"+845+ "=" + var845);
			tempfile.WriteLine("var"+846+ "=" + var846);
			tempfile.WriteLine("var"+847+ "=" + var847);
			tempfile.WriteLine("var"+848+ "=" + var848);
			tempfile.WriteLine("var"+849+ "=" + var849);
			tempfile.WriteLine("var"+850+ "=" + var850);
			tempfile.WriteLine("var"+851+ "=" + var851);
			tempfile.WriteLine("var"+852+ "=" + var852);
			tempfile.WriteLine("var"+853+ "=" + var853);
			tempfile.WriteLine("var"+854+ "=" + var854);
			tempfile.WriteLine("var"+855+ "=" + var855);
			tempfile.WriteLine("var"+856+ "=" + var856);
			tempfile.WriteLine("var"+857+ "=" + var857);
			tempfile.WriteLine("var"+858+ "=" + var858);
			tempfile.WriteLine("var"+859+ "=" + var859);
			tempfile.WriteLine("var"+860+ "=" + var860);
			tempfile.WriteLine("var"+861+ "=" + var861);
			tempfile.WriteLine("var"+862+ "=" + var862);
			tempfile.WriteLine("var"+863+ "=" + var863);
			tempfile.WriteLine("var"+864+ "=" + var864);
			tempfile.WriteLine("var"+865+ "=" + var865);
			tempfile.WriteLine("var"+866+ "=" + var866);
			tempfile.WriteLine("var"+867+ "=" + var867);
			tempfile.WriteLine("var"+868+ "=" + var868);
			tempfile.WriteLine("var"+869+ "=" + var869);
			tempfile.WriteLine("var"+870+ "=" + var870);
			tempfile.WriteLine("var"+871+ "=" + var871);
			tempfile.WriteLine("var"+872+ "=" + var872);
			tempfile.WriteLine("var"+873+ "=" + var873);
			tempfile.WriteLine("var"+874+ "=" + var874);
			tempfile.WriteLine("var"+875+ "=" + var875);
			tempfile.WriteLine("var"+876+ "=" + var876);
			tempfile.WriteLine("var"+877+ "=" + var877);
			tempfile.WriteLine("var"+878+ "=" + var878);
			tempfile.WriteLine("var"+879+ "=" + var879);
			tempfile.WriteLine("var"+880+ "=" + var880);
			tempfile.WriteLine("var"+881+ "=" + var881);
			tempfile.WriteLine("var"+882+ "=" + var882);
			tempfile.WriteLine("var"+883+ "=" + var883);
			tempfile.WriteLine("var"+884+ "=" + var884);
			tempfile.WriteLine("var"+885+ "=" + var885);
			tempfile.WriteLine("var"+886+ "=" + var886);
			tempfile.WriteLine("var"+887+ "=" + var887);
			tempfile.WriteLine("var"+888+ "=" + var888);
			tempfile.WriteLine("var"+889+ "=" + var889);
			tempfile.WriteLine("var"+890+ "=" + var890);
			tempfile.WriteLine("var"+891+ "=" + var891);
			tempfile.WriteLine("var"+892+ "=" + var892);
			tempfile.WriteLine("var"+893+ "=" + var893);
			tempfile.WriteLine("var"+894+ "=" + var894);
			tempfile.WriteLine("var"+895+ "=" + var895);
			tempfile.WriteLine("var"+896+ "=" + var896);
			tempfile.WriteLine("var"+897+ "=" + var897);
			tempfile.WriteLine("var"+898+ "=" + var898);
			tempfile.WriteLine("var"+899+ "=" + var899);
			tempfile.WriteLine("var"+900+ "=" + var900);
			tempfile.WriteLine("var"+901+ "=" + var901);
			tempfile.WriteLine("var"+902+ "=" + var902);
			tempfile.WriteLine("var"+903+ "=" + var903);
			tempfile.WriteLine("var"+904+ "=" + var904);
			tempfile.WriteLine("var"+905+ "=" + var905);
			tempfile.WriteLine("var"+906+ "=" + var906);
			tempfile.WriteLine("var"+907+ "=" + var907);
			tempfile.WriteLine("var"+908+ "=" + var908);
			tempfile.WriteLine("var"+909+ "=" + var909);
			tempfile.WriteLine("var"+910+ "=" + var910);
			tempfile.WriteLine("var"+911+ "=" + var911);
			tempfile.WriteLine("var"+912+ "=" + var912);
			tempfile.WriteLine("var"+913+ "=" + var913);
			tempfile.WriteLine("var"+914+ "=" + var914);
			tempfile.WriteLine("var"+915+ "=" + var915);
			tempfile.WriteLine("var"+916+ "=" + var916);
			tempfile.WriteLine("var"+917+ "=" + var917);
			tempfile.WriteLine("var"+918+ "=" + var918);
			tempfile.WriteLine("var"+919+ "=" + var919);
			tempfile.WriteLine("var"+920+ "=" + var920);
			tempfile.WriteLine("var"+921+ "=" + var921);
			tempfile.WriteLine("var"+922+ "=" + var922);
			tempfile.WriteLine("var"+923+ "=" + var923);
			tempfile.WriteLine("var"+924+ "=" + var924);
			tempfile.WriteLine("var"+925+ "=" + var925);
			tempfile.WriteLine("var"+926+ "=" + var926);
			tempfile.WriteLine("var"+927+ "=" + var927);
			tempfile.WriteLine("var"+928+ "=" + var928);
			tempfile.WriteLine("var"+929+ "=" + var929);
			tempfile.WriteLine("var"+930+ "=" + var930);
			tempfile.WriteLine("var"+931+ "=" + var931);
			tempfile.WriteLine("var"+932+ "=" + var932);
			tempfile.WriteLine("var"+933+ "=" + var933);
			tempfile.WriteLine("var"+934+ "=" + var934);
			tempfile.WriteLine("var"+935+ "=" + var935);
			tempfile.WriteLine("var"+936+ "=" + var936);
			tempfile.WriteLine("var"+937+ "=" + var937);
			tempfile.WriteLine("var"+938+ "=" + var938);
			tempfile.WriteLine("var"+939+ "=" + var939);
			tempfile.WriteLine("var"+940+ "=" + var940);
			tempfile.WriteLine("var"+941+ "=" + var941);
			tempfile.WriteLine("var"+942+ "=" + var942);
			tempfile.WriteLine("var"+943+ "=" + var943);
			tempfile.WriteLine("var"+944+ "=" + var944);
			tempfile.WriteLine("var"+945+ "=" + var945);
			tempfile.WriteLine("var"+946+ "=" + var946);
			tempfile.WriteLine("var"+947+ "=" + var947);
			tempfile.WriteLine("var"+948+ "=" + var948);
			tempfile.WriteLine("var"+949+ "=" + var949);
			tempfile.WriteLine("var"+950+ "=" + var950);
			tempfile.WriteLine("var"+951+ "=" + var951);
			tempfile.WriteLine("var"+952+ "=" + var952);
			tempfile.WriteLine("var"+953+ "=" + var953);
			tempfile.WriteLine("var"+954+ "=" + var954);
			tempfile.WriteLine("var"+955+ "=" + var955);
			tempfile.WriteLine("var"+956+ "=" + var956);
			tempfile.WriteLine("var"+957+ "=" + var957);
			tempfile.WriteLine("var"+958+ "=" + var958);
			tempfile.WriteLine("var"+959+ "=" + var959);
			tempfile.WriteLine("var"+960+ "=" + var960);
			tempfile.WriteLine("var"+961+ "=" + var961);
			tempfile.WriteLine("var"+962+ "=" + var962);
			tempfile.WriteLine("var"+963+ "=" + var963);
			tempfile.WriteLine("var"+964+ "=" + var964);
			tempfile.WriteLine("var"+965+ "=" + var965);
			tempfile.WriteLine("var"+966+ "=" + var966);
			tempfile.WriteLine("var"+967+ "=" + var967);
			tempfile.WriteLine("var"+968+ "=" + var968);
			tempfile.WriteLine("var"+969+ "=" + var969);
			tempfile.WriteLine("var"+970+ "=" + var970);
			tempfile.WriteLine("var"+971+ "=" + var971);
			tempfile.WriteLine("var"+972+ "=" + var972);
			tempfile.WriteLine("var"+973+ "=" + var973);
			tempfile.WriteLine("var"+974+ "=" + var974);
			tempfile.WriteLine("var"+975+ "=" + var975);
			tempfile.WriteLine("var"+976+ "=" + var976);
			tempfile.WriteLine("var"+977+ "=" + var977);
			tempfile.WriteLine("var"+978+ "=" + var978);
			tempfile.WriteLine("var"+979+ "=" + var979);
			tempfile.WriteLine("var"+980+ "=" + var980);
			tempfile.WriteLine("var"+981+ "=" + var981);
			tempfile.WriteLine("var"+982+ "=" + var982);
			tempfile.WriteLine("var"+983+ "=" + var983);
			tempfile.WriteLine("var"+984+ "=" + var984);
			tempfile.WriteLine("var"+985+ "=" + var985);
			tempfile.WriteLine("var"+986+ "=" + var986);
			tempfile.WriteLine("var"+987+ "=" + var987);
			tempfile.WriteLine("var"+988+ "=" + var988);
			tempfile.WriteLine("var"+989+ "=" + var989);
			tempfile.WriteLine("var"+990+ "=" + var990);
			tempfile.WriteLine("var"+991+ "=" + var991);
			tempfile.WriteLine("var"+992+ "=" + var992);
			tempfile.WriteLine("var"+993+ "=" + var993);
			tempfile.WriteLine("var"+994+ "=" + var994);
			tempfile.WriteLine("var"+995+ "=" + var995);
			tempfile.WriteLine("var"+996+ "=" + var996);
			tempfile.WriteLine("var"+997+ "=" + var997);
			tempfile.WriteLine("var"+998+ "=" + var998);
			tempfile.WriteLine("var"+999+ "=" + var999);
			#endregion

			QueryPerformanceFrequency(ref frequency);

            Console.WriteLine("Frequency: " + frequency + " Ticks/Sec.");
            Console.WriteLine("First run: " + (time1 / (double)(frequency / 1000000.0)) + " micros");
            Console.WriteLine("Second run: " + (time2 / (double)(frequency / 1000000.0)) + " micros");
            Console.WriteLine("Loop run1: " + (time3 / (double)(frequency / 1000000.0)) + " micros");
            Console.WriteLine("Loop run2: " + (time4 / (double)(frequency / 1000000.0)) + " micros");
            // Print sum in order to avoid that the compiler removes code
            Console.WriteLine("retVal: " + retVal.ToString());
            Console.WriteLine("Sum: " + sum);

            /* write execution times to file */
            if (File.Exists("timings.txt") == false)
            {
                TextWriter timings = new StreamWriter("timings.txt");
                Console.WriteLine("File created");
                timings.Close();
            }

            StreamWriter outfile = File.AppendText("timings.txt");
            outfile.WriteLine("startuptime:\t" + lStartupTime.ToString());
            outfile.WriteLine((time1 / (double)(frequency / 1000000.0)) + "\t" +
                    (time2 / (double)(frequency / 1000000.0)) + "\t" +
                    (time3 / (double)(frequency / 1000000.0)) + "\t" +
                    (time4 / (double)(frequency / 1000000.0)));
            outfile.Close();

            return sum;
		}

class mthdcls0 {

	static int stvr;

	static mthdcls0() {
		stvr = 841;
	}

	public int method0 (int var955, int var313) {
		if (var955>var313)
			return (var955-var313 + stvr);
		else
			return (var313-var955 - stvr);
	}
}

class mthdcls1 {

	static int stvr;

	static mthdcls1() {
		stvr = 498;
	}

	public int method1 (int var862, int var753) {
		if (var862>var753)
			return (var862+var753 + stvr);
		else
			return (var753+var862 - stvr);
	}
}

class mthdcls2 {

	static int stvr;

	static mthdcls2() {
		stvr = 635;
	}

	public int method2 (int var959, int var212) {
		if (var959>var212)
			return (var959+var212 + stvr);
		else
			return (var212+var959 - stvr);
	}
}

class mthdcls3 {

	static int stvr;

	static mthdcls3() {
		stvr = 167;
	}

	public int method3 (int var94, int var940) {
		if (var94>var940)
			return (var94-var940 + stvr);
		else
			return (var940-var94 - stvr);
	}
}

class mthdcls4 {

	static int stvr;

	static mthdcls4() {
		stvr = 552;
	}

	public int method4 (int var981, int var546) {
		if (var981>var546)
			return (var981+var546 + stvr);
		else
			return (var546+var981 - stvr);
	}
}

class mthdcls5 {

	static int stvr;

	static mthdcls5() {
		stvr = 739;
	}

	public int method5 (int var971, int var904) {
		if (var971>var904)
			return (var971+var904 + stvr);
		else
			return (var904+var971 - stvr);
	}
}

class mthdcls6 {

	static int stvr;

	static mthdcls6() {
		stvr = 678;
	}

	public int method6 (int var852, int var167) {
		if (var852>var167)
			return (var852+var167 + stvr);
		else
			return (var167+var852 - stvr);
	}
}

class mthdcls7 {

	static int stvr;

	static mthdcls7() {
		stvr = 199;
	}

	public int method7 (int var794, int var658) {
		if (var794>var658)
			return (var794+var658 + stvr);
		else
			return (var658+var794 - stvr);
	}
}

class mthdcls8 {

	static int stvr;

	static mthdcls8() {
		stvr = 538;
	}

	public int method8 (int var425, int var726) {
		if (var425>var726)
			return (var425-var726 + stvr);
		else
			return (var726-var425 - stvr);
	}
}

class mthdcls9 {

	static int stvr;

	static mthdcls9() {
		stvr = 5;
	}

	public int method9 (int var757, int var986) {
		if (var757>var986)
			return (var757+var986 + stvr);
		else
			return (var986+var757 - stvr);
	}
}

class mthdcls10 {

	static int stvr;

	static mthdcls10() {
		stvr = 934;
	}

	public int method10 (int var26, int var359) {
		if (var26>var359)
			return (var26+var359 + stvr);
		else
			return (var359+var26 - stvr);
	}
}

class mthdcls11 {

	static int stvr;

	static mthdcls11() {
		stvr = 118;
	}

	public int method11 (int var788, int var634) {
		if (var788>var634)
			return (var788-var634 + stvr);
		else
			return (var634-var788 - stvr);
	}
}

class mthdcls12 {

	static int stvr;

	static mthdcls12() {
		stvr = 800;
	}

	public int method12 (int var204, int var554) {
		if (var204>var554)
			return (var204+var554 + stvr);
		else
			return (var554+var204 - stvr);
	}
}

class mthdcls13 {

	static int stvr;

	static mthdcls13() {
		stvr = 145;
	}

	public int method13 (int var233, int var674) {
		if (var233>var674)
			return (var233+var674 + stvr);
		else
			return (var674+var233 - stvr);
	}
}

class mthdcls14 {

	static int stvr;

	static mthdcls14() {
		stvr = 356;
	}

	public int method14 (int var108, int var934) {
		if (var108>var934)
			return (var108+var934 + stvr);
		else
			return (var934+var108 - stvr);
	}
}

class mthdcls15 {

	static int stvr;

	static mthdcls15() {
		stvr = 699;
	}

	public int method15 (int var621, int var611) {
		if (var621>var611)
			return (var621*var611 + stvr);
		else
			return (var611*var621 - stvr);
	}
}

class mthdcls16 {

	static int stvr;

	static mthdcls16() {
		stvr = 101;
	}

	public int method16 (int var336, int var785) {
		if (var336>var785)
			return (var336+var785 + stvr);
		else
			return (var785+var336 - stvr);
	}
}

class mthdcls17 {

	static int stvr;

	static mthdcls17() {
		stvr = 939;
	}

	public int method17 (int var297, int var366) {
		if (var297>var366)
			return (var297+var366 + stvr);
		else
			return (var366+var297 - stvr);
	}
}

class mthdcls18 {

	static int stvr;

	static mthdcls18() {
		stvr = 306;
	}

	public int method18 (int var120, int var512) {
		if (var120>var512)
			return (var120*var512 + stvr);
		else
			return (var512*var120 - stvr);
	}
}

class mthdcls19 {

	static int stvr;

	static mthdcls19() {
		stvr = 757;
	}

	public int method19 (int var491, int var369) {
		if (var491>var369)
			return (var491+var369 + stvr);
		else
			return (var369+var491 - stvr);
	}
}

class mthdcls20 {

	static int stvr;

	static mthdcls20() {
		stvr = 986;
	}

	public int method20 (int var145, int var286) {
		if (var145>var286)
			return (var145*var286 + stvr);
		else
			return (var286*var145 - stvr);
	}
}

class mthdcls21 {

	static int stvr;

	static mthdcls21() {
		stvr = 92;
	}

	public int method21 (int var240, int var258) {
		if (var240>var258)
			return (var240-var258 + stvr);
		else
			return (var258-var240 - stvr);
	}
}

class mthdcls22 {

	static int stvr;

	static mthdcls22() {
		stvr = 374;
	}

	public int method22 (int var421, int var694) {
		if (var421>var694)
			return (var421+var694 + stvr);
		else
			return (var694+var421 - stvr);
	}
}

class mthdcls23 {

	static int stvr;

	static mthdcls23() {
		stvr = 733;
	}

	public int method23 (int var691, int var248) {
		if (var691>var248)
			return (var691+var248 + stvr);
		else
			return (var248+var691 - stvr);
	}
}

class mthdcls24 {

	static int stvr;

	static mthdcls24() {
		stvr = 304;
	}

	public int method24 (int var863, int var916) {
		if (var863>var916)
			return (var863-var916 + stvr);
		else
			return (var916-var863 - stvr);
	}
}

class mthdcls25 {

	static int stvr;

	static mthdcls25() {
		stvr = 225;
	}

	public int method25 (int var821, int var947) {
		if (var821>var947)
			return (var821-var947 + stvr);
		else
			return (var947-var821 - stvr);
	}
}

class mthdcls26 {

	static int stvr;

	static mthdcls26() {
		stvr = 197;
	}

	public int method26 (int var363, int var518) {
		if (var363>var518)
			return (var363-var518 + stvr);
		else
			return (var518-var363 - stvr);
	}
}

class mthdcls27 {

	static int stvr;

	static mthdcls27() {
		stvr = 232;
	}

	public int method27 (int var642, int var410) {
		if (var642>var410)
			return (var642*var410 + stvr);
		else
			return (var410*var642 - stvr);
	}
}

class mthdcls28 {

	static int stvr;

	static mthdcls28() {
		stvr = 172;
	}

	public int method28 (int var49, int var632) {
		if (var49>var632)
			return (var49*var632 + stvr);
		else
			return (var632*var49 - stvr);
	}
}

class mthdcls29 {

	static int stvr;

	static mthdcls29() {
		stvr = 796;
	}

	public int method29 (int var203, int var208) {
		if (var203>var208)
			return (var203-var208 + stvr);
		else
			return (var208-var203 - stvr);
	}
}

class mthdcls30 {

	static int stvr;

	static mthdcls30() {
		stvr = 360;
	}

	public int method30 (int var543, int var144) {
		if (var543>var144)
			return (var543-var144 + stvr);
		else
			return (var144-var543 - stvr);
	}
}

class mthdcls31 {

	static int stvr;

	static mthdcls31() {
		stvr = 646;
	}

	public int method31 (int var626, int var88) {
		if (var626>var88)
			return (var626+var88 + stvr);
		else
			return (var88+var626 - stvr);
	}
}

class mthdcls32 {

	static int stvr;

	static mthdcls32() {
		stvr = 47;
	}

	public int method32 (int var829, int var25) {
		if (var829>var25)
			return (var829+var25 + stvr);
		else
			return (var25+var829 - stvr);
	}
}

class mthdcls33 {

	static int stvr;

	static mthdcls33() {
		stvr = 116;
	}

	public int method33 (int var789, int var522) {
		if (var789>var522)
			return (var789+var522 + stvr);
		else
			return (var522+var789 - stvr);
	}
}

class mthdcls34 {

	static int stvr;

	static mthdcls34() {
		stvr = 152;
	}

	public int method34 (int var483, int var451) {
		if (var483>var451)
			return (var483-var451 + stvr);
		else
			return (var451-var483 - stvr);
	}
}

class mthdcls35 {

	static int stvr;

	static mthdcls35() {
		stvr = 874;
	}

	public int method35 (int var719, int var556) {
		if (var719>var556)
			return (var719-var556 + stvr);
		else
			return (var556-var719 - stvr);
	}
}

class mthdcls36 {

	static int stvr;

	static mthdcls36() {
		stvr = 350;
	}

	public int method36 (int var733, int var301) {
		if (var733>var301)
			return (var733-var301 + stvr);
		else
			return (var301-var733 - stvr);
	}
}

class mthdcls37 {

	static int stvr;

	static mthdcls37() {
		stvr = 356;
	}

	public int method37 (int var337, int var471) {
		if (var337>var471)
			return (var337+var471 + stvr);
		else
			return (var471+var337 - stvr);
	}
}

class mthdcls38 {

	static int stvr;

	static mthdcls38() {
		stvr = 270;
	}

	public int method38 (int var119, int var294) {
		if (var119>var294)
			return (var119-var294 + stvr);
		else
			return (var294-var119 - stvr);
	}
}

class mthdcls39 {

	static int stvr;

	static mthdcls39() {
		stvr = 553;
	}

	public int method39 (int var598, int var480) {
		if (var598>var480)
			return (var598*var480 + stvr);
		else
			return (var480*var598 - stvr);
	}
}

class mthdcls40 {

	static int stvr;

	static mthdcls40() {
		stvr = 306;
	}

	public int method40 (int var647, int var333) {
		if (var647>var333)
			return (var647+var333 + stvr);
		else
			return (var333+var647 - stvr);
	}
}

class mthdcls41 {

	static int stvr;

	static mthdcls41() {
		stvr = 704;
	}

	public int method41 (int var242, int var793) {
		if (var242>var793)
			return (var242+var793 + stvr);
		else
			return (var793+var242 - stvr);
	}
}

class mthdcls42 {

	static int stvr;

	static mthdcls42() {
		stvr = 933;
	}

	public int method42 (int var155, int var732) {
		if (var155>var732)
			return (var155-var732 + stvr);
		else
			return (var732-var155 - stvr);
	}
}

class mthdcls43 {

	static int stvr;

	static mthdcls43() {
		stvr = 596;
	}

	public int method43 (int var535, int var755) {
		if (var535>var755)
			return (var535-var755 + stvr);
		else
			return (var755-var535 - stvr);
	}
}

class mthdcls44 {

	static int stvr;

	static mthdcls44() {
		stvr = 800;
	}

	public int method44 (int var309, int var968) {
		if (var309>var968)
			return (var309+var968 + stvr);
		else
			return (var968+var309 - stvr);
	}
}

class mthdcls45 {

	static int stvr;

	static mthdcls45() {
		stvr = 428;
	}

	public int method45 (int var149, int var144) {
		if (var149>var144)
			return (var149*var144 + stvr);
		else
			return (var144*var149 - stvr);
	}
}

class mthdcls46 {

	static int stvr;

	static mthdcls46() {
		stvr = 23;
	}

	public int method46 (int var925, int var851) {
		if (var925>var851)
			return (var925+var851 + stvr);
		else
			return (var851+var925 - stvr);
	}
}

class mthdcls47 {

	static int stvr;

	static mthdcls47() {
		stvr = 943;
	}

	public int method47 (int var0, int var367) {
		if (var0>var367)
			return (var0*var367 + stvr);
		else
			return (var367*var0 - stvr);
	}
}

class mthdcls48 {

	static int stvr;

	static mthdcls48() {
		stvr = 690;
	}

	public int method48 (int var582, int var874) {
		if (var582>var874)
			return (var582+var874 + stvr);
		else
			return (var874+var582 - stvr);
	}
}

class mthdcls49 {

	static int stvr;

	static mthdcls49() {
		stvr = 494;
	}

	public int method49 (int var150, int var493) {
		if (var150>var493)
			return (var150-var493 + stvr);
		else
			return (var493-var150 - stvr);
	}
}

class mthdcls50 {

	static int stvr;

	static mthdcls50() {
		stvr = 351;
	}

	public int method50 (int var453, int var51) {
		if (var453>var51)
			return (var453-var51 + stvr);
		else
			return (var51-var453 - stvr);
	}
}

class mthdcls51 {

	static int stvr;

	static mthdcls51() {
		stvr = 498;
	}

	public int method51 (int var795, int var309) {
		if (var795>var309)
			return (var795*var309 + stvr);
		else
			return (var309*var795 - stvr);
	}
}

class mthdcls52 {

	static int stvr;

	static mthdcls52() {
		stvr = 670;
	}

	public int method52 (int var874, int var850) {
		if (var874>var850)
			return (var874+var850 + stvr);
		else
			return (var850+var874 - stvr);
	}
}

class mthdcls53 {

	static int stvr;

	static mthdcls53() {
		stvr = 250;
	}

	public int method53 (int var281, int var41) {
		if (var281>var41)
			return (var281*var41 + stvr);
		else
			return (var41*var281 - stvr);
	}
}

class mthdcls54 {

	static int stvr;

	static mthdcls54() {
		stvr = 91;
	}

	public int method54 (int var41, int var260) {
		if (var41>var260)
			return (var41-var260 + stvr);
		else
			return (var260-var41 - stvr);
	}
}

class mthdcls55 {

	static int stvr;

	static mthdcls55() {
		stvr = 185;
	}

	public int method55 (int var257, int var616) {
		if (var257>var616)
			return (var257-var616 + stvr);
		else
			return (var616-var257 - stvr);
	}
}

class mthdcls56 {

	static int stvr;

	static mthdcls56() {
		stvr = 406;
	}

	public int method56 (int var839, int var646) {
		if (var839>var646)
			return (var839+var646 + stvr);
		else
			return (var646+var839 - stvr);
	}
}

class mthdcls57 {

	static int stvr;

	static mthdcls57() {
		stvr = 217;
	}

	public int method57 (int var75, int var181) {
		if (var75>var181)
			return (var75+var181 + stvr);
		else
			return (var181+var75 - stvr);
	}
}

class mthdcls58 {

	static int stvr;

	static mthdcls58() {
		stvr = 695;
	}

	public int method58 (int var959, int var116) {
		if (var959>var116)
			return (var959-var116 + stvr);
		else
			return (var116-var959 - stvr);
	}
}

class mthdcls59 {

	static int stvr;

	static mthdcls59() {
		stvr = 51;
	}

	public int method59 (int var321, int var783) {
		if (var321>var783)
			return (var321*var783 + stvr);
		else
			return (var783*var321 - stvr);
	}
}

class mthdcls60 {

	static int stvr;

	static mthdcls60() {
		stvr = 980;
	}

	public int method60 (int var533, int var307) {
		if (var533>var307)
			return (var533-var307 + stvr);
		else
			return (var307-var533 - stvr);
	}
}

class mthdcls61 {

	static int stvr;

	static mthdcls61() {
		stvr = 565;
	}

	public int method61 (int var806, int var644) {
		if (var806>var644)
			return (var806+var644 + stvr);
		else
			return (var644+var806 - stvr);
	}
}

class mthdcls62 {

	static int stvr;

	static mthdcls62() {
		stvr = 789;
	}

	public int method62 (int var613, int var92) {
		if (var613>var92)
			return (var613-var92 + stvr);
		else
			return (var92-var613 - stvr);
	}
}

class mthdcls63 {

	static int stvr;

	static mthdcls63() {
		stvr = 107;
	}

	public int method63 (int var757, int var155) {
		if (var757>var155)
			return (var757+var155 + stvr);
		else
			return (var155+var757 - stvr);
	}
}

class mthdcls64 {

	static int stvr;

	static mthdcls64() {
		stvr = 144;
	}

	public int method64 (int var498, int var989) {
		if (var498>var989)
			return (var498-var989 + stvr);
		else
			return (var989-var498 - stvr);
	}
}

class mthdcls65 {

	static int stvr;

	static mthdcls65() {
		stvr = 737;
	}

	public int method65 (int var733, int var280) {
		if (var733>var280)
			return (var733-var280 + stvr);
		else
			return (var280-var733 - stvr);
	}
}

class mthdcls66 {

	static int stvr;

	static mthdcls66() {
		stvr = 396;
	}

	public int method66 (int var612, int var51) {
		if (var612>var51)
			return (var612*var51 + stvr);
		else
			return (var51*var612 - stvr);
	}
}

class mthdcls67 {

	static int stvr;

	static mthdcls67() {
		stvr = 806;
	}

	public int method67 (int var747, int var856) {
		if (var747>var856)
			return (var747*var856 + stvr);
		else
			return (var856*var747 - stvr);
	}
}

class mthdcls68 {

	static int stvr;

	static mthdcls68() {
		stvr = 414;
	}

	public int method68 (int var920, int var73) {
		if (var920>var73)
			return (var920+var73 + stvr);
		else
			return (var73+var920 - stvr);
	}
}

class mthdcls69 {

	static int stvr;

	static mthdcls69() {
		stvr = 347;
	}

	public int method69 (int var969, int var972) {
		if (var969>var972)
			return (var969-var972 + stvr);
		else
			return (var972-var969 - stvr);
	}
}

class mthdcls70 {

	static int stvr;

	static mthdcls70() {
		stvr = 600;
	}

	public int method70 (int var41, int var46) {
		if (var41>var46)
			return (var41-var46 + stvr);
		else
			return (var46-var41 - stvr);
	}
}

class mthdcls71 {

	static int stvr;

	static mthdcls71() {
		stvr = 651;
	}

	public int method71 (int var481, int var911) {
		if (var481>var911)
			return (var481+var911 + stvr);
		else
			return (var911+var481 - stvr);
	}
}

class mthdcls72 {

	static int stvr;

	static mthdcls72() {
		stvr = 234;
	}

	public int method72 (int var950, int var838) {
		if (var950>var838)
			return (var950+var838 + stvr);
		else
			return (var838+var950 - stvr);
	}
}

class mthdcls73 {

	static int stvr;

	static mthdcls73() {
		stvr = 858;
	}

	public int method73 (int var540, int var677) {
		if (var540>var677)
			return (var540+var677 + stvr);
		else
			return (var677+var540 - stvr);
	}
}

class mthdcls74 {

	static int stvr;

	static mthdcls74() {
		stvr = 483;
	}

	public int method74 (int var785, int var807) {
		if (var785>var807)
			return (var785-var807 + stvr);
		else
			return (var807-var785 - stvr);
	}
}

class mthdcls75 {

	static int stvr;

	static mthdcls75() {
		stvr = 806;
	}

	public int method75 (int var451, int var389) {
		if (var451>var389)
			return (var451*var389 + stvr);
		else
			return (var389*var451 - stvr);
	}
}

class mthdcls76 {

	static int stvr;

	static mthdcls76() {
		stvr = 920;
	}

	public int method76 (int var822, int var628) {
		if (var822>var628)
			return (var822*var628 + stvr);
		else
			return (var628*var822 - stvr);
	}
}

class mthdcls77 {

	static int stvr;

	static mthdcls77() {
		stvr = 675;
	}

	public int method77 (int var773, int var817) {
		if (var773>var817)
			return (var773*var817 + stvr);
		else
			return (var817*var773 - stvr);
	}
}

class mthdcls78 {

	static int stvr;

	static mthdcls78() {
		stvr = 127;
	}

	public int method78 (int var69, int var997) {
		if (var69>var997)
			return (var69+var997 + stvr);
		else
			return (var997+var69 - stvr);
	}
}

class mthdcls79 {

	static int stvr;

	static mthdcls79() {
		stvr = 881;
	}

	public int method79 (int var112, int var590) {
		if (var112>var590)
			return (var112*var590 + stvr);
		else
			return (var590*var112 - stvr);
	}
}

class mthdcls80 {

	static int stvr;

	static mthdcls80() {
		stvr = 288;
	}

	public int method80 (int var579, int var165) {
		if (var579>var165)
			return (var579*var165 + stvr);
		else
			return (var165*var579 - stvr);
	}
}

class mthdcls81 {

	static int stvr;

	static mthdcls81() {
		stvr = 588;
	}

	public int method81 (int var413, int var126) {
		if (var413>var126)
			return (var413-var126 + stvr);
		else
			return (var126-var413 - stvr);
	}
}

class mthdcls82 {

	static int stvr;

	static mthdcls82() {
		stvr = 588;
	}

	public int method82 (int var664, int var235) {
		if (var664>var235)
			return (var664+var235 + stvr);
		else
			return (var235+var664 - stvr);
	}
}

class mthdcls83 {

	static int stvr;

	static mthdcls83() {
		stvr = 390;
	}

	public int method83 (int var952, int var711) {
		if (var952>var711)
			return (var952-var711 + stvr);
		else
			return (var711-var952 - stvr);
	}
}

class mthdcls84 {

	static int stvr;

	static mthdcls84() {
		stvr = 663;
	}

	public int method84 (int var949, int var796) {
		if (var949>var796)
			return (var949*var796 + stvr);
		else
			return (var796*var949 - stvr);
	}
}

class mthdcls85 {

	static int stvr;

	static mthdcls85() {
		stvr = 314;
	}

	public int method85 (int var619, int var518) {
		if (var619>var518)
			return (var619*var518 + stvr);
		else
			return (var518*var619 - stvr);
	}
}

class mthdcls86 {

	static int stvr;

	static mthdcls86() {
		stvr = 288;
	}

	public int method86 (int var325, int var800) {
		if (var325>var800)
			return (var325-var800 + stvr);
		else
			return (var800-var325 - stvr);
	}
}

class mthdcls87 {

	static int stvr;

	static mthdcls87() {
		stvr = 99;
	}

	public int method87 (int var586, int var39) {
		if (var586>var39)
			return (var586-var39 + stvr);
		else
			return (var39-var586 - stvr);
	}
}

class mthdcls88 {

	static int stvr;

	static mthdcls88() {
		stvr = 236;
	}

	public int method88 (int var62, int var426) {
		if (var62>var426)
			return (var62+var426 + stvr);
		else
			return (var426+var62 - stvr);
	}
}

class mthdcls89 {

	static int stvr;

	static mthdcls89() {
		stvr = 920;
	}

	public int method89 (int var273, int var333) {
		if (var273>var333)
			return (var273+var333 + stvr);
		else
			return (var333+var273 - stvr);
	}
}

class mthdcls90 {

	static int stvr;

	static mthdcls90() {
		stvr = 795;
	}

	public int method90 (int var593, int var275) {
		if (var593>var275)
			return (var593+var275 + stvr);
		else
			return (var275+var593 - stvr);
	}
}

class mthdcls91 {

	static int stvr;

	static mthdcls91() {
		stvr = 744;
	}

	public int method91 (int var778, int var877) {
		if (var778>var877)
			return (var778-var877 + stvr);
		else
			return (var877-var778 - stvr);
	}
}

class mthdcls92 {

	static int stvr;

	static mthdcls92() {
		stvr = 946;
	}

	public int method92 (int var373, int var26) {
		if (var373>var26)
			return (var373-var26 + stvr);
		else
			return (var26-var373 - stvr);
	}
}

class mthdcls93 {

	static int stvr;

	static mthdcls93() {
		stvr = 678;
	}

	public int method93 (int var237, int var998) {
		if (var237>var998)
			return (var237+var998 + stvr);
		else
			return (var998+var237 - stvr);
	}
}

class mthdcls94 {

	static int stvr;

	static mthdcls94() {
		stvr = 454;
	}

	public int method94 (int var618, int var790) {
		if (var618>var790)
			return (var618+var790 + stvr);
		else
			return (var790+var618 - stvr);
	}
}

class mthdcls95 {

	static int stvr;

	static mthdcls95() {
		stvr = 791;
	}

	public int method95 (int var674, int var1) {
		if (var674>var1)
			return (var674*var1 + stvr);
		else
			return (var1*var674 - stvr);
	}
}

class mthdcls96 {

	static int stvr;

	static mthdcls96() {
		stvr = 938;
	}

	public int method96 (int var741, int var773) {
		if (var741>var773)
			return (var741*var773 + stvr);
		else
			return (var773*var741 - stvr);
	}
}

class mthdcls97 {

	static int stvr;

	static mthdcls97() {
		stvr = 636;
	}

	public int method97 (int var299, int var854) {
		if (var299>var854)
			return (var299*var854 + stvr);
		else
			return (var854*var299 - stvr);
	}
}

class mthdcls98 {

	static int stvr;

	static mthdcls98() {
		stvr = 326;
	}

	public int method98 (int var587, int var361) {
		if (var587>var361)
			return (var587-var361 + stvr);
		else
			return (var361-var587 - stvr);
	}
}

class mthdcls99 {

	static int stvr;

	static mthdcls99() {
		stvr = 461;
	}

	public int method99 (int var271, int var972) {
		if (var271>var972)
			return (var271-var972 + stvr);
		else
			return (var972-var271 - stvr);
	}
}

class mthdcls100 {

	static int stvr;

	static mthdcls100() {
		stvr = 858;
	}

	public int method100 (int var271, int var542) {
		if (var271>var542)
			return (var271+var542 + stvr);
		else
			return (var542+var271 - stvr);
	}
}

class mthdcls101 {

	static int stvr;

	static mthdcls101() {
		stvr = 569;
	}

	public int method101 (int var819, int var337) {
		if (var819>var337)
			return (var819+var337 + stvr);
		else
			return (var337+var819 - stvr);
	}
}

class mthdcls102 {

	static int stvr;

	static mthdcls102() {
		stvr = 653;
	}

	public int method102 (int var924, int var241) {
		if (var924>var241)
			return (var924+var241 + stvr);
		else
			return (var241+var924 - stvr);
	}
}

class mthdcls103 {

	static int stvr;

	static mthdcls103() {
		stvr = 400;
	}

	public int method103 (int var12, int var699) {
		if (var12>var699)
			return (var12-var699 + stvr);
		else
			return (var699-var12 - stvr);
	}
}

class mthdcls104 {

	static int stvr;

	static mthdcls104() {
		stvr = 328;
	}

	public int method104 (int var265, int var537) {
		if (var265>var537)
			return (var265-var537 + stvr);
		else
			return (var537-var265 - stvr);
	}
}

class mthdcls105 {

	static int stvr;

	static mthdcls105() {
		stvr = 646;
	}

	public int method105 (int var75, int var932) {
		if (var75>var932)
			return (var75+var932 + stvr);
		else
			return (var932+var75 - stvr);
	}
}

class mthdcls106 {

	static int stvr;

	static mthdcls106() {
		stvr = 867;
	}

	public int method106 (int var337, int var431) {
		if (var337>var431)
			return (var337*var431 + stvr);
		else
			return (var431*var337 - stvr);
	}
}

class mthdcls107 {

	static int stvr;

	static mthdcls107() {
		stvr = 115;
	}

	public int method107 (int var499, int var120) {
		if (var499>var120)
			return (var499+var120 + stvr);
		else
			return (var120+var499 - stvr);
	}
}

class mthdcls108 {

	static int stvr;

	static mthdcls108() {
		stvr = 231;
	}

	public int method108 (int var599, int var453) {
		if (var599>var453)
			return (var599+var453 + stvr);
		else
			return (var453+var599 - stvr);
	}
}

class mthdcls109 {

	static int stvr;

	static mthdcls109() {
		stvr = 926;
	}

	public int method109 (int var50, int var32) {
		if (var50>var32)
			return (var50+var32 + stvr);
		else
			return (var32+var50 - stvr);
	}
}

class mthdcls110 {

	static int stvr;

	static mthdcls110() {
		stvr = 609;
	}

	public int method110 (int var339, int var326) {
		if (var339>var326)
			return (var339-var326 + stvr);
		else
			return (var326-var339 - stvr);
	}
}

class mthdcls111 {

	static int stvr;

	static mthdcls111() {
		stvr = 641;
	}

	public int method111 (int var840, int var674) {
		if (var840>var674)
			return (var840-var674 + stvr);
		else
			return (var674-var840 - stvr);
	}
}

class mthdcls112 {

	static int stvr;

	static mthdcls112() {
		stvr = 616;
	}

	public int method112 (int var699, int var221) {
		if (var699>var221)
			return (var699+var221 + stvr);
		else
			return (var221+var699 - stvr);
	}
}

class mthdcls113 {

	static int stvr;

	static mthdcls113() {
		stvr = 741;
	}

	public int method113 (int var470, int var10) {
		if (var470>var10)
			return (var470-var10 + stvr);
		else
			return (var10-var470 - stvr);
	}
}

class mthdcls114 {

	static int stvr;

	static mthdcls114() {
		stvr = 679;
	}

	public int method114 (int var979, int var773) {
		if (var979>var773)
			return (var979*var773 + stvr);
		else
			return (var773*var979 - stvr);
	}
}

class mthdcls115 {

	static int stvr;

	static mthdcls115() {
		stvr = 718;
	}

	public int method115 (int var939, int var928) {
		if (var939>var928)
			return (var939-var928 + stvr);
		else
			return (var928-var939 - stvr);
	}
}

class mthdcls116 {

	static int stvr;

	static mthdcls116() {
		stvr = 43;
	}

	public int method116 (int var400, int var291) {
		if (var400>var291)
			return (var400-var291 + stvr);
		else
			return (var291-var400 - stvr);
	}
}

class mthdcls117 {

	static int stvr;

	static mthdcls117() {
		stvr = 881;
	}

	public int method117 (int var115, int var815) {
		if (var115>var815)
			return (var115+var815 + stvr);
		else
			return (var815+var115 - stvr);
	}
}

class mthdcls118 {

	static int stvr;

	static mthdcls118() {
		stvr = 581;
	}

	public int method118 (int var489, int var378) {
		if (var489>var378)
			return (var489+var378 + stvr);
		else
			return (var378+var489 - stvr);
	}
}

class mthdcls119 {

	static int stvr;

	static mthdcls119() {
		stvr = 998;
	}

	public int method119 (int var826, int var773) {
		if (var826>var773)
			return (var826*var773 + stvr);
		else
			return (var773*var826 - stvr);
	}
}

class mthdcls120 {

	static int stvr;

	static mthdcls120() {
		stvr = 561;
	}

	public int method120 (int var122, int var314) {
		if (var122>var314)
			return (var122+var314 + stvr);
		else
			return (var314+var122 - stvr);
	}
}

class mthdcls121 {

	static int stvr;

	static mthdcls121() {
		stvr = 179;
	}

	public int method121 (int var48, int var283) {
		if (var48>var283)
			return (var48-var283 + stvr);
		else
			return (var283-var48 - stvr);
	}
}

class mthdcls122 {

	static int stvr;

	static mthdcls122() {
		stvr = 670;
	}

	public int method122 (int var24, int var793) {
		if (var24>var793)
			return (var24+var793 + stvr);
		else
			return (var793+var24 - stvr);
	}
}

class mthdcls123 {

	static int stvr;

	static mthdcls123() {
		stvr = 885;
	}

	public int method123 (int var320, int var640) {
		if (var320>var640)
			return (var320*var640 + stvr);
		else
			return (var640*var320 - stvr);
	}
}

class mthdcls124 {

	static int stvr;

	static mthdcls124() {
		stvr = 671;
	}

	public int method124 (int var696, int var11) {
		if (var696>var11)
			return (var696*var11 + stvr);
		else
			return (var11*var696 - stvr);
	}
}

class mthdcls125 {

	static int stvr;

	static mthdcls125() {
		stvr = 943;
	}

	public int method125 (int var665, int var211) {
		if (var665>var211)
			return (var665-var211 + stvr);
		else
			return (var211-var665 - stvr);
	}
}

class mthdcls126 {

	static int stvr;

	static mthdcls126() {
		stvr = 148;
	}

	public int method126 (int var655, int var748) {
		if (var655>var748)
			return (var655-var748 + stvr);
		else
			return (var748-var655 - stvr);
	}
}

class mthdcls127 {

	static int stvr;

	static mthdcls127() {
		stvr = 173;
	}

	public int method127 (int var607, int var620) {
		if (var607>var620)
			return (var607-var620 + stvr);
		else
			return (var620-var607 - stvr);
	}
}

class mthdcls128 {

	static int stvr;

	static mthdcls128() {
		stvr = 852;
	}

	public int method128 (int var475, int var929) {
		if (var475>var929)
			return (var475-var929 + stvr);
		else
			return (var929-var475 - stvr);
	}
}

class mthdcls129 {

	static int stvr;

	static mthdcls129() {
		stvr = 397;
	}

	public int method129 (int var477, int var707) {
		if (var477>var707)
			return (var477-var707 + stvr);
		else
			return (var707-var477 - stvr);
	}
}

class mthdcls130 {

	static int stvr;

	static mthdcls130() {
		stvr = 537;
	}

	public int method130 (int var614, int var830) {
		if (var614>var830)
			return (var614*var830 + stvr);
		else
			return (var830*var614 - stvr);
	}
}

class mthdcls131 {

	static int stvr;

	static mthdcls131() {
		stvr = 115;
	}

	public int method131 (int var373, int var166) {
		if (var373>var166)
			return (var373+var166 + stvr);
		else
			return (var166+var373 - stvr);
	}
}

class mthdcls132 {

	static int stvr;

	static mthdcls132() {
		stvr = 682;
	}

	public int method132 (int var427, int var109) {
		if (var427>var109)
			return (var427+var109 + stvr);
		else
			return (var109+var427 - stvr);
	}
}

class mthdcls133 {

	static int stvr;

	static mthdcls133() {
		stvr = 118;
	}

	public int method133 (int var94, int var941) {
		if (var94>var941)
			return (var94+var941 + stvr);
		else
			return (var941+var94 - stvr);
	}
}

class mthdcls134 {

	static int stvr;

	static mthdcls134() {
		stvr = 254;
	}

	public int method134 (int var613, int var242) {
		if (var613>var242)
			return (var613+var242 + stvr);
		else
			return (var242+var613 - stvr);
	}
}

class mthdcls135 {

	static int stvr;

	static mthdcls135() {
		stvr = 393;
	}

	public int method135 (int var866, int var474) {
		if (var866>var474)
			return (var866-var474 + stvr);
		else
			return (var474-var866 - stvr);
	}
}

class mthdcls136 {

	static int stvr;

	static mthdcls136() {
		stvr = 166;
	}

	public int method136 (int var499, int var96) {
		if (var499>var96)
			return (var499*var96 + stvr);
		else
			return (var96*var499 - stvr);
	}
}

class mthdcls137 {

	static int stvr;

	static mthdcls137() {
		stvr = 297;
	}

	public int method137 (int var546, int var66) {
		if (var546>var66)
			return (var546*var66 + stvr);
		else
			return (var66*var546 - stvr);
	}
}

class mthdcls138 {

	static int stvr;

	static mthdcls138() {
		stvr = 340;
	}

	public int method138 (int var665, int var502) {
		if (var665>var502)
			return (var665*var502 + stvr);
		else
			return (var502*var665 - stvr);
	}
}

class mthdcls139 {

	static int stvr;

	static mthdcls139() {
		stvr = 115;
	}

	public int method139 (int var232, int var674) {
		if (var232>var674)
			return (var232+var674 + stvr);
		else
			return (var674+var232 - stvr);
	}
}

class mthdcls140 {

	static int stvr;

	static mthdcls140() {
		stvr = 906;
	}

	public int method140 (int var3, int var313) {
		if (var3>var313)
			return (var3+var313 + stvr);
		else
			return (var313+var3 - stvr);
	}
}

class mthdcls141 {

	static int stvr;

	static mthdcls141() {
		stvr = 239;
	}

	public int method141 (int var518, int var663) {
		if (var518>var663)
			return (var518+var663 + stvr);
		else
			return (var663+var518 - stvr);
	}
}

class mthdcls142 {

	static int stvr;

	static mthdcls142() {
		stvr = 114;
	}

	public int method142 (int var307, int var868) {
		if (var307>var868)
			return (var307+var868 + stvr);
		else
			return (var868+var307 - stvr);
	}
}

class mthdcls143 {

	static int stvr;

	static mthdcls143() {
		stvr = 641;
	}

	public int method143 (int var925, int var768) {
		if (var925>var768)
			return (var925+var768 + stvr);
		else
			return (var768+var925 - stvr);
	}
}

class mthdcls144 {

	static int stvr;

	static mthdcls144() {
		stvr = 370;
	}

	public int method144 (int var419, int var826) {
		if (var419>var826)
			return (var419-var826 + stvr);
		else
			return (var826-var419 - stvr);
	}
}

class mthdcls145 {

	static int stvr;

	static mthdcls145() {
		stvr = 349;
	}

	public int method145 (int var300, int var336) {
		if (var300>var336)
			return (var300+var336 + stvr);
		else
			return (var336+var300 - stvr);
	}
}

class mthdcls146 {

	static int stvr;

	static mthdcls146() {
		stvr = 481;
	}

	public int method146 (int var202, int var234) {
		if (var202>var234)
			return (var202-var234 + stvr);
		else
			return (var234-var202 - stvr);
	}
}

class mthdcls147 {

	static int stvr;

	static mthdcls147() {
		stvr = 255;
	}

	public int method147 (int var630, int var982) {
		if (var630>var982)
			return (var630+var982 + stvr);
		else
			return (var982+var630 - stvr);
	}
}

class mthdcls148 {

	static int stvr;

	static mthdcls148() {
		stvr = 552;
	}

	public int method148 (int var777, int var424) {
		if (var777>var424)
			return (var777+var424 + stvr);
		else
			return (var424+var777 - stvr);
	}
}

class mthdcls149 {

	static int stvr;

	static mthdcls149() {
		stvr = 678;
	}

	public int method149 (int var839, int var131) {
		if (var839>var131)
			return (var839-var131 + stvr);
		else
			return (var131-var839 - stvr);
	}
}

class mthdcls150 {

	static int stvr;

	static mthdcls150() {
		stvr = 170;
	}

	public int method150 (int var896, int var325) {
		if (var896>var325)
			return (var896-var325 + stvr);
		else
			return (var325-var896 - stvr);
	}
}

class mthdcls151 {

	static int stvr;

	static mthdcls151() {
		stvr = 563;
	}

	public int method151 (int var768, int var831) {
		if (var768>var831)
			return (var768-var831 + stvr);
		else
			return (var831-var768 - stvr);
	}
}

class mthdcls152 {

	static int stvr;

	static mthdcls152() {
		stvr = 440;
	}

	public int method152 (int var536, int var408) {
		if (var536>var408)
			return (var536-var408 + stvr);
		else
			return (var408-var536 - stvr);
	}
}

class mthdcls153 {

	static int stvr;

	static mthdcls153() {
		stvr = 324;
	}

	public int method153 (int var882, int var315) {
		if (var882>var315)
			return (var882-var315 + stvr);
		else
			return (var315-var882 - stvr);
	}
}

class mthdcls154 {

	static int stvr;

	static mthdcls154() {
		stvr = 52;
	}

	public int method154 (int var793, int var90) {
		if (var793>var90)
			return (var793+var90 + stvr);
		else
			return (var90+var793 - stvr);
	}
}

class mthdcls155 {

	static int stvr;

	static mthdcls155() {
		stvr = 907;
	}

	public int method155 (int var94, int var655) {
		if (var94>var655)
			return (var94+var655 + stvr);
		else
			return (var655+var94 - stvr);
	}
}

class mthdcls156 {

	static int stvr;

	static mthdcls156() {
		stvr = 863;
	}

	public int method156 (int var468, int var772) {
		if (var468>var772)
			return (var468+var772 + stvr);
		else
			return (var772+var468 - stvr);
	}
}

class mthdcls157 {

	static int stvr;

	static mthdcls157() {
		stvr = 9;
	}

	public int method157 (int var793, int var794) {
		if (var793>var794)
			return (var793+var794 + stvr);
		else
			return (var794+var793 - stvr);
	}
}

class mthdcls158 {

	static int stvr;

	static mthdcls158() {
		stvr = 788;
	}

	public int method158 (int var314, int var657) {
		if (var314>var657)
			return (var314*var657 + stvr);
		else
			return (var657*var314 - stvr);
	}
}

class mthdcls159 {

	static int stvr;

	static mthdcls159() {
		stvr = 613;
	}

	public int method159 (int var687, int var371) {
		if (var687>var371)
			return (var687-var371 + stvr);
		else
			return (var371-var687 - stvr);
	}
}

class mthdcls160 {

	static int stvr;

	static mthdcls160() {
		stvr = 985;
	}

	public int method160 (int var184, int var224) {
		if (var184>var224)
			return (var184+var224 + stvr);
		else
			return (var224+var184 - stvr);
	}
}

class mthdcls161 {

	static int stvr;

	static mthdcls161() {
		stvr = 389;
	}

	public int method161 (int var123, int var461) {
		if (var123>var461)
			return (var123+var461 + stvr);
		else
			return (var461+var123 - stvr);
	}
}

class mthdcls162 {

	static int stvr;

	static mthdcls162() {
		stvr = 590;
	}

	public int method162 (int var974, int var822) {
		if (var974>var822)
			return (var974+var822 + stvr);
		else
			return (var822+var974 - stvr);
	}
}

class mthdcls163 {

	static int stvr;

	static mthdcls163() {
		stvr = 299;
	}

	public int method163 (int var878, int var620) {
		if (var878>var620)
			return (var878+var620 + stvr);
		else
			return (var620+var878 - stvr);
	}
}

class mthdcls164 {

	static int stvr;

	static mthdcls164() {
		stvr = 849;
	}

	public int method164 (int var511, int var702) {
		if (var511>var702)
			return (var511*var702 + stvr);
		else
			return (var702*var511 - stvr);
	}
}

class mthdcls165 {

	static int stvr;

	static mthdcls165() {
		stvr = 147;
	}

	public int method165 (int var569, int var104) {
		if (var569>var104)
			return (var569-var104 + stvr);
		else
			return (var104-var569 - stvr);
	}
}

class mthdcls166 {

	static int stvr;

	static mthdcls166() {
		stvr = 193;
	}

	public int method166 (int var632, int var266) {
		if (var632>var266)
			return (var632-var266 + stvr);
		else
			return (var266-var632 - stvr);
	}
}

class mthdcls167 {

	static int stvr;

	static mthdcls167() {
		stvr = 912;
	}

	public int method167 (int var646, int var181) {
		if (var646>var181)
			return (var646+var181 + stvr);
		else
			return (var181+var646 - stvr);
	}
}

class mthdcls168 {

	static int stvr;

	static mthdcls168() {
		stvr = 182;
	}

	public int method168 (int var173, int var495) {
		if (var173>var495)
			return (var173*var495 + stvr);
		else
			return (var495*var173 - stvr);
	}
}

class mthdcls169 {

	static int stvr;

	static mthdcls169() {
		stvr = 672;
	}

	public int method169 (int var612, int var807) {
		if (var612>var807)
			return (var612-var807 + stvr);
		else
			return (var807-var612 - stvr);
	}
}

class mthdcls170 {

	static int stvr;

	static mthdcls170() {
		stvr = 10;
	}

	public int method170 (int var582, int var224) {
		if (var582>var224)
			return (var582*var224 + stvr);
		else
			return (var224*var582 - stvr);
	}
}

class mthdcls171 {

	static int stvr;

	static mthdcls171() {
		stvr = 466;
	}

	public int method171 (int var917, int var30) {
		if (var917>var30)
			return (var917*var30 + stvr);
		else
			return (var30*var917 - stvr);
	}
}

class mthdcls172 {

	static int stvr;

	static mthdcls172() {
		stvr = 454;
	}

	public int method172 (int var941, int var548) {
		if (var941>var548)
			return (var941+var548 + stvr);
		else
			return (var548+var941 - stvr);
	}
}

class mthdcls173 {

	static int stvr;

	static mthdcls173() {
		stvr = 299;
	}

	public int method173 (int var478, int var639) {
		if (var478>var639)
			return (var478+var639 + stvr);
		else
			return (var639+var478 - stvr);
	}
}

class mthdcls174 {

	static int stvr;

	static mthdcls174() {
		stvr = 633;
	}

	public int method174 (int var70, int var947) {
		if (var70>var947)
			return (var70+var947 + stvr);
		else
			return (var947+var70 - stvr);
	}
}

class mthdcls175 {

	static int stvr;

	static mthdcls175() {
		stvr = 567;
	}

	public int method175 (int var287, int var691) {
		if (var287>var691)
			return (var287-var691 + stvr);
		else
			return (var691-var287 - stvr);
	}
}

class mthdcls176 {

	static int stvr;

	static mthdcls176() {
		stvr = 950;
	}

	public int method176 (int var539, int var637) {
		if (var539>var637)
			return (var539+var637 + stvr);
		else
			return (var637+var539 - stvr);
	}
}

class mthdcls177 {

	static int stvr;

	static mthdcls177() {
		stvr = 349;
	}

	public int method177 (int var83, int var811) {
		if (var83>var811)
			return (var83-var811 + stvr);
		else
			return (var811-var83 - stvr);
	}
}

class mthdcls178 {

	static int stvr;

	static mthdcls178() {
		stvr = 986;
	}

	public int method178 (int var6, int var882) {
		if (var6>var882)
			return (var6+var882 + stvr);
		else
			return (var882+var6 - stvr);
	}
}

class mthdcls179 {

	static int stvr;

	static mthdcls179() {
		stvr = 431;
	}

	public int method179 (int var226, int var862) {
		if (var226>var862)
			return (var226*var862 + stvr);
		else
			return (var862*var226 - stvr);
	}
}

class mthdcls180 {

	static int stvr;

	static mthdcls180() {
		stvr = 274;
	}

	public int method180 (int var921, int var240) {
		if (var921>var240)
			return (var921*var240 + stvr);
		else
			return (var240*var921 - stvr);
	}
}

class mthdcls181 {

	static int stvr;

	static mthdcls181() {
		stvr = 98;
	}

	public int method181 (int var944, int var273) {
		if (var944>var273)
			return (var944*var273 + stvr);
		else
			return (var273*var944 - stvr);
	}
}

class mthdcls182 {

	static int stvr;

	static mthdcls182() {
		stvr = 949;
	}

	public int method182 (int var106, int var681) {
		if (var106>var681)
			return (var106+var681 + stvr);
		else
			return (var681+var106 - stvr);
	}
}

class mthdcls183 {

	static int stvr;

	static mthdcls183() {
		stvr = 755;
	}

	public int method183 (int var59, int var562) {
		if (var59>var562)
			return (var59*var562 + stvr);
		else
			return (var562*var59 - stvr);
	}
}

class mthdcls184 {

	static int stvr;

	static mthdcls184() {
		stvr = 108;
	}

	public int method184 (int var616, int var207) {
		if (var616>var207)
			return (var616*var207 + stvr);
		else
			return (var207*var616 - stvr);
	}
}

class mthdcls185 {

	static int stvr;

	static mthdcls185() {
		stvr = 523;
	}

	public int method185 (int var830, int var672) {
		if (var830>var672)
			return (var830*var672 + stvr);
		else
			return (var672*var830 - stvr);
	}
}

class mthdcls186 {

	static int stvr;

	static mthdcls186() {
		stvr = 534;
	}

	public int method186 (int var13, int var592) {
		if (var13>var592)
			return (var13+var592 + stvr);
		else
			return (var592+var13 - stvr);
	}
}

class mthdcls187 {

	static int stvr;

	static mthdcls187() {
		stvr = 984;
	}

	public int method187 (int var857, int var688) {
		if (var857>var688)
			return (var857*var688 + stvr);
		else
			return (var688*var857 - stvr);
	}
}

class mthdcls188 {

	static int stvr;

	static mthdcls188() {
		stvr = 304;
	}

	public int method188 (int var521, int var56) {
		if (var521>var56)
			return (var521+var56 + stvr);
		else
			return (var56+var521 - stvr);
	}
}

class mthdcls189 {

	static int stvr;

	static mthdcls189() {
		stvr = 797;
	}

	public int method189 (int var798, int var773) {
		if (var798>var773)
			return (var798-var773 + stvr);
		else
			return (var773-var798 - stvr);
	}
}

class mthdcls190 {

	static int stvr;

	static mthdcls190() {
		stvr = 218;
	}

	public int method190 (int var553, int var338) {
		if (var553>var338)
			return (var553*var338 + stvr);
		else
			return (var338*var553 - stvr);
	}
}

class mthdcls191 {

	static int stvr;

	static mthdcls191() {
		stvr = 509;
	}

	public int method191 (int var328, int var705) {
		if (var328>var705)
			return (var328-var705 + stvr);
		else
			return (var705-var328 - stvr);
	}
}

class mthdcls192 {

	static int stvr;

	static mthdcls192() {
		stvr = 871;
	}

	public int method192 (int var256, int var288) {
		if (var256>var288)
			return (var256+var288 + stvr);
		else
			return (var288+var256 - stvr);
	}
}

class mthdcls193 {

	static int stvr;

	static mthdcls193() {
		stvr = 208;
	}

	public int method193 (int var49, int var376) {
		if (var49>var376)
			return (var49-var376 + stvr);
		else
			return (var376-var49 - stvr);
	}
}

class mthdcls194 {

	static int stvr;

	static mthdcls194() {
		stvr = 369;
	}

	public int method194 (int var286, int var764) {
		if (var286>var764)
			return (var286*var764 + stvr);
		else
			return (var764*var286 - stvr);
	}
}

class mthdcls195 {

	static int stvr;

	static mthdcls195() {
		stvr = 72;
	}

	public int method195 (int var277, int var989) {
		if (var277>var989)
			return (var277-var989 + stvr);
		else
			return (var989-var277 - stvr);
	}
}

class mthdcls196 {

	static int stvr;

	static mthdcls196() {
		stvr = 865;
	}

	public int method196 (int var124, int var163) {
		if (var124>var163)
			return (var124-var163 + stvr);
		else
			return (var163-var124 - stvr);
	}
}

class mthdcls197 {

	static int stvr;

	static mthdcls197() {
		stvr = 692;
	}

	public int method197 (int var724, int var721) {
		if (var724>var721)
			return (var724-var721 + stvr);
		else
			return (var721-var724 - stvr);
	}
}

class mthdcls198 {

	static int stvr;

	static mthdcls198() {
		stvr = 633;
	}

	public int method198 (int var481, int var479) {
		if (var481>var479)
			return (var481-var479 + stvr);
		else
			return (var479-var481 - stvr);
	}
}

class mthdcls199 {

	static int stvr;

	static mthdcls199() {
		stvr = 230;
	}

	public int method199 (int var757, int var687) {
		if (var757>var687)
			return (var757-var687 + stvr);
		else
			return (var687-var757 - stvr);
	}
}

class mthdcls200 {

	static int stvr;

	static mthdcls200() {
		stvr = 332;
	}

	public int method200 (int var808, int var701) {
		if (var808>var701)
			return (var808-var701 + stvr);
		else
			return (var701-var808 - stvr);
	}
}

class mthdcls201 {

	static int stvr;

	static mthdcls201() {
		stvr = 683;
	}

	public int method201 (int var390, int var472) {
		if (var390>var472)
			return (var390*var472 + stvr);
		else
			return (var472*var390 - stvr);
	}
}

class mthdcls202 {

	static int stvr;

	static mthdcls202() {
		stvr = 856;
	}

	public int method202 (int var607, int var13) {
		if (var607>var13)
			return (var607*var13 + stvr);
		else
			return (var13*var607 - stvr);
	}
}

class mthdcls203 {

	static int stvr;

	static mthdcls203() {
		stvr = 529;
	}

	public int method203 (int var776, int var655) {
		if (var776>var655)
			return (var776+var655 + stvr);
		else
			return (var655+var776 - stvr);
	}
}

class mthdcls204 {

	static int stvr;

	static mthdcls204() {
		stvr = 688;
	}

	public int method204 (int var362, int var146) {
		if (var362>var146)
			return (var362*var146 + stvr);
		else
			return (var146*var362 - stvr);
	}
}

class mthdcls205 {

	static int stvr;

	static mthdcls205() {
		stvr = 968;
	}

	public int method205 (int var585, int var432) {
		if (var585>var432)
			return (var585*var432 + stvr);
		else
			return (var432*var585 - stvr);
	}
}

class mthdcls206 {

	static int stvr;

	static mthdcls206() {
		stvr = 933;
	}

	public int method206 (int var804, int var305) {
		if (var804>var305)
			return (var804-var305 + stvr);
		else
			return (var305-var804 - stvr);
	}
}

class mthdcls207 {

	static int stvr;

	static mthdcls207() {
		stvr = 608;
	}

	public int method207 (int var110, int var306) {
		if (var110>var306)
			return (var110-var306 + stvr);
		else
			return (var306-var110 - stvr);
	}
}

class mthdcls208 {

	static int stvr;

	static mthdcls208() {
		stvr = 754;
	}

	public int method208 (int var69, int var192) {
		if (var69>var192)
			return (var69*var192 + stvr);
		else
			return (var192*var69 - stvr);
	}
}

class mthdcls209 {

	static int stvr;

	static mthdcls209() {
		stvr = 618;
	}

	public int method209 (int var334, int var791) {
		if (var334>var791)
			return (var334*var791 + stvr);
		else
			return (var791*var334 - stvr);
	}
}

class mthdcls210 {

	static int stvr;

	static mthdcls210() {
		stvr = 0;
	}

	public int method210 (int var325, int var718) {
		if (var325>var718)
			return (var325+var718 + stvr);
		else
			return (var718+var325 - stvr);
	}
}

class mthdcls211 {

	static int stvr;

	static mthdcls211() {
		stvr = 866;
	}

	public int method211 (int var502, int var767) {
		if (var502>var767)
			return (var502*var767 + stvr);
		else
			return (var767*var502 - stvr);
	}
}

class mthdcls212 {

	static int stvr;

	static mthdcls212() {
		stvr = 347;
	}

	public int method212 (int var84, int var863) {
		if (var84>var863)
			return (var84*var863 + stvr);
		else
			return (var863*var84 - stvr);
	}
}

class mthdcls213 {

	static int stvr;

	static mthdcls213() {
		stvr = 558;
	}

	public int method213 (int var415, int var259) {
		if (var415>var259)
			return (var415*var259 + stvr);
		else
			return (var259*var415 - stvr);
	}
}

class mthdcls214 {

	static int stvr;

	static mthdcls214() {
		stvr = 265;
	}

	public int method214 (int var985, int var37) {
		if (var985>var37)
			return (var985-var37 + stvr);
		else
			return (var37-var985 - stvr);
	}
}

class mthdcls215 {

	static int stvr;

	static mthdcls215() {
		stvr = 0;
	}

	public int method215 (int var643, int var145) {
		if (var643>var145)
			return (var643-var145 + stvr);
		else
			return (var145-var643 - stvr);
	}
}

class mthdcls216 {

	static int stvr;

	static mthdcls216() {
		stvr = 945;
	}

	public int method216 (int var817, int var565) {
		if (var817>var565)
			return (var817-var565 + stvr);
		else
			return (var565-var817 - stvr);
	}
}

class mthdcls217 {

	static int stvr;

	static mthdcls217() {
		stvr = 916;
	}

	public int method217 (int var941, int var958) {
		if (var941>var958)
			return (var941-var958 + stvr);
		else
			return (var958-var941 - stvr);
	}
}

class mthdcls218 {

	static int stvr;

	static mthdcls218() {
		stvr = 390;
	}

	public int method218 (int var851, int var747) {
		if (var851>var747)
			return (var851-var747 + stvr);
		else
			return (var747-var851 - stvr);
	}
}

class mthdcls219 {

	static int stvr;

	static mthdcls219() {
		stvr = 134;
	}

	public int method219 (int var32, int var926) {
		if (var32>var926)
			return (var32*var926 + stvr);
		else
			return (var926*var32 - stvr);
	}
}

class mthdcls220 {

	static int stvr;

	static mthdcls220() {
		stvr = 135;
	}

	public int method220 (int var189, int var790) {
		if (var189>var790)
			return (var189+var790 + stvr);
		else
			return (var790+var189 - stvr);
	}
}

class mthdcls221 {

	static int stvr;

	static mthdcls221() {
		stvr = 506;
	}

	public int method221 (int var759, int var772) {
		if (var759>var772)
			return (var759+var772 + stvr);
		else
			return (var772+var759 - stvr);
	}
}

class mthdcls222 {

	static int stvr;

	static mthdcls222() {
		stvr = 329;
	}

	public int method222 (int var544, int var851) {
		if (var544>var851)
			return (var544*var851 + stvr);
		else
			return (var851*var544 - stvr);
	}
}

class mthdcls223 {

	static int stvr;

	static mthdcls223() {
		stvr = 875;
	}

	public int method223 (int var336, int var472) {
		if (var336>var472)
			return (var336+var472 + stvr);
		else
			return (var472+var336 - stvr);
	}
}

class mthdcls224 {

	static int stvr;

	static mthdcls224() {
		stvr = 290;
	}

	public int method224 (int var488, int var124) {
		if (var488>var124)
			return (var488+var124 + stvr);
		else
			return (var124+var488 - stvr);
	}
}

class mthdcls225 {

	static int stvr;

	static mthdcls225() {
		stvr = 576;
	}

	public int method225 (int var195, int var901) {
		if (var195>var901)
			return (var195*var901 + stvr);
		else
			return (var901*var195 - stvr);
	}
}

class mthdcls226 {

	static int stvr;

	static mthdcls226() {
		stvr = 60;
	}

	public int method226 (int var870, int var639) {
		if (var870>var639)
			return (var870+var639 + stvr);
		else
			return (var639+var870 - stvr);
	}
}

class mthdcls227 {

	static int stvr;

	static mthdcls227() {
		stvr = 885;
	}

	public int method227 (int var966, int var235) {
		if (var966>var235)
			return (var966-var235 + stvr);
		else
			return (var235-var966 - stvr);
	}
}

class mthdcls228 {

	static int stvr;

	static mthdcls228() {
		stvr = 33;
	}

	public int method228 (int var468, int var82) {
		if (var468>var82)
			return (var468-var82 + stvr);
		else
			return (var82-var468 - stvr);
	}
}

class mthdcls229 {

	static int stvr;

	static mthdcls229() {
		stvr = 568;
	}

	public int method229 (int var727, int var456) {
		if (var727>var456)
			return (var727+var456 + stvr);
		else
			return (var456+var727 - stvr);
	}
}

class mthdcls230 {

	static int stvr;

	static mthdcls230() {
		stvr = 16;
	}

	public int method230 (int var130, int var350) {
		if (var130>var350)
			return (var130+var350 + stvr);
		else
			return (var350+var130 - stvr);
	}
}

class mthdcls231 {

	static int stvr;

	static mthdcls231() {
		stvr = 242;
	}

	public int method231 (int var549, int var729) {
		if (var549>var729)
			return (var549-var729 + stvr);
		else
			return (var729-var549 - stvr);
	}
}

class mthdcls232 {

	static int stvr;

	static mthdcls232() {
		stvr = 894;
	}

	public int method232 (int var523, int var887) {
		if (var523>var887)
			return (var523-var887 + stvr);
		else
			return (var887-var523 - stvr);
	}
}

class mthdcls233 {

	static int stvr;

	static mthdcls233() {
		stvr = 690;
	}

	public int method233 (int var461, int var817) {
		if (var461>var817)
			return (var461-var817 + stvr);
		else
			return (var817-var461 - stvr);
	}
}

class mthdcls234 {

	static int stvr;

	static mthdcls234() {
		stvr = 77;
	}

	public int method234 (int var880, int var904) {
		if (var880>var904)
			return (var880+var904 + stvr);
		else
			return (var904+var880 - stvr);
	}
}

class mthdcls235 {

	static int stvr;

	static mthdcls235() {
		stvr = 293;
	}

	public int method235 (int var137, int var107) {
		if (var137>var107)
			return (var137*var107 + stvr);
		else
			return (var107*var137 - stvr);
	}
}

class mthdcls236 {

	static int stvr;

	static mthdcls236() {
		stvr = 917;
	}

	public int method236 (int var465, int var658) {
		if (var465>var658)
			return (var465-var658 + stvr);
		else
			return (var658-var465 - stvr);
	}
}

class mthdcls237 {

	static int stvr;

	static mthdcls237() {
		stvr = 531;
	}

	public int method237 (int var983, int var744) {
		if (var983>var744)
			return (var983+var744 + stvr);
		else
			return (var744+var983 - stvr);
	}
}

class mthdcls238 {

	static int stvr;

	static mthdcls238() {
		stvr = 299;
	}

	public int method238 (int var149, int var545) {
		if (var149>var545)
			return (var149-var545 + stvr);
		else
			return (var545-var149 - stvr);
	}
}

class mthdcls239 {

	static int stvr;

	static mthdcls239() {
		stvr = 91;
	}

	public int method239 (int var563, int var5) {
		if (var563>var5)
			return (var563+var5 + stvr);
		else
			return (var5+var563 - stvr);
	}
}

class mthdcls240 {

	static int stvr;

	static mthdcls240() {
		stvr = 270;
	}

	public int method240 (int var619, int var162) {
		if (var619>var162)
			return (var619+var162 + stvr);
		else
			return (var162+var619 - stvr);
	}
}

class mthdcls241 {

	static int stvr;

	static mthdcls241() {
		stvr = 433;
	}

	public int method241 (int var471, int var433) {
		if (var471>var433)
			return (var471*var433 + stvr);
		else
			return (var433*var471 - stvr);
	}
}

class mthdcls242 {

	static int stvr;

	static mthdcls242() {
		stvr = 472;
	}

	public int method242 (int var804, int var198) {
		if (var804>var198)
			return (var804*var198 + stvr);
		else
			return (var198*var804 - stvr);
	}
}

class mthdcls243 {

	static int stvr;

	static mthdcls243() {
		stvr = 439;
	}

	public int method243 (int var978, int var587) {
		if (var978>var587)
			return (var978+var587 + stvr);
		else
			return (var587+var978 - stvr);
	}
}

class mthdcls244 {

	static int stvr;

	static mthdcls244() {
		stvr = 819;
	}

	public int method244 (int var456, int var726) {
		if (var456>var726)
			return (var456*var726 + stvr);
		else
			return (var726*var456 - stvr);
	}
}

class mthdcls245 {

	static int stvr;

	static mthdcls245() {
		stvr = 403;
	}

	public int method245 (int var718, int var634) {
		if (var718>var634)
			return (var718-var634 + stvr);
		else
			return (var634-var718 - stvr);
	}
}

class mthdcls246 {

	static int stvr;

	static mthdcls246() {
		stvr = 43;
	}

	public int method246 (int var704, int var674) {
		if (var704>var674)
			return (var704+var674 + stvr);
		else
			return (var674+var704 - stvr);
	}
}

class mthdcls247 {

	static int stvr;

	static mthdcls247() {
		stvr = 826;
	}

	public int method247 (int var266, int var185) {
		if (var266>var185)
			return (var266*var185 + stvr);
		else
			return (var185*var266 - stvr);
	}
}

class mthdcls248 {

	static int stvr;

	static mthdcls248() {
		stvr = 531;
	}

	public int method248 (int var395, int var305) {
		if (var395>var305)
			return (var395*var305 + stvr);
		else
			return (var305*var395 - stvr);
	}
}

class mthdcls249 {

	static int stvr;

	static mthdcls249() {
		stvr = 50;
	}

	public int method249 (int var423, int var725) {
		if (var423>var725)
			return (var423+var725 + stvr);
		else
			return (var725+var423 - stvr);
	}
}

class mthdcls250 {

	static int stvr;

	static mthdcls250() {
		stvr = 623;
	}

	public int method250 (int var929, int var602) {
		if (var929>var602)
			return (var929-var602 + stvr);
		else
			return (var602-var929 - stvr);
	}
}

class mthdcls251 {

	static int stvr;

	static mthdcls251() {
		stvr = 513;
	}

	public int method251 (int var945, int var119) {
		if (var945>var119)
			return (var945-var119 + stvr);
		else
			return (var119-var945 - stvr);
	}
}

class mthdcls252 {

	static int stvr;

	static mthdcls252() {
		stvr = 234;
	}

	public int method252 (int var285, int var607) {
		if (var285>var607)
			return (var285-var607 + stvr);
		else
			return (var607-var285 - stvr);
	}
}

class mthdcls253 {

	static int stvr;

	static mthdcls253() {
		stvr = 262;
	}

	public int method253 (int var498, int var666) {
		if (var498>var666)
			return (var498+var666 + stvr);
		else
			return (var666+var498 - stvr);
	}
}

class mthdcls254 {

	static int stvr;

	static mthdcls254() {
		stvr = 519;
	}

	public int method254 (int var252, int var536) {
		if (var252>var536)
			return (var252*var536 + stvr);
		else
			return (var536*var252 - stvr);
	}
}

class mthdcls255 {

	static int stvr;

	static mthdcls255() {
		stvr = 243;
	}

	public int method255 (int var853, int var102) {
		if (var853>var102)
			return (var853-var102 + stvr);
		else
			return (var102-var853 - stvr);
	}
}

class mthdcls256 {

	static int stvr;

	static mthdcls256() {
		stvr = 50;
	}

	public int method256 (int var598, int var121) {
		if (var598>var121)
			return (var598*var121 + stvr);
		else
			return (var121*var598 - stvr);
	}
}

class mthdcls257 {

	static int stvr;

	static mthdcls257() {
		stvr = 562;
	}

	public int method257 (int var97, int var439) {
		if (var97>var439)
			return (var97-var439 + stvr);
		else
			return (var439-var97 - stvr);
	}
}

class mthdcls258 {

	static int stvr;

	static mthdcls258() {
		stvr = 847;
	}

	public int method258 (int var599, int var923) {
		if (var599>var923)
			return (var599*var923 + stvr);
		else
			return (var923*var599 - stvr);
	}
}

class mthdcls259 {

	static int stvr;

	static mthdcls259() {
		stvr = 343;
	}

	public int method259 (int var859, int var785) {
		if (var859>var785)
			return (var859-var785 + stvr);
		else
			return (var785-var859 - stvr);
	}
}

class mthdcls260 {

	static int stvr;

	static mthdcls260() {
		stvr = 433;
	}

	public int method260 (int var320, int var482) {
		if (var320>var482)
			return (var320*var482 + stvr);
		else
			return (var482*var320 - stvr);
	}
}

class mthdcls261 {

	static int stvr;

	static mthdcls261() {
		stvr = 362;
	}

	public int method261 (int var807, int var552) {
		if (var807>var552)
			return (var807-var552 + stvr);
		else
			return (var552-var807 - stvr);
	}
}

class mthdcls262 {

	static int stvr;

	static mthdcls262() {
		stvr = 901;
	}

	public int method262 (int var506, int var556) {
		if (var506>var556)
			return (var506-var556 + stvr);
		else
			return (var556-var506 - stvr);
	}
}

class mthdcls263 {

	static int stvr;

	static mthdcls263() {
		stvr = 998;
	}

	public int method263 (int var361, int var759) {
		if (var361>var759)
			return (var361+var759 + stvr);
		else
			return (var759+var361 - stvr);
	}
}

class mthdcls264 {

	static int stvr;

	static mthdcls264() {
		stvr = 681;
	}

	public int method264 (int var712, int var323) {
		if (var712>var323)
			return (var712+var323 + stvr);
		else
			return (var323+var712 - stvr);
	}
}

class mthdcls265 {

	static int stvr;

	static mthdcls265() {
		stvr = 191;
	}

	public int method265 (int var770, int var103) {
		if (var770>var103)
			return (var770+var103 + stvr);
		else
			return (var103+var770 - stvr);
	}
}

class mthdcls266 {

	static int stvr;

	static mthdcls266() {
		stvr = 677;
	}

	public int method266 (int var301, int var739) {
		if (var301>var739)
			return (var301*var739 + stvr);
		else
			return (var739*var301 - stvr);
	}
}

class mthdcls267 {

	static int stvr;

	static mthdcls267() {
		stvr = 517;
	}

	public int method267 (int var42, int var220) {
		if (var42>var220)
			return (var42-var220 + stvr);
		else
			return (var220-var42 - stvr);
	}
}

class mthdcls268 {

	static int stvr;

	static mthdcls268() {
		stvr = 598;
	}

	public int method268 (int var338, int var440) {
		if (var338>var440)
			return (var338*var440 + stvr);
		else
			return (var440*var338 - stvr);
	}
}

class mthdcls269 {

	static int stvr;

	static mthdcls269() {
		stvr = 127;
	}

	public int method269 (int var276, int var242) {
		if (var276>var242)
			return (var276-var242 + stvr);
		else
			return (var242-var276 - stvr);
	}
}

class mthdcls270 {

	static int stvr;

	static mthdcls270() {
		stvr = 369;
	}

	public int method270 (int var755, int var594) {
		if (var755>var594)
			return (var755+var594 + stvr);
		else
			return (var594+var755 - stvr);
	}
}

class mthdcls271 {

	static int stvr;

	static mthdcls271() {
		stvr = 350;
	}

	public int method271 (int var580, int var804) {
		if (var580>var804)
			return (var580*var804 + stvr);
		else
			return (var804*var580 - stvr);
	}
}

class mthdcls272 {

	static int stvr;

	static mthdcls272() {
		stvr = 710;
	}

	public int method272 (int var587, int var34) {
		if (var587>var34)
			return (var587+var34 + stvr);
		else
			return (var34+var587 - stvr);
	}
}

class mthdcls273 {

	static int stvr;

	static mthdcls273() {
		stvr = 575;
	}

	public int method273 (int var65, int var958) {
		if (var65>var958)
			return (var65-var958 + stvr);
		else
			return (var958-var65 - stvr);
	}
}

class mthdcls274 {

	static int stvr;

	static mthdcls274() {
		stvr = 403;
	}

	public int method274 (int var118, int var632) {
		if (var118>var632)
			return (var118*var632 + stvr);
		else
			return (var632*var118 - stvr);
	}
}

class mthdcls275 {

	static int stvr;

	static mthdcls275() {
		stvr = 922;
	}

	public int method275 (int var118, int var953) {
		if (var118>var953)
			return (var118+var953 + stvr);
		else
			return (var953+var118 - stvr);
	}
}

class mthdcls276 {

	static int stvr;

	static mthdcls276() {
		stvr = 946;
	}

	public int method276 (int var965, int var752) {
		if (var965>var752)
			return (var965+var752 + stvr);
		else
			return (var752+var965 - stvr);
	}
}

class mthdcls277 {

	static int stvr;

	static mthdcls277() {
		stvr = 760;
	}

	public int method277 (int var267, int var29) {
		if (var267>var29)
			return (var267+var29 + stvr);
		else
			return (var29+var267 - stvr);
	}
}

class mthdcls278 {

	static int stvr;

	static mthdcls278() {
		stvr = 94;
	}

	public int method278 (int var83, int var644) {
		if (var83>var644)
			return (var83*var644 + stvr);
		else
			return (var644*var83 - stvr);
	}
}

class mthdcls279 {

	static int stvr;

	static mthdcls279() {
		stvr = 380;
	}

	public int method279 (int var705, int var36) {
		if (var705>var36)
			return (var705+var36 + stvr);
		else
			return (var36+var705 - stvr);
	}
}

class mthdcls280 {

	static int stvr;

	static mthdcls280() {
		stvr = 362;
	}

	public int method280 (int var322, int var320) {
		if (var322>var320)
			return (var322-var320 + stvr);
		else
			return (var320-var322 - stvr);
	}
}

class mthdcls281 {

	static int stvr;

	static mthdcls281() {
		stvr = 565;
	}

	public int method281 (int var3, int var647) {
		if (var3>var647)
			return (var3-var647 + stvr);
		else
			return (var647-var3 - stvr);
	}
}

class mthdcls282 {

	static int stvr;

	static mthdcls282() {
		stvr = 464;
	}

	public int method282 (int var550, int var43) {
		if (var550>var43)
			return (var550*var43 + stvr);
		else
			return (var43*var550 - stvr);
	}
}

class mthdcls283 {

	static int stvr;

	static mthdcls283() {
		stvr = 388;
	}

	public int method283 (int var942, int var940) {
		if (var942>var940)
			return (var942-var940 + stvr);
		else
			return (var940-var942 - stvr);
	}
}

class mthdcls284 {

	static int stvr;

	static mthdcls284() {
		stvr = 779;
	}

	public int method284 (int var29, int var578) {
		if (var29>var578)
			return (var29-var578 + stvr);
		else
			return (var578-var29 - stvr);
	}
}

class mthdcls285 {

	static int stvr;

	static mthdcls285() {
		stvr = 870;
	}

	public int method285 (int var798, int var269) {
		if (var798>var269)
			return (var798-var269 + stvr);
		else
			return (var269-var798 - stvr);
	}
}

class mthdcls286 {

	static int stvr;

	static mthdcls286() {
		stvr = 398;
	}

	public int method286 (int var470, int var388) {
		if (var470>var388)
			return (var470*var388 + stvr);
		else
			return (var388*var470 - stvr);
	}
}

class mthdcls287 {

	static int stvr;

	static mthdcls287() {
		stvr = 719;
	}

	public int method287 (int var922, int var287) {
		if (var922>var287)
			return (var922+var287 + stvr);
		else
			return (var287+var922 - stvr);
	}
}

class mthdcls288 {

	static int stvr;

	static mthdcls288() {
		stvr = 373;
	}

	public int method288 (int var327, int var641) {
		if (var327>var641)
			return (var327-var641 + stvr);
		else
			return (var641-var327 - stvr);
	}
}

class mthdcls289 {

	static int stvr;

	static mthdcls289() {
		stvr = 660;
	}

	public int method289 (int var505, int var865) {
		if (var505>var865)
			return (var505*var865 + stvr);
		else
			return (var865*var505 - stvr);
	}
}

class mthdcls290 {

	static int stvr;

	static mthdcls290() {
		stvr = 942;
	}

	public int method290 (int var436, int var166) {
		if (var436>var166)
			return (var436*var166 + stvr);
		else
			return (var166*var436 - stvr);
	}
}

class mthdcls291 {

	static int stvr;

	static mthdcls291() {
		stvr = 287;
	}

	public int method291 (int var934, int var922) {
		if (var934>var922)
			return (var934*var922 + stvr);
		else
			return (var922*var934 - stvr);
	}
}

class mthdcls292 {

	static int stvr;

	static mthdcls292() {
		stvr = 849;
	}

	public int method292 (int var715, int var927) {
		if (var715>var927)
			return (var715*var927 + stvr);
		else
			return (var927*var715 - stvr);
	}
}

class mthdcls293 {

	static int stvr;

	static mthdcls293() {
		stvr = 68;
	}

	public int method293 (int var909, int var669) {
		if (var909>var669)
			return (var909*var669 + stvr);
		else
			return (var669*var909 - stvr);
	}
}

class mthdcls294 {

	static int stvr;

	static mthdcls294() {
		stvr = 74;
	}

	public int method294 (int var77, int var280) {
		if (var77>var280)
			return (var77-var280 + stvr);
		else
			return (var280-var77 - stvr);
	}
}

class mthdcls295 {

	static int stvr;

	static mthdcls295() {
		stvr = 510;
	}

	public int method295 (int var862, int var635) {
		if (var862>var635)
			return (var862+var635 + stvr);
		else
			return (var635+var862 - stvr);
	}
}

class mthdcls296 {

	static int stvr;

	static mthdcls296() {
		stvr = 847;
	}

	public int method296 (int var876, int var981) {
		if (var876>var981)
			return (var876-var981 + stvr);
		else
			return (var981-var876 - stvr);
	}
}

class mthdcls297 {

	static int stvr;

	static mthdcls297() {
		stvr = 166;
	}

	public int method297 (int var542, int var539) {
		if (var542>var539)
			return (var542+var539 + stvr);
		else
			return (var539+var542 - stvr);
	}
}

class mthdcls298 {

	static int stvr;

	static mthdcls298() {
		stvr = 482;
	}

	public int method298 (int var252, int var219) {
		if (var252>var219)
			return (var252*var219 + stvr);
		else
			return (var219*var252 - stvr);
	}
}

class mthdcls299 {

	static int stvr;

	static mthdcls299() {
		stvr = 718;
	}

	public int method299 (int var47, int var566) {
		if (var47>var566)
			return (var47-var566 + stvr);
		else
			return (var566-var47 - stvr);
	}
}

class mthdcls300 {

	static int stvr;

	static mthdcls300() {
		stvr = 746;
	}

	public int method300 (int var870, int var355) {
		if (var870>var355)
			return (var870*var355 + stvr);
		else
			return (var355*var870 - stvr);
	}
}

class mthdcls301 {

	static int stvr;

	static mthdcls301() {
		stvr = 363;
	}

	public int method301 (int var454, int var318) {
		if (var454>var318)
			return (var454-var318 + stvr);
		else
			return (var318-var454 - stvr);
	}
}

class mthdcls302 {

	static int stvr;

	static mthdcls302() {
		stvr = 574;
	}

	public int method302 (int var394, int var755) {
		if (var394>var755)
			return (var394-var755 + stvr);
		else
			return (var755-var394 - stvr);
	}
}

class mthdcls303 {

	static int stvr;

	static mthdcls303() {
		stvr = 714;
	}

	public int method303 (int var496, int var445) {
		if (var496>var445)
			return (var496*var445 + stvr);
		else
			return (var445*var496 - stvr);
	}
}

class mthdcls304 {

	static int stvr;

	static mthdcls304() {
		stvr = 727;
	}

	public int method304 (int var343, int var950) {
		if (var343>var950)
			return (var343*var950 + stvr);
		else
			return (var950*var343 - stvr);
	}
}

class mthdcls305 {

	static int stvr;

	static mthdcls305() {
		stvr = 835;
	}

	public int method305 (int var722, int var533) {
		if (var722>var533)
			return (var722+var533 + stvr);
		else
			return (var533+var722 - stvr);
	}
}

class mthdcls306 {

	static int stvr;

	static mthdcls306() {
		stvr = 51;
	}

	public int method306 (int var543, int var272) {
		if (var543>var272)
			return (var543+var272 + stvr);
		else
			return (var272+var543 - stvr);
	}
}

class mthdcls307 {

	static int stvr;

	static mthdcls307() {
		stvr = 425;
	}

	public int method307 (int var120, int var407) {
		if (var120>var407)
			return (var120+var407 + stvr);
		else
			return (var407+var120 - stvr);
	}
}

class mthdcls308 {

	static int stvr;

	static mthdcls308() {
		stvr = 147;
	}

	public int method308 (int var96, int var824) {
		if (var96>var824)
			return (var96-var824 + stvr);
		else
			return (var824-var96 - stvr);
	}
}

class mthdcls309 {

	static int stvr;

	static mthdcls309() {
		stvr = 370;
	}

	public int method309 (int var301, int var491) {
		if (var301>var491)
			return (var301-var491 + stvr);
		else
			return (var491-var301 - stvr);
	}
}

class mthdcls310 {

	static int stvr;

	static mthdcls310() {
		stvr = 919;
	}

	public int method310 (int var513, int var730) {
		if (var513>var730)
			return (var513+var730 + stvr);
		else
			return (var730+var513 - stvr);
	}
}

class mthdcls311 {

	static int stvr;

	static mthdcls311() {
		stvr = 809;
	}

	public int method311 (int var597, int var303) {
		if (var597>var303)
			return (var597-var303 + stvr);
		else
			return (var303-var597 - stvr);
	}
}

class mthdcls312 {

	static int stvr;

	static mthdcls312() {
		stvr = 389;
	}

	public int method312 (int var47, int var893) {
		if (var47>var893)
			return (var47*var893 + stvr);
		else
			return (var893*var47 - stvr);
	}
}

class mthdcls313 {

	static int stvr;

	static mthdcls313() {
		stvr = 175;
	}

	public int method313 (int var570, int var608) {
		if (var570>var608)
			return (var570*var608 + stvr);
		else
			return (var608*var570 - stvr);
	}
}

class mthdcls314 {

	static int stvr;

	static mthdcls314() {
		stvr = 291;
	}

	public int method314 (int var4, int var75) {
		if (var4>var75)
			return (var4-var75 + stvr);
		else
			return (var75-var4 - stvr);
	}
}

class mthdcls315 {

	static int stvr;

	static mthdcls315() {
		stvr = 985;
	}

	public int method315 (int var613, int var31) {
		if (var613>var31)
			return (var613-var31 + stvr);
		else
			return (var31-var613 - stvr);
	}
}

class mthdcls316 {

	static int stvr;

	static mthdcls316() {
		stvr = 274;
	}

	public int method316 (int var418, int var724) {
		if (var418>var724)
			return (var418*var724 + stvr);
		else
			return (var724*var418 - stvr);
	}
}

class mthdcls317 {

	static int stvr;

	static mthdcls317() {
		stvr = 907;
	}

	public int method317 (int var650, int var882) {
		if (var650>var882)
			return (var650*var882 + stvr);
		else
			return (var882*var650 - stvr);
	}
}

class mthdcls318 {

	static int stvr;

	static mthdcls318() {
		stvr = 260;
	}

	public int method318 (int var512, int var231) {
		if (var512>var231)
			return (var512+var231 + stvr);
		else
			return (var231+var512 - stvr);
	}
}

class mthdcls319 {

	static int stvr;

	static mthdcls319() {
		stvr = 621;
	}

	public int method319 (int var21, int var533) {
		if (var21>var533)
			return (var21-var533 + stvr);
		else
			return (var533-var21 - stvr);
	}
}

class mthdcls320 {

	static int stvr;

	static mthdcls320() {
		stvr = 840;
	}

	public int method320 (int var270, int var506) {
		if (var270>var506)
			return (var270*var506 + stvr);
		else
			return (var506*var270 - stvr);
	}
}

class mthdcls321 {

	static int stvr;

	static mthdcls321() {
		stvr = 854;
	}

	public int method321 (int var788, int var455) {
		if (var788>var455)
			return (var788*var455 + stvr);
		else
			return (var455*var788 - stvr);
	}
}

class mthdcls322 {

	static int stvr;

	static mthdcls322() {
		stvr = 753;
	}

	public int method322 (int var715, int var395) {
		if (var715>var395)
			return (var715-var395 + stvr);
		else
			return (var395-var715 - stvr);
	}
}

class mthdcls323 {

	static int stvr;

	static mthdcls323() {
		stvr = 719;
	}

	public int method323 (int var816, int var633) {
		if (var816>var633)
			return (var816+var633 + stvr);
		else
			return (var633+var816 - stvr);
	}
}

class mthdcls324 {

	static int stvr;

	static mthdcls324() {
		stvr = 579;
	}

	public int method324 (int var37, int var986) {
		if (var37>var986)
			return (var37-var986 + stvr);
		else
			return (var986-var37 - stvr);
	}
}

class mthdcls325 {

	static int stvr;

	static mthdcls325() {
		stvr = 982;
	}

	public int method325 (int var498, int var234) {
		if (var498>var234)
			return (var498-var234 + stvr);
		else
			return (var234-var498 - stvr);
	}
}

class mthdcls326 {

	static int stvr;

	static mthdcls326() {
		stvr = 794;
	}

	public int method326 (int var157, int var176) {
		if (var157>var176)
			return (var157-var176 + stvr);
		else
			return (var176-var157 - stvr);
	}
}

class mthdcls327 {

	static int stvr;

	static mthdcls327() {
		stvr = 823;
	}

	public int method327 (int var23, int var971) {
		if (var23>var971)
			return (var23+var971 + stvr);
		else
			return (var971+var23 - stvr);
	}
}

class mthdcls328 {

	static int stvr;

	static mthdcls328() {
		stvr = 74;
	}

	public int method328 (int var17, int var163) {
		if (var17>var163)
			return (var17*var163 + stvr);
		else
			return (var163*var17 - stvr);
	}
}

class mthdcls329 {

	static int stvr;

	static mthdcls329() {
		stvr = 738;
	}

	public int method329 (int var525, int var652) {
		if (var525>var652)
			return (var525-var652 + stvr);
		else
			return (var652-var525 - stvr);
	}
}

class mthdcls330 {

	static int stvr;

	static mthdcls330() {
		stvr = 167;
	}

	public int method330 (int var786, int var550) {
		if (var786>var550)
			return (var786-var550 + stvr);
		else
			return (var550-var786 - stvr);
	}
}

class mthdcls331 {

	static int stvr;

	static mthdcls331() {
		stvr = 242;
	}

	public int method331 (int var93, int var711) {
		if (var93>var711)
			return (var93+var711 + stvr);
		else
			return (var711+var93 - stvr);
	}
}

class mthdcls332 {

	static int stvr;

	static mthdcls332() {
		stvr = 14;
	}

	public int method332 (int var817, int var632) {
		if (var817>var632)
			return (var817-var632 + stvr);
		else
			return (var632-var817 - stvr);
	}
}

class mthdcls333 {

	static int stvr;

	static mthdcls333() {
		stvr = 556;
	}

	public int method333 (int var551, int var321) {
		if (var551>var321)
			return (var551-var321 + stvr);
		else
			return (var321-var551 - stvr);
	}
}

class mthdcls334 {

	static int stvr;

	static mthdcls334() {
		stvr = 82;
	}

	public int method334 (int var164, int var894) {
		if (var164>var894)
			return (var164*var894 + stvr);
		else
			return (var894*var164 - stvr);
	}
}

class mthdcls335 {

	static int stvr;

	static mthdcls335() {
		stvr = 995;
	}

	public int method335 (int var486, int var819) {
		if (var486>var819)
			return (var486*var819 + stvr);
		else
			return (var819*var486 - stvr);
	}
}

class mthdcls336 {

	static int stvr;

	static mthdcls336() {
		stvr = 72;
	}

	public int method336 (int var786, int var991) {
		if (var786>var991)
			return (var786-var991 + stvr);
		else
			return (var991-var786 - stvr);
	}
}

class mthdcls337 {

	static int stvr;

	static mthdcls337() {
		stvr = 340;
	}

	public int method337 (int var524, int var162) {
		if (var524>var162)
			return (var524+var162 + stvr);
		else
			return (var162+var524 - stvr);
	}
}

class mthdcls338 {

	static int stvr;

	static mthdcls338() {
		stvr = 443;
	}

	public int method338 (int var701, int var414) {
		if (var701>var414)
			return (var701*var414 + stvr);
		else
			return (var414*var701 - stvr);
	}
}

class mthdcls339 {

	static int stvr;

	static mthdcls339() {
		stvr = 52;
	}

	public int method339 (int var122, int var80) {
		if (var122>var80)
			return (var122*var80 + stvr);
		else
			return (var80*var122 - stvr);
	}
}

class mthdcls340 {

	static int stvr;

	static mthdcls340() {
		stvr = 704;
	}

	public int method340 (int var706, int var656) {
		if (var706>var656)
			return (var706+var656 + stvr);
		else
			return (var656+var706 - stvr);
	}
}

class mthdcls341 {

	static int stvr;

	static mthdcls341() {
		stvr = 763;
	}

	public int method341 (int var794, int var478) {
		if (var794>var478)
			return (var794-var478 + stvr);
		else
			return (var478-var794 - stvr);
	}
}

class mthdcls342 {

	static int stvr;

	static mthdcls342() {
		stvr = 559;
	}

	public int method342 (int var930, int var371) {
		if (var930>var371)
			return (var930*var371 + stvr);
		else
			return (var371*var930 - stvr);
	}
}

class mthdcls343 {

	static int stvr;

	static mthdcls343() {
		stvr = 871;
	}

	public int method343 (int var403, int var189) {
		if (var403>var189)
			return (var403+var189 + stvr);
		else
			return (var189+var403 - stvr);
	}
}

class mthdcls344 {

	static int stvr;

	static mthdcls344() {
		stvr = 433;
	}

	public int method344 (int var471, int var268) {
		if (var471>var268)
			return (var471+var268 + stvr);
		else
			return (var268+var471 - stvr);
	}
}

class mthdcls345 {

	static int stvr;

	static mthdcls345() {
		stvr = 386;
	}

	public int method345 (int var507, int var190) {
		if (var507>var190)
			return (var507+var190 + stvr);
		else
			return (var190+var507 - stvr);
	}
}

class mthdcls346 {

	static int stvr;

	static mthdcls346() {
		stvr = 606;
	}

	public int method346 (int var8, int var55) {
		if (var8>var55)
			return (var8*var55 + stvr);
		else
			return (var55*var8 - stvr);
	}
}

class mthdcls347 {

	static int stvr;

	static mthdcls347() {
		stvr = 7;
	}

	public int method347 (int var415, int var432) {
		if (var415>var432)
			return (var415*var432 + stvr);
		else
			return (var432*var415 - stvr);
	}
}

class mthdcls348 {

	static int stvr;

	static mthdcls348() {
		stvr = 456;
	}

	public int method348 (int var334, int var291) {
		if (var334>var291)
			return (var334+var291 + stvr);
		else
			return (var291+var334 - stvr);
	}
}

class mthdcls349 {

	static int stvr;

	static mthdcls349() {
		stvr = 649;
	}

	public int method349 (int var433, int var980) {
		if (var433>var980)
			return (var433*var980 + stvr);
		else
			return (var980*var433 - stvr);
	}
}

class mthdcls350 {

	static int stvr;

	static mthdcls350() {
		stvr = 56;
	}

	public int method350 (int var931, int var694) {
		if (var931>var694)
			return (var931+var694 + stvr);
		else
			return (var694+var931 - stvr);
	}
}

class mthdcls351 {

	static int stvr;

	static mthdcls351() {
		stvr = 632;
	}

	public int method351 (int var650, int var50) {
		if (var650>var50)
			return (var650+var50 + stvr);
		else
			return (var50+var650 - stvr);
	}
}

class mthdcls352 {

	static int stvr;

	static mthdcls352() {
		stvr = 707;
	}

	public int method352 (int var362, int var470) {
		if (var362>var470)
			return (var362-var470 + stvr);
		else
			return (var470-var362 - stvr);
	}
}

class mthdcls353 {

	static int stvr;

	static mthdcls353() {
		stvr = 668;
	}

	public int method353 (int var639, int var914) {
		if (var639>var914)
			return (var639+var914 + stvr);
		else
			return (var914+var639 - stvr);
	}
}

class mthdcls354 {

	static int stvr;

	static mthdcls354() {
		stvr = 720;
	}

	public int method354 (int var422, int var540) {
		if (var422>var540)
			return (var422+var540 + stvr);
		else
			return (var540+var422 - stvr);
	}
}

class mthdcls355 {

	static int stvr;

	static mthdcls355() {
		stvr = 589;
	}

	public int method355 (int var777, int var212) {
		if (var777>var212)
			return (var777*var212 + stvr);
		else
			return (var212*var777 - stvr);
	}
}

class mthdcls356 {

	static int stvr;

	static mthdcls356() {
		stvr = 699;
	}

	public int method356 (int var456, int var558) {
		if (var456>var558)
			return (var456-var558 + stvr);
		else
			return (var558-var456 - stvr);
	}
}

class mthdcls357 {

	static int stvr;

	static mthdcls357() {
		stvr = 10;
	}

	public int method357 (int var538, int var348) {
		if (var538>var348)
			return (var538-var348 + stvr);
		else
			return (var348-var538 - stvr);
	}
}

class mthdcls358 {

	static int stvr;

	static mthdcls358() {
		stvr = 318;
	}

	public int method358 (int var500, int var763) {
		if (var500>var763)
			return (var500*var763 + stvr);
		else
			return (var763*var500 - stvr);
	}
}

class mthdcls359 {

	static int stvr;

	static mthdcls359() {
		stvr = 637;
	}

	public int method359 (int var794, int var570) {
		if (var794>var570)
			return (var794*var570 + stvr);
		else
			return (var570*var794 - stvr);
	}
}

class mthdcls360 {

	static int stvr;

	static mthdcls360() {
		stvr = 431;
	}

	public int method360 (int var220, int var391) {
		if (var220>var391)
			return (var220+var391 + stvr);
		else
			return (var391+var220 - stvr);
	}
}

class mthdcls361 {

	static int stvr;

	static mthdcls361() {
		stvr = 670;
	}

	public int method361 (int var373, int var994) {
		if (var373>var994)
			return (var373+var994 + stvr);
		else
			return (var994+var373 - stvr);
	}
}

class mthdcls362 {

	static int stvr;

	static mthdcls362() {
		stvr = 188;
	}

	public int method362 (int var302, int var39) {
		if (var302>var39)
			return (var302*var39 + stvr);
		else
			return (var39*var302 - stvr);
	}
}

class mthdcls363 {

	static int stvr;

	static mthdcls363() {
		stvr = 565;
	}

	public int method363 (int var598, int var151) {
		if (var598>var151)
			return (var598*var151 + stvr);
		else
			return (var151*var598 - stvr);
	}
}

class mthdcls364 {

	static int stvr;

	static mthdcls364() {
		stvr = 637;
	}

	public int method364 (int var68, int var276) {
		if (var68>var276)
			return (var68*var276 + stvr);
		else
			return (var276*var68 - stvr);
	}
}

class mthdcls365 {

	static int stvr;

	static mthdcls365() {
		stvr = 910;
	}

	public int method365 (int var31, int var108) {
		if (var31>var108)
			return (var31*var108 + stvr);
		else
			return (var108*var31 - stvr);
	}
}

class mthdcls366 {

	static int stvr;

	static mthdcls366() {
		stvr = 661;
	}

	public int method366 (int var782, int var542) {
		if (var782>var542)
			return (var782*var542 + stvr);
		else
			return (var542*var782 - stvr);
	}
}

class mthdcls367 {

	static int stvr;

	static mthdcls367() {
		stvr = 382;
	}

	public int method367 (int var417, int var370) {
		if (var417>var370)
			return (var417*var370 + stvr);
		else
			return (var370*var417 - stvr);
	}
}

class mthdcls368 {

	static int stvr;

	static mthdcls368() {
		stvr = 783;
	}

	public int method368 (int var387, int var782) {
		if (var387>var782)
			return (var387-var782 + stvr);
		else
			return (var782-var387 - stvr);
	}
}

class mthdcls369 {

	static int stvr;

	static mthdcls369() {
		stvr = 227;
	}

	public int method369 (int var223, int var125) {
		if (var223>var125)
			return (var223-var125 + stvr);
		else
			return (var125-var223 - stvr);
	}
}

class mthdcls370 {

	static int stvr;

	static mthdcls370() {
		stvr = 426;
	}

	public int method370 (int var686, int var660) {
		if (var686>var660)
			return (var686-var660 + stvr);
		else
			return (var660-var686 - stvr);
	}
}

class mthdcls371 {

	static int stvr;

	static mthdcls371() {
		stvr = 252;
	}

	public int method371 (int var677, int var729) {
		if (var677>var729)
			return (var677+var729 + stvr);
		else
			return (var729+var677 - stvr);
	}
}

class mthdcls372 {

	static int stvr;

	static mthdcls372() {
		stvr = 394;
	}

	public int method372 (int var3, int var611) {
		if (var3>var611)
			return (var3-var611 + stvr);
		else
			return (var611-var3 - stvr);
	}
}

class mthdcls373 {

	static int stvr;

	static mthdcls373() {
		stvr = 577;
	}

	public int method373 (int var519, int var256) {
		if (var519>var256)
			return (var519*var256 + stvr);
		else
			return (var256*var519 - stvr);
	}
}

class mthdcls374 {

	static int stvr;

	static mthdcls374() {
		stvr = 118;
	}

	public int method374 (int var473, int var923) {
		if (var473>var923)
			return (var473+var923 + stvr);
		else
			return (var923+var473 - stvr);
	}
}

class mthdcls375 {

	static int stvr;

	static mthdcls375() {
		stvr = 93;
	}

	public int method375 (int var408, int var850) {
		if (var408>var850)
			return (var408*var850 + stvr);
		else
			return (var850*var408 - stvr);
	}
}

class mthdcls376 {

	static int stvr;

	static mthdcls376() {
		stvr = 139;
	}

	public int method376 (int var302, int var855) {
		if (var302>var855)
			return (var302*var855 + stvr);
		else
			return (var855*var302 - stvr);
	}
}

class mthdcls377 {

	static int stvr;

	static mthdcls377() {
		stvr = 51;
	}

	public int method377 (int var170, int var148) {
		if (var170>var148)
			return (var170-var148 + stvr);
		else
			return (var148-var170 - stvr);
	}
}

class mthdcls378 {

	static int stvr;

	static mthdcls378() {
		stvr = 366;
	}

	public int method378 (int var160, int var792) {
		if (var160>var792)
			return (var160-var792 + stvr);
		else
			return (var792-var160 - stvr);
	}
}

class mthdcls379 {

	static int stvr;

	static mthdcls379() {
		stvr = 894;
	}

	public int method379 (int var463, int var664) {
		if (var463>var664)
			return (var463-var664 + stvr);
		else
			return (var664-var463 - stvr);
	}
}

class mthdcls380 {

	static int stvr;

	static mthdcls380() {
		stvr = 951;
	}

	public int method380 (int var372, int var31) {
		if (var372>var31)
			return (var372-var31 + stvr);
		else
			return (var31-var372 - stvr);
	}
}

class mthdcls381 {

	static int stvr;

	static mthdcls381() {
		stvr = 405;
	}

	public int method381 (int var830, int var520) {
		if (var830>var520)
			return (var830*var520 + stvr);
		else
			return (var520*var830 - stvr);
	}
}

class mthdcls382 {

	static int stvr;

	static mthdcls382() {
		stvr = 724;
	}

	public int method382 (int var529, int var677) {
		if (var529>var677)
			return (var529*var677 + stvr);
		else
			return (var677*var529 - stvr);
	}
}

class mthdcls383 {

	static int stvr;

	static mthdcls383() {
		stvr = 4;
	}

	public int method383 (int var210, int var245) {
		if (var210>var245)
			return (var210+var245 + stvr);
		else
			return (var245+var210 - stvr);
	}
}

class mthdcls384 {

	static int stvr;

	static mthdcls384() {
		stvr = 112;
	}

	public int method384 (int var854, int var362) {
		if (var854>var362)
			return (var854*var362 + stvr);
		else
			return (var362*var854 - stvr);
	}
}

class mthdcls385 {

	static int stvr;

	static mthdcls385() {
		stvr = 687;
	}

	public int method385 (int var441, int var971) {
		if (var441>var971)
			return (var441+var971 + stvr);
		else
			return (var971+var441 - stvr);
	}
}

class mthdcls386 {

	static int stvr;

	static mthdcls386() {
		stvr = 32;
	}

	public int method386 (int var887, int var445) {
		if (var887>var445)
			return (var887-var445 + stvr);
		else
			return (var445-var887 - stvr);
	}
}

class mthdcls387 {

	static int stvr;

	static mthdcls387() {
		stvr = 887;
	}

	public int method387 (int var624, int var130) {
		if (var624>var130)
			return (var624+var130 + stvr);
		else
			return (var130+var624 - stvr);
	}
}

class mthdcls388 {

	static int stvr;

	static mthdcls388() {
		stvr = 286;
	}

	public int method388 (int var925, int var143) {
		if (var925>var143)
			return (var925-var143 + stvr);
		else
			return (var143-var925 - stvr);
	}
}

class mthdcls389 {

	static int stvr;

	static mthdcls389() {
		stvr = 242;
	}

	public int method389 (int var798, int var680) {
		if (var798>var680)
			return (var798+var680 + stvr);
		else
			return (var680+var798 - stvr);
	}
}

class mthdcls390 {

	static int stvr;

	static mthdcls390() {
		stvr = 41;
	}

	public int method390 (int var491, int var977) {
		if (var491>var977)
			return (var491*var977 + stvr);
		else
			return (var977*var491 - stvr);
	}
}

class mthdcls391 {

	static int stvr;

	static mthdcls391() {
		stvr = 320;
	}

	public int method391 (int var927, int var998) {
		if (var927>var998)
			return (var927+var998 + stvr);
		else
			return (var998+var927 - stvr);
	}
}

class mthdcls392 {

	static int stvr;

	static mthdcls392() {
		stvr = 413;
	}

	public int method392 (int var699, int var633) {
		if (var699>var633)
			return (var699+var633 + stvr);
		else
			return (var633+var699 - stvr);
	}
}

class mthdcls393 {

	static int stvr;

	static mthdcls393() {
		stvr = 384;
	}

	public int method393 (int var385, int var391) {
		if (var385>var391)
			return (var385-var391 + stvr);
		else
			return (var391-var385 - stvr);
	}
}

class mthdcls394 {

	static int stvr;

	static mthdcls394() {
		stvr = 129;
	}

	public int method394 (int var530, int var2) {
		if (var530>var2)
			return (var530-var2 + stvr);
		else
			return (var2-var530 - stvr);
	}
}

class mthdcls395 {

	static int stvr;

	static mthdcls395() {
		stvr = 963;
	}

	public int method395 (int var876, int var321) {
		if (var876>var321)
			return (var876*var321 + stvr);
		else
			return (var321*var876 - stvr);
	}
}

class mthdcls396 {

	static int stvr;

	static mthdcls396() {
		stvr = 743;
	}

	public int method396 (int var442, int var650) {
		if (var442>var650)
			return (var442-var650 + stvr);
		else
			return (var650-var442 - stvr);
	}
}

class mthdcls397 {

	static int stvr;

	static mthdcls397() {
		stvr = 244;
	}

	public int method397 (int var254, int var31) {
		if (var254>var31)
			return (var254+var31 + stvr);
		else
			return (var31+var254 - stvr);
	}
}

class mthdcls398 {

	static int stvr;

	static mthdcls398() {
		stvr = 300;
	}

	public int method398 (int var233, int var746) {
		if (var233>var746)
			return (var233+var746 + stvr);
		else
			return (var746+var233 - stvr);
	}
}

class mthdcls399 {

	static int stvr;

	static mthdcls399() {
		stvr = 923;
	}

	public int method399 (int var922, int var14) {
		if (var922>var14)
			return (var922-var14 + stvr);
		else
			return (var14-var922 - stvr);
	}
}

class mthdcls400 {

	static int stvr;

	static mthdcls400() {
		stvr = 122;
	}

	public int method400 (int var477, int var716) {
		if (var477>var716)
			return (var477*var716 + stvr);
		else
			return (var716*var477 - stvr);
	}
}

class mthdcls401 {

	static int stvr;

	static mthdcls401() {
		stvr = 598;
	}

	public int method401 (int var841, int var234) {
		if (var841>var234)
			return (var841-var234 + stvr);
		else
			return (var234-var841 - stvr);
	}
}

class mthdcls402 {

	static int stvr;

	static mthdcls402() {
		stvr = 409;
	}

	public int method402 (int var895, int var754) {
		if (var895>var754)
			return (var895+var754 + stvr);
		else
			return (var754+var895 - stvr);
	}
}

class mthdcls403 {

	static int stvr;

	static mthdcls403() {
		stvr = 653;
	}

	public int method403 (int var953, int var332) {
		if (var953>var332)
			return (var953*var332 + stvr);
		else
			return (var332*var953 - stvr);
	}
}

class mthdcls404 {

	static int stvr;

	static mthdcls404() {
		stvr = 740;
	}

	public int method404 (int var371, int var467) {
		if (var371>var467)
			return (var371+var467 + stvr);
		else
			return (var467+var371 - stvr);
	}
}

class mthdcls405 {

	static int stvr;

	static mthdcls405() {
		stvr = 839;
	}

	public int method405 (int var813, int var879) {
		if (var813>var879)
			return (var813-var879 + stvr);
		else
			return (var879-var813 - stvr);
	}
}

class mthdcls406 {

	static int stvr;

	static mthdcls406() {
		stvr = 431;
	}

	public int method406 (int var642, int var722) {
		if (var642>var722)
			return (var642-var722 + stvr);
		else
			return (var722-var642 - stvr);
	}
}

class mthdcls407 {

	static int stvr;

	static mthdcls407() {
		stvr = 900;
	}

	public int method407 (int var688, int var241) {
		if (var688>var241)
			return (var688*var241 + stvr);
		else
			return (var241*var688 - stvr);
	}
}

class mthdcls408 {

	static int stvr;

	static mthdcls408() {
		stvr = 198;
	}

	public int method408 (int var922, int var378) {
		if (var922>var378)
			return (var922*var378 + stvr);
		else
			return (var378*var922 - stvr);
	}
}

class mthdcls409 {

	static int stvr;

	static mthdcls409() {
		stvr = 501;
	}

	public int method409 (int var766, int var5) {
		if (var766>var5)
			return (var766*var5 + stvr);
		else
			return (var5*var766 - stvr);
	}
}

class mthdcls410 {

	static int stvr;

	static mthdcls410() {
		stvr = 522;
	}

	public int method410 (int var42, int var174) {
		if (var42>var174)
			return (var42+var174 + stvr);
		else
			return (var174+var42 - stvr);
	}
}

class mthdcls411 {

	static int stvr;

	static mthdcls411() {
		stvr = 752;
	}

	public int method411 (int var754, int var284) {
		if (var754>var284)
			return (var754-var284 + stvr);
		else
			return (var284-var754 - stvr);
	}
}

class mthdcls412 {

	static int stvr;

	static mthdcls412() {
		stvr = 333;
	}

	public int method412 (int var599, int var333) {
		if (var599>var333)
			return (var599+var333 + stvr);
		else
			return (var333+var599 - stvr);
	}
}

class mthdcls413 {

	static int stvr;

	static mthdcls413() {
		stvr = 294;
	}

	public int method413 (int var516, int var555) {
		if (var516>var555)
			return (var516*var555 + stvr);
		else
			return (var555*var516 - stvr);
	}
}

class mthdcls414 {

	static int stvr;

	static mthdcls414() {
		stvr = 646;
	}

	public int method414 (int var948, int var830) {
		if (var948>var830)
			return (var948*var830 + stvr);
		else
			return (var830*var948 - stvr);
	}
}

class mthdcls415 {

	static int stvr;

	static mthdcls415() {
		stvr = 951;
	}

	public int method415 (int var196, int var945) {
		if (var196>var945)
			return (var196-var945 + stvr);
		else
			return (var945-var196 - stvr);
	}
}

class mthdcls416 {

	static int stvr;

	static mthdcls416() {
		stvr = 240;
	}

	public int method416 (int var528, int var126) {
		if (var528>var126)
			return (var528*var126 + stvr);
		else
			return (var126*var528 - stvr);
	}
}

class mthdcls417 {

	static int stvr;

	static mthdcls417() {
		stvr = 561;
	}

	public int method417 (int var309, int var389) {
		if (var309>var389)
			return (var309-var389 + stvr);
		else
			return (var389-var309 - stvr);
	}
}

class mthdcls418 {

	static int stvr;

	static mthdcls418() {
		stvr = 788;
	}

	public int method418 (int var132, int var946) {
		if (var132>var946)
			return (var132+var946 + stvr);
		else
			return (var946+var132 - stvr);
	}
}

class mthdcls419 {

	static int stvr;

	static mthdcls419() {
		stvr = 81;
	}

	public int method419 (int var91, int var731) {
		if (var91>var731)
			return (var91-var731 + stvr);
		else
			return (var731-var91 - stvr);
	}
}

class mthdcls420 {

	static int stvr;

	static mthdcls420() {
		stvr = 848;
	}

	public int method420 (int var821, int var53) {
		if (var821>var53)
			return (var821*var53 + stvr);
		else
			return (var53*var821 - stvr);
	}
}

class mthdcls421 {

	static int stvr;

	static mthdcls421() {
		stvr = 392;
	}

	public int method421 (int var916, int var933) {
		if (var916>var933)
			return (var916-var933 + stvr);
		else
			return (var933-var916 - stvr);
	}
}

class mthdcls422 {

	static int stvr;

	static mthdcls422() {
		stvr = 210;
	}

	public int method422 (int var365, int var723) {
		if (var365>var723)
			return (var365-var723 + stvr);
		else
			return (var723-var365 - stvr);
	}
}

class mthdcls423 {

	static int stvr;

	static mthdcls423() {
		stvr = 478;
	}

	public int method423 (int var653, int var545) {
		if (var653>var545)
			return (var653*var545 + stvr);
		else
			return (var545*var653 - stvr);
	}
}

class mthdcls424 {

	static int stvr;

	static mthdcls424() {
		stvr = 933;
	}

	public int method424 (int var785, int var474) {
		if (var785>var474)
			return (var785-var474 + stvr);
		else
			return (var474-var785 - stvr);
	}
}

class mthdcls425 {

	static int stvr;

	static mthdcls425() {
		stvr = 639;
	}

	public int method425 (int var894, int var982) {
		if (var894>var982)
			return (var894+var982 + stvr);
		else
			return (var982+var894 - stvr);
	}
}

class mthdcls426 {

	static int stvr;

	static mthdcls426() {
		stvr = 417;
	}

	public int method426 (int var262, int var552) {
		if (var262>var552)
			return (var262+var552 + stvr);
		else
			return (var552+var262 - stvr);
	}
}

class mthdcls427 {

	static int stvr;

	static mthdcls427() {
		stvr = 815;
	}

	public int method427 (int var805, int var916) {
		if (var805>var916)
			return (var805+var916 + stvr);
		else
			return (var916+var805 - stvr);
	}
}

class mthdcls428 {

	static int stvr;

	static mthdcls428() {
		stvr = 406;
	}

	public int method428 (int var764, int var910) {
		if (var764>var910)
			return (var764+var910 + stvr);
		else
			return (var910+var764 - stvr);
	}
}

class mthdcls429 {

	static int stvr;

	static mthdcls429() {
		stvr = 208;
	}

	public int method429 (int var658, int var13) {
		if (var658>var13)
			return (var658*var13 + stvr);
		else
			return (var13*var658 - stvr);
	}
}

class mthdcls430 {

	static int stvr;

	static mthdcls430() {
		stvr = 363;
	}

	public int method430 (int var109, int var92) {
		if (var109>var92)
			return (var109-var92 + stvr);
		else
			return (var92-var109 - stvr);
	}
}

class mthdcls431 {

	static int stvr;

	static mthdcls431() {
		stvr = 29;
	}

	public int method431 (int var268, int var636) {
		if (var268>var636)
			return (var268-var636 + stvr);
		else
			return (var636-var268 - stvr);
	}
}

class mthdcls432 {

	static int stvr;

	static mthdcls432() {
		stvr = 403;
	}

	public int method432 (int var999, int var117) {
		if (var999>var117)
			return (var999+var117 + stvr);
		else
			return (var117+var999 - stvr);
	}
}

class mthdcls433 {

	static int stvr;

	static mthdcls433() {
		stvr = 403;
	}

	public int method433 (int var454, int var316) {
		if (var454>var316)
			return (var454*var316 + stvr);
		else
			return (var316*var454 - stvr);
	}
}

class mthdcls434 {

	static int stvr;

	static mthdcls434() {
		stvr = 212;
	}

	public int method434 (int var639, int var336) {
		if (var639>var336)
			return (var639-var336 + stvr);
		else
			return (var336-var639 - stvr);
	}
}

class mthdcls435 {

	static int stvr;

	static mthdcls435() {
		stvr = 410;
	}

	public int method435 (int var692, int var111) {
		if (var692>var111)
			return (var692*var111 + stvr);
		else
			return (var111*var692 - stvr);
	}
}

class mthdcls436 {

	static int stvr;

	static mthdcls436() {
		stvr = 264;
	}

	public int method436 (int var258, int var953) {
		if (var258>var953)
			return (var258*var953 + stvr);
		else
			return (var953*var258 - stvr);
	}
}

class mthdcls437 {

	static int stvr;

	static mthdcls437() {
		stvr = 484;
	}

	public int method437 (int var144, int var148) {
		if (var144>var148)
			return (var144+var148 + stvr);
		else
			return (var148+var144 - stvr);
	}
}

class mthdcls438 {

	static int stvr;

	static mthdcls438() {
		stvr = 779;
	}

	public int method438 (int var489, int var513) {
		if (var489>var513)
			return (var489-var513 + stvr);
		else
			return (var513-var489 - stvr);
	}
}

class mthdcls439 {

	static int stvr;

	static mthdcls439() {
		stvr = 70;
	}

	public int method439 (int var428, int var698) {
		if (var428>var698)
			return (var428*var698 + stvr);
		else
			return (var698*var428 - stvr);
	}
}

class mthdcls440 {

	static int stvr;

	static mthdcls440() {
		stvr = 899;
	}

	public int method440 (int var546, int var603) {
		if (var546>var603)
			return (var546+var603 + stvr);
		else
			return (var603+var546 - stvr);
	}
}

class mthdcls441 {

	static int stvr;

	static mthdcls441() {
		stvr = 238;
	}

	public int method441 (int var156, int var827) {
		if (var156>var827)
			return (var156+var827 + stvr);
		else
			return (var827+var156 - stvr);
	}
}

class mthdcls442 {

	static int stvr;

	static mthdcls442() {
		stvr = 281;
	}

	public int method442 (int var120, int var151) {
		if (var120>var151)
			return (var120*var151 + stvr);
		else
			return (var151*var120 - stvr);
	}
}

class mthdcls443 {

	static int stvr;

	static mthdcls443() {
		stvr = 896;
	}

	public int method443 (int var486, int var338) {
		if (var486>var338)
			return (var486+var338 + stvr);
		else
			return (var338+var486 - stvr);
	}
}

class mthdcls444 {

	static int stvr;

	static mthdcls444() {
		stvr = 222;
	}

	public int method444 (int var756, int var246) {
		if (var756>var246)
			return (var756-var246 + stvr);
		else
			return (var246-var756 - stvr);
	}
}

class mthdcls445 {

	static int stvr;

	static mthdcls445() {
		stvr = 267;
	}

	public int method445 (int var36, int var436) {
		if (var36>var436)
			return (var36+var436 + stvr);
		else
			return (var436+var36 - stvr);
	}
}

class mthdcls446 {

	static int stvr;

	static mthdcls446() {
		stvr = 725;
	}

	public int method446 (int var864, int var872) {
		if (var864>var872)
			return (var864*var872 + stvr);
		else
			return (var872*var864 - stvr);
	}
}

class mthdcls447 {

	static int stvr;

	static mthdcls447() {
		stvr = 69;
	}

	public int method447 (int var107, int var671) {
		if (var107>var671)
			return (var107*var671 + stvr);
		else
			return (var671*var107 - stvr);
	}
}

class mthdcls448 {

	static int stvr;

	static mthdcls448() {
		stvr = 514;
	}

	public int method448 (int var806, int var251) {
		if (var806>var251)
			return (var806*var251 + stvr);
		else
			return (var251*var806 - stvr);
	}
}

class mthdcls449 {

	static int stvr;

	static mthdcls449() {
		stvr = 438;
	}

	public int method449 (int var242, int var290) {
		if (var242>var290)
			return (var242-var290 + stvr);
		else
			return (var290-var242 - stvr);
	}
}

class mthdcls450 {

	static int stvr;

	static mthdcls450() {
		stvr = 294;
	}

	public int method450 (int var991, int var430) {
		if (var991>var430)
			return (var991+var430 + stvr);
		else
			return (var430+var991 - stvr);
	}
}

class mthdcls451 {

	static int stvr;

	static mthdcls451() {
		stvr = 715;
	}

	public int method451 (int var674, int var878) {
		if (var674>var878)
			return (var674+var878 + stvr);
		else
			return (var878+var674 - stvr);
	}
}

class mthdcls452 {

	static int stvr;

	static mthdcls452() {
		stvr = 218;
	}

	public int method452 (int var484, int var758) {
		if (var484>var758)
			return (var484*var758 + stvr);
		else
			return (var758*var484 - stvr);
	}
}

class mthdcls453 {

	static int stvr;

	static mthdcls453() {
		stvr = 176;
	}

	public int method453 (int var868, int var637) {
		if (var868>var637)
			return (var868+var637 + stvr);
		else
			return (var637+var868 - stvr);
	}
}

class mthdcls454 {

	static int stvr;

	static mthdcls454() {
		stvr = 325;
	}

	public int method454 (int var195, int var899) {
		if (var195>var899)
			return (var195*var899 + stvr);
		else
			return (var899*var195 - stvr);
	}
}

class mthdcls455 {

	static int stvr;

	static mthdcls455() {
		stvr = 512;
	}

	public int method455 (int var326, int var951) {
		if (var326>var951)
			return (var326-var951 + stvr);
		else
			return (var951-var326 - stvr);
	}
}

class mthdcls456 {

	static int stvr;

	static mthdcls456() {
		stvr = 523;
	}

	public int method456 (int var158, int var720) {
		if (var158>var720)
			return (var158*var720 + stvr);
		else
			return (var720*var158 - stvr);
	}
}

class mthdcls457 {

	static int stvr;

	static mthdcls457() {
		stvr = 391;
	}

	public int method457 (int var106, int var654) {
		if (var106>var654)
			return (var106+var654 + stvr);
		else
			return (var654+var106 - stvr);
	}
}

class mthdcls458 {

	static int stvr;

	static mthdcls458() {
		stvr = 294;
	}

	public int method458 (int var469, int var495) {
		if (var469>var495)
			return (var469*var495 + stvr);
		else
			return (var495*var469 - stvr);
	}
}

class mthdcls459 {

	static int stvr;

	static mthdcls459() {
		stvr = 469;
	}

	public int method459 (int var906, int var926) {
		if (var906>var926)
			return (var906-var926 + stvr);
		else
			return (var926-var906 - stvr);
	}
}

class mthdcls460 {

	static int stvr;

	static mthdcls460() {
		stvr = 929;
	}

	public int method460 (int var291, int var778) {
		if (var291>var778)
			return (var291*var778 + stvr);
		else
			return (var778*var291 - stvr);
	}
}

class mthdcls461 {

	static int stvr;

	static mthdcls461() {
		stvr = 972;
	}

	public int method461 (int var270, int var907) {
		if (var270>var907)
			return (var270+var907 + stvr);
		else
			return (var907+var270 - stvr);
	}
}

class mthdcls462 {

	static int stvr;

	static mthdcls462() {
		stvr = 851;
	}

	public int method462 (int var20, int var486) {
		if (var20>var486)
			return (var20-var486 + stvr);
		else
			return (var486-var20 - stvr);
	}
}

class mthdcls463 {

	static int stvr;

	static mthdcls463() {
		stvr = 59;
	}

	public int method463 (int var989, int var463) {
		if (var989>var463)
			return (var989*var463 + stvr);
		else
			return (var463*var989 - stvr);
	}
}

class mthdcls464 {

	static int stvr;

	static mthdcls464() {
		stvr = 721;
	}

	public int method464 (int var942, int var168) {
		if (var942>var168)
			return (var942+var168 + stvr);
		else
			return (var168+var942 - stvr);
	}
}

class mthdcls465 {

	static int stvr;

	static mthdcls465() {
		stvr = 14;
	}

	public int method465 (int var417, int var970) {
		if (var417>var970)
			return (var417*var970 + stvr);
		else
			return (var970*var417 - stvr);
	}
}

class mthdcls466 {

	static int stvr;

	static mthdcls466() {
		stvr = 786;
	}

	public int method466 (int var418, int var979) {
		if (var418>var979)
			return (var418+var979 + stvr);
		else
			return (var979+var418 - stvr);
	}
}

class mthdcls467 {

	static int stvr;

	static mthdcls467() {
		stvr = 110;
	}

	public int method467 (int var671, int var869) {
		if (var671>var869)
			return (var671-var869 + stvr);
		else
			return (var869-var671 - stvr);
	}
}

class mthdcls468 {

	static int stvr;

	static mthdcls468() {
		stvr = 214;
	}

	public int method468 (int var642, int var594) {
		if (var642>var594)
			return (var642-var594 + stvr);
		else
			return (var594-var642 - stvr);
	}
}

class mthdcls469 {

	static int stvr;

	static mthdcls469() {
		stvr = 616;
	}

	public int method469 (int var301, int var774) {
		if (var301>var774)
			return (var301-var774 + stvr);
		else
			return (var774-var301 - stvr);
	}
}

class mthdcls470 {

	static int stvr;

	static mthdcls470() {
		stvr = 192;
	}

	public int method470 (int var936, int var911) {
		if (var936>var911)
			return (var936*var911 + stvr);
		else
			return (var911*var936 - stvr);
	}
}

class mthdcls471 {

	static int stvr;

	static mthdcls471() {
		stvr = 144;
	}

	public int method471 (int var312, int var991) {
		if (var312>var991)
			return (var312*var991 + stvr);
		else
			return (var991*var312 - stvr);
	}
}

class mthdcls472 {

	static int stvr;

	static mthdcls472() {
		stvr = 633;
	}

	public int method472 (int var400, int var796) {
		if (var400>var796)
			return (var400+var796 + stvr);
		else
			return (var796+var400 - stvr);
	}
}

class mthdcls473 {

	static int stvr;

	static mthdcls473() {
		stvr = 398;
	}

	public int method473 (int var425, int var271) {
		if (var425>var271)
			return (var425*var271 + stvr);
		else
			return (var271*var425 - stvr);
	}
}

class mthdcls474 {

	static int stvr;

	static mthdcls474() {
		stvr = 470;
	}

	public int method474 (int var215, int var847) {
		if (var215>var847)
			return (var215-var847 + stvr);
		else
			return (var847-var215 - stvr);
	}
}

class mthdcls475 {

	static int stvr;

	static mthdcls475() {
		stvr = 646;
	}

	public int method475 (int var30, int var975) {
		if (var30>var975)
			return (var30*var975 + stvr);
		else
			return (var975*var30 - stvr);
	}
}

class mthdcls476 {

	static int stvr;

	static mthdcls476() {
		stvr = 945;
	}

	public int method476 (int var425, int var825) {
		if (var425>var825)
			return (var425*var825 + stvr);
		else
			return (var825*var425 - stvr);
	}
}

class mthdcls477 {

	static int stvr;

	static mthdcls477() {
		stvr = 57;
	}

	public int method477 (int var622, int var345) {
		if (var622>var345)
			return (var622+var345 + stvr);
		else
			return (var345+var622 - stvr);
	}
}

class mthdcls478 {

	static int stvr;

	static mthdcls478() {
		stvr = 87;
	}

	public int method478 (int var400, int var471) {
		if (var400>var471)
			return (var400-var471 + stvr);
		else
			return (var471-var400 - stvr);
	}
}

class mthdcls479 {

	static int stvr;

	static mthdcls479() {
		stvr = 776;
	}

	public int method479 (int var795, int var124) {
		if (var795>var124)
			return (var795+var124 + stvr);
		else
			return (var124+var795 - stvr);
	}
}

class mthdcls480 {

	static int stvr;

	static mthdcls480() {
		stvr = 614;
	}

	public int method480 (int var326, int var127) {
		if (var326>var127)
			return (var326*var127 + stvr);
		else
			return (var127*var326 - stvr);
	}
}

class mthdcls481 {

	static int stvr;

	static mthdcls481() {
		stvr = 11;
	}

	public int method481 (int var111, int var966) {
		if (var111>var966)
			return (var111*var966 + stvr);
		else
			return (var966*var111 - stvr);
	}
}

class mthdcls482 {

	static int stvr;

	static mthdcls482() {
		stvr = 441;
	}

	public int method482 (int var966, int var934) {
		if (var966>var934)
			return (var966*var934 + stvr);
		else
			return (var934*var966 - stvr);
	}
}

class mthdcls483 {

	static int stvr;

	static mthdcls483() {
		stvr = 976;
	}

	public int method483 (int var929, int var708) {
		if (var929>var708)
			return (var929+var708 + stvr);
		else
			return (var708+var929 - stvr);
	}
}

class mthdcls484 {

	static int stvr;

	static mthdcls484() {
		stvr = 421;
	}

	public int method484 (int var300, int var494) {
		if (var300>var494)
			return (var300-var494 + stvr);
		else
			return (var494-var300 - stvr);
	}
}

class mthdcls485 {

	static int stvr;

	static mthdcls485() {
		stvr = 139;
	}

	public int method485 (int var87, int var232) {
		if (var87>var232)
			return (var87*var232 + stvr);
		else
			return (var232*var87 - stvr);
	}
}

class mthdcls486 {

	static int stvr;

	static mthdcls486() {
		stvr = 551;
	}

	public int method486 (int var64, int var964) {
		if (var64>var964)
			return (var64+var964 + stvr);
		else
			return (var964+var64 - stvr);
	}
}

class mthdcls487 {

	static int stvr;

	static mthdcls487() {
		stvr = 691;
	}

	public int method487 (int var490, int var384) {
		if (var490>var384)
			return (var490-var384 + stvr);
		else
			return (var384-var490 - stvr);
	}
}

class mthdcls488 {

	static int stvr;

	static mthdcls488() {
		stvr = 108;
	}

	public int method488 (int var913, int var369) {
		if (var913>var369)
			return (var913-var369 + stvr);
		else
			return (var369-var913 - stvr);
	}
}

class mthdcls489 {

	static int stvr;

	static mthdcls489() {
		stvr = 424;
	}

	public int method489 (int var905, int var50) {
		if (var905>var50)
			return (var905*var50 + stvr);
		else
			return (var50*var905 - stvr);
	}
}

class mthdcls490 {

	static int stvr;

	static mthdcls490() {
		stvr = 652;
	}

	public int method490 (int var563, int var985) {
		if (var563>var985)
			return (var563-var985 + stvr);
		else
			return (var985-var563 - stvr);
	}
}

class mthdcls491 {

	static int stvr;

	static mthdcls491() {
		stvr = 667;
	}

	public int method491 (int var362, int var575) {
		if (var362>var575)
			return (var362*var575 + stvr);
		else
			return (var575*var362 - stvr);
	}
}

class mthdcls492 {

	static int stvr;

	static mthdcls492() {
		stvr = 526;
	}

	public int method492 (int var727, int var274) {
		if (var727>var274)
			return (var727*var274 + stvr);
		else
			return (var274*var727 - stvr);
	}
}

class mthdcls493 {

	static int stvr;

	static mthdcls493() {
		stvr = 25;
	}

	public int method493 (int var597, int var825) {
		if (var597>var825)
			return (var597+var825 + stvr);
		else
			return (var825+var597 - stvr);
	}
}

class mthdcls494 {

	static int stvr;

	static mthdcls494() {
		stvr = 674;
	}

	public int method494 (int var879, int var284) {
		if (var879>var284)
			return (var879*var284 + stvr);
		else
			return (var284*var879 - stvr);
	}
}

class mthdcls495 {

	static int stvr;

	static mthdcls495() {
		stvr = 808;
	}

	public int method495 (int var315, int var842) {
		if (var315>var842)
			return (var315-var842 + stvr);
		else
			return (var842-var315 - stvr);
	}
}

class mthdcls496 {

	static int stvr;

	static mthdcls496() {
		stvr = 99;
	}

	public int method496 (int var512, int var564) {
		if (var512>var564)
			return (var512*var564 + stvr);
		else
			return (var564*var512 - stvr);
	}
}

class mthdcls497 {

	static int stvr;

	static mthdcls497() {
		stvr = 505;
	}

	public int method497 (int var790, int var437) {
		if (var790>var437)
			return (var790+var437 + stvr);
		else
			return (var437+var790 - stvr);
	}
}

class mthdcls498 {

	static int stvr;

	static mthdcls498() {
		stvr = 952;
	}

	public int method498 (int var664, int var358) {
		if (var664>var358)
			return (var664+var358 + stvr);
		else
			return (var358+var664 - stvr);
	}
}

class mthdcls499 {

	static int stvr;

	static mthdcls499() {
		stvr = 853;
	}

	public int method499 (int var628, int var694) {
		if (var628>var694)
			return (var628+var694 + stvr);
		else
			return (var694+var628 - stvr);
	}
}

class mthdcls500 {

	static int stvr;

	static mthdcls500() {
		stvr = 54;
	}

	public int method500 (int var62, int var242) {
		if (var62>var242)
			return (var62*var242 + stvr);
		else
			return (var242*var62 - stvr);
	}
}

class mthdcls501 {

	static int stvr;

	static mthdcls501() {
		stvr = 350;
	}

	public int method501 (int var998, int var885) {
		if (var998>var885)
			return (var998+var885 + stvr);
		else
			return (var885+var998 - stvr);
	}
}

class mthdcls502 {

	static int stvr;

	static mthdcls502() {
		stvr = 861;
	}

	public int method502 (int var924, int var69) {
		if (var924>var69)
			return (var924-var69 + stvr);
		else
			return (var69-var924 - stvr);
	}
}

class mthdcls503 {

	static int stvr;

	static mthdcls503() {
		stvr = 831;
	}

	public int method503 (int var368, int var321) {
		if (var368>var321)
			return (var368+var321 + stvr);
		else
			return (var321+var368 - stvr);
	}
}

class mthdcls504 {

	static int stvr;

	static mthdcls504() {
		stvr = 331;
	}

	public int method504 (int var902, int var972) {
		if (var902>var972)
			return (var902-var972 + stvr);
		else
			return (var972-var902 - stvr);
	}
}

class mthdcls505 {

	static int stvr;

	static mthdcls505() {
		stvr = 366;
	}

	public int method505 (int var637, int var229) {
		if (var637>var229)
			return (var637*var229 + stvr);
		else
			return (var229*var637 - stvr);
	}
}

class mthdcls506 {

	static int stvr;

	static mthdcls506() {
		stvr = 832;
	}

	public int method506 (int var430, int var491) {
		if (var430>var491)
			return (var430+var491 + stvr);
		else
			return (var491+var430 - stvr);
	}
}

class mthdcls507 {

	static int stvr;

	static mthdcls507() {
		stvr = 754;
	}

	public int method507 (int var442, int var702) {
		if (var442>var702)
			return (var442*var702 + stvr);
		else
			return (var702*var442 - stvr);
	}
}

class mthdcls508 {

	static int stvr;

	static mthdcls508() {
		stvr = 630;
	}

	public int method508 (int var469, int var606) {
		if (var469>var606)
			return (var469*var606 + stvr);
		else
			return (var606*var469 - stvr);
	}
}

class mthdcls509 {

	static int stvr;

	static mthdcls509() {
		stvr = 413;
	}

	public int method509 (int var692, int var27) {
		if (var692>var27)
			return (var692+var27 + stvr);
		else
			return (var27+var692 - stvr);
	}
}

class mthdcls510 {

	static int stvr;

	static mthdcls510() {
		stvr = 777;
	}

	public int method510 (int var398, int var327) {
		if (var398>var327)
			return (var398+var327 + stvr);
		else
			return (var327+var398 - stvr);
	}
}

class mthdcls511 {

	static int stvr;

	static mthdcls511() {
		stvr = 189;
	}

	public int method511 (int var570, int var409) {
		if (var570>var409)
			return (var570*var409 + stvr);
		else
			return (var409*var570 - stvr);
	}
}

class mthdcls512 {

	static int stvr;

	static mthdcls512() {
		stvr = 207;
	}

	public int method512 (int var296, int var131) {
		if (var296>var131)
			return (var296+var131 + stvr);
		else
			return (var131+var296 - stvr);
	}
}

class mthdcls513 {

	static int stvr;

	static mthdcls513() {
		stvr = 567;
	}

	public int method513 (int var317, int var439) {
		if (var317>var439)
			return (var317-var439 + stvr);
		else
			return (var439-var317 - stvr);
	}
}

class mthdcls514 {

	static int stvr;

	static mthdcls514() {
		stvr = 864;
	}

	public int method514 (int var341, int var908) {
		if (var341>var908)
			return (var341-var908 + stvr);
		else
			return (var908-var341 - stvr);
	}
}

class mthdcls515 {

	static int stvr;

	static mthdcls515() {
		stvr = 362;
	}

	public int method515 (int var575, int var194) {
		if (var575>var194)
			return (var575+var194 + stvr);
		else
			return (var194+var575 - stvr);
	}
}

class mthdcls516 {

	static int stvr;

	static mthdcls516() {
		stvr = 128;
	}

	public int method516 (int var227, int var40) {
		if (var227>var40)
			return (var227-var40 + stvr);
		else
			return (var40-var227 - stvr);
	}
}

class mthdcls517 {

	static int stvr;

	static mthdcls517() {
		stvr = 125;
	}

	public int method517 (int var298, int var283) {
		if (var298>var283)
			return (var298+var283 + stvr);
		else
			return (var283+var298 - stvr);
	}
}

class mthdcls518 {

	static int stvr;

	static mthdcls518() {
		stvr = 199;
	}

	public int method518 (int var2, int var135) {
		if (var2>var135)
			return (var2+var135 + stvr);
		else
			return (var135+var2 - stvr);
	}
}

class mthdcls519 {

	static int stvr;

	static mthdcls519() {
		stvr = 286;
	}

	public int method519 (int var560, int var742) {
		if (var560>var742)
			return (var560*var742 + stvr);
		else
			return (var742*var560 - stvr);
	}
}

class mthdcls520 {

	static int stvr;

	static mthdcls520() {
		stvr = 642;
	}

	public int method520 (int var497, int var664) {
		if (var497>var664)
			return (var497+var664 + stvr);
		else
			return (var664+var497 - stvr);
	}
}

class mthdcls521 {

	static int stvr;

	static mthdcls521() {
		stvr = 440;
	}

	public int method521 (int var358, int var199) {
		if (var358>var199)
			return (var358-var199 + stvr);
		else
			return (var199-var358 - stvr);
	}
}

class mthdcls522 {

	static int stvr;

	static mthdcls522() {
		stvr = 359;
	}

	public int method522 (int var286, int var284) {
		if (var286>var284)
			return (var286-var284 + stvr);
		else
			return (var284-var286 - stvr);
	}
}

class mthdcls523 {

	static int stvr;

	static mthdcls523() {
		stvr = 530;
	}

	public int method523 (int var160, int var931) {
		if (var160>var931)
			return (var160+var931 + stvr);
		else
			return (var931+var160 - stvr);
	}
}

class mthdcls524 {

	static int stvr;

	static mthdcls524() {
		stvr = 387;
	}

	public int method524 (int var574, int var152) {
		if (var574>var152)
			return (var574-var152 + stvr);
		else
			return (var152-var574 - stvr);
	}
}

class mthdcls525 {

	static int stvr;

	static mthdcls525() {
		stvr = 53;
	}

	public int method525 (int var676, int var265) {
		if (var676>var265)
			return (var676*var265 + stvr);
		else
			return (var265*var676 - stvr);
	}
}

class mthdcls526 {

	static int stvr;

	static mthdcls526() {
		stvr = 821;
	}

	public int method526 (int var375, int var753) {
		if (var375>var753)
			return (var375*var753 + stvr);
		else
			return (var753*var375 - stvr);
	}
}

class mthdcls527 {

	static int stvr;

	static mthdcls527() {
		stvr = 641;
	}

	public int method527 (int var943, int var680) {
		if (var943>var680)
			return (var943-var680 + stvr);
		else
			return (var680-var943 - stvr);
	}
}

class mthdcls528 {

	static int stvr;

	static mthdcls528() {
		stvr = 346;
	}

	public int method528 (int var367, int var753) {
		if (var367>var753)
			return (var367-var753 + stvr);
		else
			return (var753-var367 - stvr);
	}
}

class mthdcls529 {

	static int stvr;

	static mthdcls529() {
		stvr = 861;
	}

	public int method529 (int var849, int var747) {
		if (var849>var747)
			return (var849+var747 + stvr);
		else
			return (var747+var849 - stvr);
	}
}

class mthdcls530 {

	static int stvr;

	static mthdcls530() {
		stvr = 247;
	}

	public int method530 (int var294, int var688) {
		if (var294>var688)
			return (var294-var688 + stvr);
		else
			return (var688-var294 - stvr);
	}
}

class mthdcls531 {

	static int stvr;

	static mthdcls531() {
		stvr = 341;
	}

	public int method531 (int var743, int var843) {
		if (var743>var843)
			return (var743+var843 + stvr);
		else
			return (var843+var743 - stvr);
	}
}

class mthdcls532 {

	static int stvr;

	static mthdcls532() {
		stvr = 839;
	}

	public int method532 (int var677, int var557) {
		if (var677>var557)
			return (var677*var557 + stvr);
		else
			return (var557*var677 - stvr);
	}
}

class mthdcls533 {

	static int stvr;

	static mthdcls533() {
		stvr = 3;
	}

	public int method533 (int var533, int var937) {
		if (var533>var937)
			return (var533*var937 + stvr);
		else
			return (var937*var533 - stvr);
	}
}

class mthdcls534 {

	static int stvr;

	static mthdcls534() {
		stvr = 820;
	}

	public int method534 (int var413, int var69) {
		if (var413>var69)
			return (var413-var69 + stvr);
		else
			return (var69-var413 - stvr);
	}
}

class mthdcls535 {

	static int stvr;

	static mthdcls535() {
		stvr = 961;
	}

	public int method535 (int var885, int var905) {
		if (var885>var905)
			return (var885-var905 + stvr);
		else
			return (var905-var885 - stvr);
	}
}

class mthdcls536 {

	static int stvr;

	static mthdcls536() {
		stvr = 113;
	}

	public int method536 (int var833, int var924) {
		if (var833>var924)
			return (var833+var924 + stvr);
		else
			return (var924+var833 - stvr);
	}
}

class mthdcls537 {

	static int stvr;

	static mthdcls537() {
		stvr = 547;
	}

	public int method537 (int var818, int var914) {
		if (var818>var914)
			return (var818*var914 + stvr);
		else
			return (var914*var818 - stvr);
	}
}

class mthdcls538 {

	static int stvr;

	static mthdcls538() {
		stvr = 332;
	}

	public int method538 (int var5, int var677) {
		if (var5>var677)
			return (var5+var677 + stvr);
		else
			return (var677+var5 - stvr);
	}
}

class mthdcls539 {

	static int stvr;

	static mthdcls539() {
		stvr = 819;
	}

	public int method539 (int var297, int var932) {
		if (var297>var932)
			return (var297*var932 + stvr);
		else
			return (var932*var297 - stvr);
	}
}

class mthdcls540 {

	static int stvr;

	static mthdcls540() {
		stvr = 363;
	}

	public int method540 (int var944, int var785) {
		if (var944>var785)
			return (var944-var785 + stvr);
		else
			return (var785-var944 - stvr);
	}
}

class mthdcls541 {

	static int stvr;

	static mthdcls541() {
		stvr = 570;
	}

	public int method541 (int var369, int var575) {
		if (var369>var575)
			return (var369-var575 + stvr);
		else
			return (var575-var369 - stvr);
	}
}

class mthdcls542 {

	static int stvr;

	static mthdcls542() {
		stvr = 142;
	}

	public int method542 (int var828, int var295) {
		if (var828>var295)
			return (var828-var295 + stvr);
		else
			return (var295-var828 - stvr);
	}
}

class mthdcls543 {

	static int stvr;

	static mthdcls543() {
		stvr = 122;
	}

	public int method543 (int var0, int var225) {
		if (var0>var225)
			return (var0-var225 + stvr);
		else
			return (var225-var0 - stvr);
	}
}

class mthdcls544 {

	static int stvr;

	static mthdcls544() {
		stvr = 826;
	}

	public int method544 (int var600, int var118) {
		if (var600>var118)
			return (var600*var118 + stvr);
		else
			return (var118*var600 - stvr);
	}
}

class mthdcls545 {

	static int stvr;

	static mthdcls545() {
		stvr = 336;
	}

	public int method545 (int var627, int var706) {
		if (var627>var706)
			return (var627*var706 + stvr);
		else
			return (var706*var627 - stvr);
	}
}

class mthdcls546 {

	static int stvr;

	static mthdcls546() {
		stvr = 613;
	}

	public int method546 (int var310, int var334) {
		if (var310>var334)
			return (var310-var334 + stvr);
		else
			return (var334-var310 - stvr);
	}
}

class mthdcls547 {

	static int stvr;

	static mthdcls547() {
		stvr = 509;
	}

	public int method547 (int var537, int var782) {
		if (var537>var782)
			return (var537*var782 + stvr);
		else
			return (var782*var537 - stvr);
	}
}

class mthdcls548 {

	static int stvr;

	static mthdcls548() {
		stvr = 127;
	}

	public int method548 (int var592, int var791) {
		if (var592>var791)
			return (var592-var791 + stvr);
		else
			return (var791-var592 - stvr);
	}
}

class mthdcls549 {

	static int stvr;

	static mthdcls549() {
		stvr = 314;
	}

	public int method549 (int var886, int var850) {
		if (var886>var850)
			return (var886+var850 + stvr);
		else
			return (var850+var886 - stvr);
	}
}

class mthdcls550 {

	static int stvr;

	static mthdcls550() {
		stvr = 710;
	}

	public int method550 (int var590, int var596) {
		if (var590>var596)
			return (var590-var596 + stvr);
		else
			return (var596-var590 - stvr);
	}
}

class mthdcls551 {

	static int stvr;

	static mthdcls551() {
		stvr = 886;
	}

	public int method551 (int var610, int var171) {
		if (var610>var171)
			return (var610*var171 + stvr);
		else
			return (var171*var610 - stvr);
	}
}

class mthdcls552 {

	static int stvr;

	static mthdcls552() {
		stvr = 250;
	}

	public int method552 (int var587, int var65) {
		if (var587>var65)
			return (var587*var65 + stvr);
		else
			return (var65*var587 - stvr);
	}
}

class mthdcls553 {

	static int stvr;

	static mthdcls553() {
		stvr = 748;
	}

	public int method553 (int var36, int var168) {
		if (var36>var168)
			return (var36+var168 + stvr);
		else
			return (var168+var36 - stvr);
	}
}

class mthdcls554 {

	static int stvr;

	static mthdcls554() {
		stvr = 347;
	}

	public int method554 (int var149, int var910) {
		if (var149>var910)
			return (var149*var910 + stvr);
		else
			return (var910*var149 - stvr);
	}
}

class mthdcls555 {

	static int stvr;

	static mthdcls555() {
		stvr = 459;
	}

	public int method555 (int var4, int var407) {
		if (var4>var407)
			return (var4*var407 + stvr);
		else
			return (var407*var4 - stvr);
	}
}

class mthdcls556 {

	static int stvr;

	static mthdcls556() {
		stvr = 247;
	}

	public int method556 (int var455, int var820) {
		if (var455>var820)
			return (var455*var820 + stvr);
		else
			return (var820*var455 - stvr);
	}
}

class mthdcls557 {

	static int stvr;

	static mthdcls557() {
		stvr = 343;
	}

	public int method557 (int var245, int var83) {
		if (var245>var83)
			return (var245*var83 + stvr);
		else
			return (var83*var245 - stvr);
	}
}

class mthdcls558 {

	static int stvr;

	static mthdcls558() {
		stvr = 642;
	}

	public int method558 (int var368, int var33) {
		if (var368>var33)
			return (var368+var33 + stvr);
		else
			return (var33+var368 - stvr);
	}
}

class mthdcls559 {

	static int stvr;

	static mthdcls559() {
		stvr = 934;
	}

	public int method559 (int var682, int var444) {
		if (var682>var444)
			return (var682*var444 + stvr);
		else
			return (var444*var682 - stvr);
	}
}

class mthdcls560 {

	static int stvr;

	static mthdcls560() {
		stvr = 277;
	}

	public int method560 (int var479, int var391) {
		if (var479>var391)
			return (var479-var391 + stvr);
		else
			return (var391-var479 - stvr);
	}
}

class mthdcls561 {

	static int stvr;

	static mthdcls561() {
		stvr = 483;
	}

	public int method561 (int var770, int var348) {
		if (var770>var348)
			return (var770*var348 + stvr);
		else
			return (var348*var770 - stvr);
	}
}

class mthdcls562 {

	static int stvr;

	static mthdcls562() {
		stvr = 838;
	}

	public int method562 (int var527, int var828) {
		if (var527>var828)
			return (var527+var828 + stvr);
		else
			return (var828+var527 - stvr);
	}
}

class mthdcls563 {

	static int stvr;

	static mthdcls563() {
		stvr = 399;
	}

	public int method563 (int var553, int var423) {
		if (var553>var423)
			return (var553-var423 + stvr);
		else
			return (var423-var553 - stvr);
	}
}

class mthdcls564 {

	static int stvr;

	static mthdcls564() {
		stvr = 569;
	}

	public int method564 (int var592, int var234) {
		if (var592>var234)
			return (var592*var234 + stvr);
		else
			return (var234*var592 - stvr);
	}
}

class mthdcls565 {

	static int stvr;

	static mthdcls565() {
		stvr = 901;
	}

	public int method565 (int var757, int var633) {
		if (var757>var633)
			return (var757-var633 + stvr);
		else
			return (var633-var757 - stvr);
	}
}

class mthdcls566 {

	static int stvr;

	static mthdcls566() {
		stvr = 313;
	}

	public int method566 (int var655, int var923) {
		if (var655>var923)
			return (var655*var923 + stvr);
		else
			return (var923*var655 - stvr);
	}
}

class mthdcls567 {

	static int stvr;

	static mthdcls567() {
		stvr = 518;
	}

	public int method567 (int var626, int var981) {
		if (var626>var981)
			return (var626+var981 + stvr);
		else
			return (var981+var626 - stvr);
	}
}

class mthdcls568 {

	static int stvr;

	static mthdcls568() {
		stvr = 0;
	}

	public int method568 (int var822, int var684) {
		if (var822>var684)
			return (var822*var684 + stvr);
		else
			return (var684*var822 - stvr);
	}
}

class mthdcls569 {

	static int stvr;

	static mthdcls569() {
		stvr = 506;
	}

	public int method569 (int var134, int var463) {
		if (var134>var463)
			return (var134+var463 + stvr);
		else
			return (var463+var134 - stvr);
	}
}

class mthdcls570 {

	static int stvr;

	static mthdcls570() {
		stvr = 513;
	}

	public int method570 (int var48, int var542) {
		if (var48>var542)
			return (var48+var542 + stvr);
		else
			return (var542+var48 - stvr);
	}
}

class mthdcls571 {

	static int stvr;

	static mthdcls571() {
		stvr = 975;
	}

	public int method571 (int var555, int var78) {
		if (var555>var78)
			return (var555-var78 + stvr);
		else
			return (var78-var555 - stvr);
	}
}

class mthdcls572 {

	static int stvr;

	static mthdcls572() {
		stvr = 768;
	}

	public int method572 (int var789, int var830) {
		if (var789>var830)
			return (var789-var830 + stvr);
		else
			return (var830-var789 - stvr);
	}
}

class mthdcls573 {

	static int stvr;

	static mthdcls573() {
		stvr = 537;
	}

	public int method573 (int var842, int var827) {
		if (var842>var827)
			return (var842+var827 + stvr);
		else
			return (var827+var842 - stvr);
	}
}

class mthdcls574 {

	static int stvr;

	static mthdcls574() {
		stvr = 538;
	}

	public int method574 (int var90, int var916) {
		if (var90>var916)
			return (var90*var916 + stvr);
		else
			return (var916*var90 - stvr);
	}
}

class mthdcls575 {

	static int stvr;

	static mthdcls575() {
		stvr = 834;
	}

	public int method575 (int var49, int var720) {
		if (var49>var720)
			return (var49*var720 + stvr);
		else
			return (var720*var49 - stvr);
	}
}

class mthdcls576 {

	static int stvr;

	static mthdcls576() {
		stvr = 258;
	}

	public int method576 (int var678, int var657) {
		if (var678>var657)
			return (var678*var657 + stvr);
		else
			return (var657*var678 - stvr);
	}
}

class mthdcls577 {

	static int stvr;

	static mthdcls577() {
		stvr = 835;
	}

	public int method577 (int var825, int var155) {
		if (var825>var155)
			return (var825-var155 + stvr);
		else
			return (var155-var825 - stvr);
	}
}

class mthdcls578 {

	static int stvr;

	static mthdcls578() {
		stvr = 554;
	}

	public int method578 (int var798, int var443) {
		if (var798>var443)
			return (var798-var443 + stvr);
		else
			return (var443-var798 - stvr);
	}
}

class mthdcls579 {

	static int stvr;

	static mthdcls579() {
		stvr = 828;
	}

	public int method579 (int var905, int var145) {
		if (var905>var145)
			return (var905-var145 + stvr);
		else
			return (var145-var905 - stvr);
	}
}

class mthdcls580 {

	static int stvr;

	static mthdcls580() {
		stvr = 447;
	}

	public int method580 (int var413, int var629) {
		if (var413>var629)
			return (var413-var629 + stvr);
		else
			return (var629-var413 - stvr);
	}
}

class mthdcls581 {

	static int stvr;

	static mthdcls581() {
		stvr = 237;
	}

	public int method581 (int var391, int var284) {
		if (var391>var284)
			return (var391*var284 + stvr);
		else
			return (var284*var391 - stvr);
	}
}

class mthdcls582 {

	static int stvr;

	static mthdcls582() {
		stvr = 586;
	}

	public int method582 (int var400, int var243) {
		if (var400>var243)
			return (var400+var243 + stvr);
		else
			return (var243+var400 - stvr);
	}
}

class mthdcls583 {

	static int stvr;

	static mthdcls583() {
		stvr = 432;
	}

	public int method583 (int var345, int var275) {
		if (var345>var275)
			return (var345+var275 + stvr);
		else
			return (var275+var345 - stvr);
	}
}

class mthdcls584 {

	static int stvr;

	static mthdcls584() {
		stvr = 934;
	}

	public int method584 (int var697, int var998) {
		if (var697>var998)
			return (var697*var998 + stvr);
		else
			return (var998*var697 - stvr);
	}
}

class mthdcls585 {

	static int stvr;

	static mthdcls585() {
		stvr = 112;
	}

	public int method585 (int var460, int var469) {
		if (var460>var469)
			return (var460+var469 + stvr);
		else
			return (var469+var460 - stvr);
	}
}

class mthdcls586 {

	static int stvr;

	static mthdcls586() {
		stvr = 753;
	}

	public int method586 (int var765, int var483) {
		if (var765>var483)
			return (var765+var483 + stvr);
		else
			return (var483+var765 - stvr);
	}
}

class mthdcls587 {

	static int stvr;

	static mthdcls587() {
		stvr = 575;
	}

	public int method587 (int var435, int var71) {
		if (var435>var71)
			return (var435+var71 + stvr);
		else
			return (var71+var435 - stvr);
	}
}

class mthdcls588 {

	static int stvr;

	static mthdcls588() {
		stvr = 934;
	}

	public int method588 (int var550, int var722) {
		if (var550>var722)
			return (var550-var722 + stvr);
		else
			return (var722-var550 - stvr);
	}
}

class mthdcls589 {

	static int stvr;

	static mthdcls589() {
		stvr = 308;
	}

	public int method589 (int var800, int var509) {
		if (var800>var509)
			return (var800-var509 + stvr);
		else
			return (var509-var800 - stvr);
	}
}

class mthdcls590 {

	static int stvr;

	static mthdcls590() {
		stvr = 148;
	}

	public int method590 (int var436, int var32) {
		if (var436>var32)
			return (var436*var32 + stvr);
		else
			return (var32*var436 - stvr);
	}
}

class mthdcls591 {

	static int stvr;

	static mthdcls591() {
		stvr = 638;
	}

	public int method591 (int var930, int var875) {
		if (var930>var875)
			return (var930-var875 + stvr);
		else
			return (var875-var930 - stvr);
	}
}

class mthdcls592 {

	static int stvr;

	static mthdcls592() {
		stvr = 307;
	}

	public int method592 (int var319, int var708) {
		if (var319>var708)
			return (var319+var708 + stvr);
		else
			return (var708+var319 - stvr);
	}
}

class mthdcls593 {

	static int stvr;

	static mthdcls593() {
		stvr = 203;
	}

	public int method593 (int var677, int var308) {
		if (var677>var308)
			return (var677-var308 + stvr);
		else
			return (var308-var677 - stvr);
	}
}

class mthdcls594 {

	static int stvr;

	static mthdcls594() {
		stvr = 60;
	}

	public int method594 (int var835, int var966) {
		if (var835>var966)
			return (var835+var966 + stvr);
		else
			return (var966+var835 - stvr);
	}
}

class mthdcls595 {

	static int stvr;

	static mthdcls595() {
		stvr = 637;
	}

	public int method595 (int var664, int var849) {
		if (var664>var849)
			return (var664*var849 + stvr);
		else
			return (var849*var664 - stvr);
	}
}

class mthdcls596 {

	static int stvr;

	static mthdcls596() {
		stvr = 721;
	}

	public int method596 (int var584, int var831) {
		if (var584>var831)
			return (var584-var831 + stvr);
		else
			return (var831-var584 - stvr);
	}
}

class mthdcls597 {

	static int stvr;

	static mthdcls597() {
		stvr = 982;
	}

	public int method597 (int var56, int var175) {
		if (var56>var175)
			return (var56*var175 + stvr);
		else
			return (var175*var56 - stvr);
	}
}

class mthdcls598 {

	static int stvr;

	static mthdcls598() {
		stvr = 155;
	}

	public int method598 (int var126, int var868) {
		if (var126>var868)
			return (var126-var868 + stvr);
		else
			return (var868-var126 - stvr);
	}
}

class mthdcls599 {

	static int stvr;

	static mthdcls599() {
		stvr = 151;
	}

	public int method599 (int var583, int var661) {
		if (var583>var661)
			return (var583+var661 + stvr);
		else
			return (var661+var583 - stvr);
	}
}

class mthdcls600 {

	static int stvr;

	static mthdcls600() {
		stvr = 14;
	}

	public int method600 (int var950, int var872) {
		if (var950>var872)
			return (var950-var872 + stvr);
		else
			return (var872-var950 - stvr);
	}
}

class mthdcls601 {

	static int stvr;

	static mthdcls601() {
		stvr = 525;
	}

	public int method601 (int var604, int var311) {
		if (var604>var311)
			return (var604*var311 + stvr);
		else
			return (var311*var604 - stvr);
	}
}

class mthdcls602 {

	static int stvr;

	static mthdcls602() {
		stvr = 529;
	}

	public int method602 (int var755, int var893) {
		if (var755>var893)
			return (var755*var893 + stvr);
		else
			return (var893*var755 - stvr);
	}
}

class mthdcls603 {

	static int stvr;

	static mthdcls603() {
		stvr = 924;
	}

	public int method603 (int var451, int var552) {
		if (var451>var552)
			return (var451*var552 + stvr);
		else
			return (var552*var451 - stvr);
	}
}

class mthdcls604 {

	static int stvr;

	static mthdcls604() {
		stvr = 391;
	}

	public int method604 (int var15, int var157) {
		if (var15>var157)
			return (var15+var157 + stvr);
		else
			return (var157+var15 - stvr);
	}
}

class mthdcls605 {

	static int stvr;

	static mthdcls605() {
		stvr = 723;
	}

	public int method605 (int var963, int var952) {
		if (var963>var952)
			return (var963*var952 + stvr);
		else
			return (var952*var963 - stvr);
	}
}

class mthdcls606 {

	static int stvr;

	static mthdcls606() {
		stvr = 327;
	}

	public int method606 (int var352, int var324) {
		if (var352>var324)
			return (var352-var324 + stvr);
		else
			return (var324-var352 - stvr);
	}
}

class mthdcls607 {

	static int stvr;

	static mthdcls607() {
		stvr = 686;
	}

	public int method607 (int var691, int var302) {
		if (var691>var302)
			return (var691+var302 + stvr);
		else
			return (var302+var691 - stvr);
	}
}

class mthdcls608 {

	static int stvr;

	static mthdcls608() {
		stvr = 683;
	}

	public int method608 (int var503, int var250) {
		if (var503>var250)
			return (var503+var250 + stvr);
		else
			return (var250+var503 - stvr);
	}
}

class mthdcls609 {

	static int stvr;

	static mthdcls609() {
		stvr = 333;
	}

	public int method609 (int var969, int var476) {
		if (var969>var476)
			return (var969+var476 + stvr);
		else
			return (var476+var969 - stvr);
	}
}

class mthdcls610 {

	static int stvr;

	static mthdcls610() {
		stvr = 900;
	}

	public int method610 (int var631, int var938) {
		if (var631>var938)
			return (var631+var938 + stvr);
		else
			return (var938+var631 - stvr);
	}
}

class mthdcls611 {

	static int stvr;

	static mthdcls611() {
		stvr = 614;
	}

	public int method611 (int var625, int var544) {
		if (var625>var544)
			return (var625*var544 + stvr);
		else
			return (var544*var625 - stvr);
	}
}

class mthdcls612 {

	static int stvr;

	static mthdcls612() {
		stvr = 830;
	}

	public int method612 (int var302, int var625) {
		if (var302>var625)
			return (var302+var625 + stvr);
		else
			return (var625+var302 - stvr);
	}
}

class mthdcls613 {

	static int stvr;

	static mthdcls613() {
		stvr = 537;
	}

	public int method613 (int var504, int var209) {
		if (var504>var209)
			return (var504+var209 + stvr);
		else
			return (var209+var504 - stvr);
	}
}

class mthdcls614 {

	static int stvr;

	static mthdcls614() {
		stvr = 136;
	}

	public int method614 (int var974, int var219) {
		if (var974>var219)
			return (var974+var219 + stvr);
		else
			return (var219+var974 - stvr);
	}
}

class mthdcls615 {

	static int stvr;

	static mthdcls615() {
		stvr = 545;
	}

	public int method615 (int var77, int var257) {
		if (var77>var257)
			return (var77-var257 + stvr);
		else
			return (var257-var77 - stvr);
	}
}

class mthdcls616 {

	static int stvr;

	static mthdcls616() {
		stvr = 665;
	}

	public int method616 (int var419, int var337) {
		if (var419>var337)
			return (var419-var337 + stvr);
		else
			return (var337-var419 - stvr);
	}
}

class mthdcls617 {

	static int stvr;

	static mthdcls617() {
		stvr = 554;
	}

	public int method617 (int var727, int var493) {
		if (var727>var493)
			return (var727*var493 + stvr);
		else
			return (var493*var727 - stvr);
	}
}

class mthdcls618 {

	static int stvr;

	static mthdcls618() {
		stvr = 993;
	}

	public int method618 (int var481, int var764) {
		if (var481>var764)
			return (var481+var764 + stvr);
		else
			return (var764+var481 - stvr);
	}
}

class mthdcls619 {

	static int stvr;

	static mthdcls619() {
		stvr = 895;
	}

	public int method619 (int var284, int var114) {
		if (var284>var114)
			return (var284-var114 + stvr);
		else
			return (var114-var284 - stvr);
	}
}

class mthdcls620 {

	static int stvr;

	static mthdcls620() {
		stvr = 756;
	}

	public int method620 (int var712, int var931) {
		if (var712>var931)
			return (var712+var931 + stvr);
		else
			return (var931+var712 - stvr);
	}
}

class mthdcls621 {

	static int stvr;

	static mthdcls621() {
		stvr = 489;
	}

	public int method621 (int var293, int var272) {
		if (var293>var272)
			return (var293*var272 + stvr);
		else
			return (var272*var293 - stvr);
	}
}

class mthdcls622 {

	static int stvr;

	static mthdcls622() {
		stvr = 605;
	}

	public int method622 (int var132, int var990) {
		if (var132>var990)
			return (var132*var990 + stvr);
		else
			return (var990*var132 - stvr);
	}
}

class mthdcls623 {

	static int stvr;

	static mthdcls623() {
		stvr = 96;
	}

	public int method623 (int var538, int var632) {
		if (var538>var632)
			return (var538*var632 + stvr);
		else
			return (var632*var538 - stvr);
	}
}

class mthdcls624 {

	static int stvr;

	static mthdcls624() {
		stvr = 30;
	}

	public int method624 (int var390, int var314) {
		if (var390>var314)
			return (var390-var314 + stvr);
		else
			return (var314-var390 - stvr);
	}
}

class mthdcls625 {

	static int stvr;

	static mthdcls625() {
		stvr = 318;
	}

	public int method625 (int var43, int var462) {
		if (var43>var462)
			return (var43+var462 + stvr);
		else
			return (var462+var43 - stvr);
	}
}

class mthdcls626 {

	static int stvr;

	static mthdcls626() {
		stvr = 362;
	}

	public int method626 (int var804, int var767) {
		if (var804>var767)
			return (var804*var767 + stvr);
		else
			return (var767*var804 - stvr);
	}
}

class mthdcls627 {

	static int stvr;

	static mthdcls627() {
		stvr = 382;
	}

	public int method627 (int var429, int var732) {
		if (var429>var732)
			return (var429*var732 + stvr);
		else
			return (var732*var429 - stvr);
	}
}

class mthdcls628 {

	static int stvr;

	static mthdcls628() {
		stvr = 342;
	}

	public int method628 (int var95, int var397) {
		if (var95>var397)
			return (var95+var397 + stvr);
		else
			return (var397+var95 - stvr);
	}
}

class mthdcls629 {

	static int stvr;

	static mthdcls629() {
		stvr = 563;
	}

	public int method629 (int var166, int var734) {
		if (var166>var734)
			return (var166-var734 + stvr);
		else
			return (var734-var166 - stvr);
	}
}

class mthdcls630 {

	static int stvr;

	static mthdcls630() {
		stvr = 196;
	}

	public int method630 (int var821, int var795) {
		if (var821>var795)
			return (var821*var795 + stvr);
		else
			return (var795*var821 - stvr);
	}
}

class mthdcls631 {

	static int stvr;

	static mthdcls631() {
		stvr = 698;
	}

	public int method631 (int var945, int var568) {
		if (var945>var568)
			return (var945+var568 + stvr);
		else
			return (var568+var945 - stvr);
	}
}

class mthdcls632 {

	static int stvr;

	static mthdcls632() {
		stvr = 439;
	}

	public int method632 (int var561, int var889) {
		if (var561>var889)
			return (var561*var889 + stvr);
		else
			return (var889*var561 - stvr);
	}
}

class mthdcls633 {

	static int stvr;

	static mthdcls633() {
		stvr = 702;
	}

	public int method633 (int var735, int var647) {
		if (var735>var647)
			return (var735+var647 + stvr);
		else
			return (var647+var735 - stvr);
	}
}

class mthdcls634 {

	static int stvr;

	static mthdcls634() {
		stvr = 608;
	}

	public int method634 (int var804, int var68) {
		if (var804>var68)
			return (var804+var68 + stvr);
		else
			return (var68+var804 - stvr);
	}
}

class mthdcls635 {

	static int stvr;

	static mthdcls635() {
		stvr = 714;
	}

	public int method635 (int var594, int var118) {
		if (var594>var118)
			return (var594+var118 + stvr);
		else
			return (var118+var594 - stvr);
	}
}

class mthdcls636 {

	static int stvr;

	static mthdcls636() {
		stvr = 338;
	}

	public int method636 (int var474, int var764) {
		if (var474>var764)
			return (var474*var764 + stvr);
		else
			return (var764*var474 - stvr);
	}
}

class mthdcls637 {

	static int stvr;

	static mthdcls637() {
		stvr = 304;
	}

	public int method637 (int var914, int var328) {
		if (var914>var328)
			return (var914-var328 + stvr);
		else
			return (var328-var914 - stvr);
	}
}

class mthdcls638 {

	static int stvr;

	static mthdcls638() {
		stvr = 484;
	}

	public int method638 (int var781, int var29) {
		if (var781>var29)
			return (var781+var29 + stvr);
		else
			return (var29+var781 - stvr);
	}
}

class mthdcls639 {

	static int stvr;

	static mthdcls639() {
		stvr = 649;
	}

	public int method639 (int var26, int var789) {
		if (var26>var789)
			return (var26-var789 + stvr);
		else
			return (var789-var26 - stvr);
	}
}

class mthdcls640 {

	static int stvr;

	static mthdcls640() {
		stvr = 593;
	}

	public int method640 (int var47, int var19) {
		if (var47>var19)
			return (var47*var19 + stvr);
		else
			return (var19*var47 - stvr);
	}
}

class mthdcls641 {

	static int stvr;

	static mthdcls641() {
		stvr = 158;
	}

	public int method641 (int var56, int var457) {
		if (var56>var457)
			return (var56+var457 + stvr);
		else
			return (var457+var56 - stvr);
	}
}

class mthdcls642 {

	static int stvr;

	static mthdcls642() {
		stvr = 63;
	}

	public int method642 (int var616, int var264) {
		if (var616>var264)
			return (var616+var264 + stvr);
		else
			return (var264+var616 - stvr);
	}
}

class mthdcls643 {

	static int stvr;

	static mthdcls643() {
		stvr = 582;
	}

	public int method643 (int var531, int var405) {
		if (var531>var405)
			return (var531+var405 + stvr);
		else
			return (var405+var531 - stvr);
	}
}

class mthdcls644 {

	static int stvr;

	static mthdcls644() {
		stvr = 85;
	}

	public int method644 (int var945, int var998) {
		if (var945>var998)
			return (var945*var998 + stvr);
		else
			return (var998*var945 - stvr);
	}
}

class mthdcls645 {

	static int stvr;

	static mthdcls645() {
		stvr = 421;
	}

	public int method645 (int var784, int var474) {
		if (var784>var474)
			return (var784-var474 + stvr);
		else
			return (var474-var784 - stvr);
	}
}

class mthdcls646 {

	static int stvr;

	static mthdcls646() {
		stvr = 28;
	}

	public int method646 (int var137, int var960) {
		if (var137>var960)
			return (var137-var960 + stvr);
		else
			return (var960-var137 - stvr);
	}
}

class mthdcls647 {

	static int stvr;

	static mthdcls647() {
		stvr = 460;
	}

	public int method647 (int var210, int var701) {
		if (var210>var701)
			return (var210*var701 + stvr);
		else
			return (var701*var210 - stvr);
	}
}

class mthdcls648 {

	static int stvr;

	static mthdcls648() {
		stvr = 566;
	}

	public int method648 (int var509, int var746) {
		if (var509>var746)
			return (var509*var746 + stvr);
		else
			return (var746*var509 - stvr);
	}
}

class mthdcls649 {

	static int stvr;

	static mthdcls649() {
		stvr = 376;
	}

	public int method649 (int var783, int var944) {
		if (var783>var944)
			return (var783+var944 + stvr);
		else
			return (var944+var783 - stvr);
	}
}

class mthdcls650 {

	static int stvr;

	static mthdcls650() {
		stvr = 999;
	}

	public int method650 (int var552, int var367) {
		if (var552>var367)
			return (var552-var367 + stvr);
		else
			return (var367-var552 - stvr);
	}
}

class mthdcls651 {

	static int stvr;

	static mthdcls651() {
		stvr = 167;
	}

	public int method651 (int var87, int var990) {
		if (var87>var990)
			return (var87-var990 + stvr);
		else
			return (var990-var87 - stvr);
	}
}

class mthdcls652 {

	static int stvr;

	static mthdcls652() {
		stvr = 998;
	}

	public int method652 (int var355, int var996) {
		if (var355>var996)
			return (var355-var996 + stvr);
		else
			return (var996-var355 - stvr);
	}
}

class mthdcls653 {

	static int stvr;

	static mthdcls653() {
		stvr = 824;
	}

	public int method653 (int var870, int var698) {
		if (var870>var698)
			return (var870+var698 + stvr);
		else
			return (var698+var870 - stvr);
	}
}

class mthdcls654 {

	static int stvr;

	static mthdcls654() {
		stvr = 871;
	}

	public int method654 (int var586, int var29) {
		if (var586>var29)
			return (var586*var29 + stvr);
		else
			return (var29*var586 - stvr);
	}
}

class mthdcls655 {

	static int stvr;

	static mthdcls655() {
		stvr = 696;
	}

	public int method655 (int var578, int var998) {
		if (var578>var998)
			return (var578-var998 + stvr);
		else
			return (var998-var578 - stvr);
	}
}

class mthdcls656 {

	static int stvr;

	static mthdcls656() {
		stvr = 919;
	}

	public int method656 (int var794, int var307) {
		if (var794>var307)
			return (var794+var307 + stvr);
		else
			return (var307+var794 - stvr);
	}
}

class mthdcls657 {

	static int stvr;

	static mthdcls657() {
		stvr = 441;
	}

	public int method657 (int var141, int var961) {
		if (var141>var961)
			return (var141-var961 + stvr);
		else
			return (var961-var141 - stvr);
	}
}

class mthdcls658 {

	static int stvr;

	static mthdcls658() {
		stvr = 738;
	}

	public int method658 (int var511, int var876) {
		if (var511>var876)
			return (var511*var876 + stvr);
		else
			return (var876*var511 - stvr);
	}
}

class mthdcls659 {

	static int stvr;

	static mthdcls659() {
		stvr = 364;
	}

	public int method659 (int var479, int var874) {
		if (var479>var874)
			return (var479-var874 + stvr);
		else
			return (var874-var479 - stvr);
	}
}

class mthdcls660 {

	static int stvr;

	static mthdcls660() {
		stvr = 919;
	}

	public int method660 (int var785, int var248) {
		if (var785>var248)
			return (var785-var248 + stvr);
		else
			return (var248-var785 - stvr);
	}
}

class mthdcls661 {

	static int stvr;

	static mthdcls661() {
		stvr = 183;
	}

	public int method661 (int var244, int var447) {
		if (var244>var447)
			return (var244-var447 + stvr);
		else
			return (var447-var244 - stvr);
	}
}

class mthdcls662 {

	static int stvr;

	static mthdcls662() {
		stvr = 776;
	}

	public int method662 (int var125, int var548) {
		if (var125>var548)
			return (var125+var548 + stvr);
		else
			return (var548+var125 - stvr);
	}
}

class mthdcls663 {

	static int stvr;

	static mthdcls663() {
		stvr = 934;
	}

	public int method663 (int var479, int var258) {
		if (var479>var258)
			return (var479*var258 + stvr);
		else
			return (var258*var479 - stvr);
	}
}

class mthdcls664 {

	static int stvr;

	static mthdcls664() {
		stvr = 429;
	}

	public int method664 (int var996, int var333) {
		if (var996>var333)
			return (var996*var333 + stvr);
		else
			return (var333*var996 - stvr);
	}
}

class mthdcls665 {

	static int stvr;

	static mthdcls665() {
		stvr = 219;
	}

	public int method665 (int var338, int var109) {
		if (var338>var109)
			return (var338-var109 + stvr);
		else
			return (var109-var338 - stvr);
	}
}

class mthdcls666 {

	static int stvr;

	static mthdcls666() {
		stvr = 179;
	}

	public int method666 (int var130, int var815) {
		if (var130>var815)
			return (var130+var815 + stvr);
		else
			return (var815+var130 - stvr);
	}
}

class mthdcls667 {

	static int stvr;

	static mthdcls667() {
		stvr = 231;
	}

	public int method667 (int var245, int var531) {
		if (var245>var531)
			return (var245-var531 + stvr);
		else
			return (var531-var245 - stvr);
	}
}

class mthdcls668 {

	static int stvr;

	static mthdcls668() {
		stvr = 365;
	}

	public int method668 (int var882, int var27) {
		if (var882>var27)
			return (var882+var27 + stvr);
		else
			return (var27+var882 - stvr);
	}
}

class mthdcls669 {

	static int stvr;

	static mthdcls669() {
		stvr = 595;
	}

	public int method669 (int var177, int var447) {
		if (var177>var447)
			return (var177+var447 + stvr);
		else
			return (var447+var177 - stvr);
	}
}

class mthdcls670 {

	static int stvr;

	static mthdcls670() {
		stvr = 903;
	}

	public int method670 (int var369, int var654) {
		if (var369>var654)
			return (var369+var654 + stvr);
		else
			return (var654+var369 - stvr);
	}
}

class mthdcls671 {

	static int stvr;

	static mthdcls671() {
		stvr = 973;
	}

	public int method671 (int var969, int var68) {
		if (var969>var68)
			return (var969+var68 + stvr);
		else
			return (var68+var969 - stvr);
	}
}

class mthdcls672 {

	static int stvr;

	static mthdcls672() {
		stvr = 688;
	}

	public int method672 (int var713, int var216) {
		if (var713>var216)
			return (var713*var216 + stvr);
		else
			return (var216*var713 - stvr);
	}
}

class mthdcls673 {

	static int stvr;

	static mthdcls673() {
		stvr = 939;
	}

	public int method673 (int var98, int var182) {
		if (var98>var182)
			return (var98+var182 + stvr);
		else
			return (var182+var98 - stvr);
	}
}

class mthdcls674 {

	static int stvr;

	static mthdcls674() {
		stvr = 179;
	}

	public int method674 (int var31, int var663) {
		if (var31>var663)
			return (var31+var663 + stvr);
		else
			return (var663+var31 - stvr);
	}
}

class mthdcls675 {

	static int stvr;

	static mthdcls675() {
		stvr = 632;
	}

	public int method675 (int var341, int var430) {
		if (var341>var430)
			return (var341+var430 + stvr);
		else
			return (var430+var341 - stvr);
	}
}

class mthdcls676 {

	static int stvr;

	static mthdcls676() {
		stvr = 500;
	}

	public int method676 (int var269, int var136) {
		if (var269>var136)
			return (var269+var136 + stvr);
		else
			return (var136+var269 - stvr);
	}
}

class mthdcls677 {

	static int stvr;

	static mthdcls677() {
		stvr = 56;
	}

	public int method677 (int var913, int var127) {
		if (var913>var127)
			return (var913*var127 + stvr);
		else
			return (var127*var913 - stvr);
	}
}

class mthdcls678 {

	static int stvr;

	static mthdcls678() {
		stvr = 361;
	}

	public int method678 (int var62, int var591) {
		if (var62>var591)
			return (var62-var591 + stvr);
		else
			return (var591-var62 - stvr);
	}
}

class mthdcls679 {

	static int stvr;

	static mthdcls679() {
		stvr = 463;
	}

	public int method679 (int var219, int var847) {
		if (var219>var847)
			return (var219-var847 + stvr);
		else
			return (var847-var219 - stvr);
	}
}

class mthdcls680 {

	static int stvr;

	static mthdcls680() {
		stvr = 900;
	}

	public int method680 (int var747, int var814) {
		if (var747>var814)
			return (var747-var814 + stvr);
		else
			return (var814-var747 - stvr);
	}
}

class mthdcls681 {

	static int stvr;

	static mthdcls681() {
		stvr = 939;
	}

	public int method681 (int var233, int var154) {
		if (var233>var154)
			return (var233+var154 + stvr);
		else
			return (var154+var233 - stvr);
	}
}

class mthdcls682 {

	static int stvr;

	static mthdcls682() {
		stvr = 655;
	}

	public int method682 (int var842, int var11) {
		if (var842>var11)
			return (var842+var11 + stvr);
		else
			return (var11+var842 - stvr);
	}
}

class mthdcls683 {

	static int stvr;

	static mthdcls683() {
		stvr = 336;
	}

	public int method683 (int var122, int var854) {
		if (var122>var854)
			return (var122+var854 + stvr);
		else
			return (var854+var122 - stvr);
	}
}

class mthdcls684 {

	static int stvr;

	static mthdcls684() {
		stvr = 288;
	}

	public int method684 (int var251, int var719) {
		if (var251>var719)
			return (var251-var719 + stvr);
		else
			return (var719-var251 - stvr);
	}
}

class mthdcls685 {

	static int stvr;

	static mthdcls685() {
		stvr = 765;
	}

	public int method685 (int var217, int var762) {
		if (var217>var762)
			return (var217+var762 + stvr);
		else
			return (var762+var217 - stvr);
	}
}

class mthdcls686 {

	static int stvr;

	static mthdcls686() {
		stvr = 721;
	}

	public int method686 (int var186, int var490) {
		if (var186>var490)
			return (var186*var490 + stvr);
		else
			return (var490*var186 - stvr);
	}
}

class mthdcls687 {

	static int stvr;

	static mthdcls687() {
		stvr = 615;
	}

	public int method687 (int var258, int var480) {
		if (var258>var480)
			return (var258-var480 + stvr);
		else
			return (var480-var258 - stvr);
	}
}

class mthdcls688 {

	static int stvr;

	static mthdcls688() {
		stvr = 295;
	}

	public int method688 (int var59, int var790) {
		if (var59>var790)
			return (var59+var790 + stvr);
		else
			return (var790+var59 - stvr);
	}
}

class mthdcls689 {

	static int stvr;

	static mthdcls689() {
		stvr = 968;
	}

	public int method689 (int var343, int var302) {
		if (var343>var302)
			return (var343+var302 + stvr);
		else
			return (var302+var343 - stvr);
	}
}

class mthdcls690 {

	static int stvr;

	static mthdcls690() {
		stvr = 463;
	}

	public int method690 (int var457, int var81) {
		if (var457>var81)
			return (var457+var81 + stvr);
		else
			return (var81+var457 - stvr);
	}
}

class mthdcls691 {

	static int stvr;

	static mthdcls691() {
		stvr = 740;
	}

	public int method691 (int var256, int var92) {
		if (var256>var92)
			return (var256+var92 + stvr);
		else
			return (var92+var256 - stvr);
	}
}

class mthdcls692 {

	static int stvr;

	static mthdcls692() {
		stvr = 407;
	}

	public int method692 (int var753, int var538) {
		if (var753>var538)
			return (var753*var538 + stvr);
		else
			return (var538*var753 - stvr);
	}
}

class mthdcls693 {

	static int stvr;

	static mthdcls693() {
		stvr = 746;
	}

	public int method693 (int var51, int var715) {
		if (var51>var715)
			return (var51*var715 + stvr);
		else
			return (var715*var51 - stvr);
	}
}

class mthdcls694 {

	static int stvr;

	static mthdcls694() {
		stvr = 380;
	}

	public int method694 (int var820, int var885) {
		if (var820>var885)
			return (var820-var885 + stvr);
		else
			return (var885-var820 - stvr);
	}
}

class mthdcls695 {

	static int stvr;

	static mthdcls695() {
		stvr = 589;
	}

	public int method695 (int var169, int var255) {
		if (var169>var255)
			return (var169*var255 + stvr);
		else
			return (var255*var169 - stvr);
	}
}

class mthdcls696 {

	static int stvr;

	static mthdcls696() {
		stvr = 849;
	}

	public int method696 (int var124, int var22) {
		if (var124>var22)
			return (var124-var22 + stvr);
		else
			return (var22-var124 - stvr);
	}
}

class mthdcls697 {

	static int stvr;

	static mthdcls697() {
		stvr = 573;
	}

	public int method697 (int var648, int var83) {
		if (var648>var83)
			return (var648+var83 + stvr);
		else
			return (var83+var648 - stvr);
	}
}

class mthdcls698 {

	static int stvr;

	static mthdcls698() {
		stvr = 313;
	}

	public int method698 (int var542, int var733) {
		if (var542>var733)
			return (var542-var733 + stvr);
		else
			return (var733-var542 - stvr);
	}
}

class mthdcls699 {

	static int stvr;

	static mthdcls699() {
		stvr = 680;
	}

	public int method699 (int var173, int var410) {
		if (var173>var410)
			return (var173+var410 + stvr);
		else
			return (var410+var173 - stvr);
	}
}

class mthdcls700 {

	static int stvr;

	static mthdcls700() {
		stvr = 865;
	}

	public int method700 (int var88, int var713) {
		if (var88>var713)
			return (var88+var713 + stvr);
		else
			return (var713+var88 - stvr);
	}
}

class mthdcls701 {

	static int stvr;

	static mthdcls701() {
		stvr = 109;
	}

	public int method701 (int var434, int var231) {
		if (var434>var231)
			return (var434-var231 + stvr);
		else
			return (var231-var434 - stvr);
	}
}

class mthdcls702 {

	static int stvr;

	static mthdcls702() {
		stvr = 54;
	}

	public int method702 (int var172, int var519) {
		if (var172>var519)
			return (var172-var519 + stvr);
		else
			return (var519-var172 - stvr);
	}
}

class mthdcls703 {

	static int stvr;

	static mthdcls703() {
		stvr = 964;
	}

	public int method703 (int var19, int var224) {
		if (var19>var224)
			return (var19*var224 + stvr);
		else
			return (var224*var19 - stvr);
	}
}

class mthdcls704 {

	static int stvr;

	static mthdcls704() {
		stvr = 789;
	}

	public int method704 (int var640, int var35) {
		if (var640>var35)
			return (var640+var35 + stvr);
		else
			return (var35+var640 - stvr);
	}
}

class mthdcls705 {

	static int stvr;

	static mthdcls705() {
		stvr = 563;
	}

	public int method705 (int var106, int var19) {
		if (var106>var19)
			return (var106-var19 + stvr);
		else
			return (var19-var106 - stvr);
	}
}

class mthdcls706 {

	static int stvr;

	static mthdcls706() {
		stvr = 88;
	}

	public int method706 (int var938, int var145) {
		if (var938>var145)
			return (var938-var145 + stvr);
		else
			return (var145-var938 - stvr);
	}
}

class mthdcls707 {

	static int stvr;

	static mthdcls707() {
		stvr = 381;
	}

	public int method707 (int var604, int var967) {
		if (var604>var967)
			return (var604*var967 + stvr);
		else
			return (var967*var604 - stvr);
	}
}

class mthdcls708 {

	static int stvr;

	static mthdcls708() {
		stvr = 367;
	}

	public int method708 (int var423, int var119) {
		if (var423>var119)
			return (var423-var119 + stvr);
		else
			return (var119-var423 - stvr);
	}
}

class mthdcls709 {

	static int stvr;

	static mthdcls709() {
		stvr = 424;
	}

	public int method709 (int var506, int var944) {
		if (var506>var944)
			return (var506-var944 + stvr);
		else
			return (var944-var506 - stvr);
	}
}

class mthdcls710 {

	static int stvr;

	static mthdcls710() {
		stvr = 84;
	}

	public int method710 (int var153, int var847) {
		if (var153>var847)
			return (var153-var847 + stvr);
		else
			return (var847-var153 - stvr);
	}
}

class mthdcls711 {

	static int stvr;

	static mthdcls711() {
		stvr = 404;
	}

	public int method711 (int var942, int var625) {
		if (var942>var625)
			return (var942+var625 + stvr);
		else
			return (var625+var942 - stvr);
	}
}

class mthdcls712 {

	static int stvr;

	static mthdcls712() {
		stvr = 858;
	}

	public int method712 (int var466, int var850) {
		if (var466>var850)
			return (var466*var850 + stvr);
		else
			return (var850*var466 - stvr);
	}
}

class mthdcls713 {

	static int stvr;

	static mthdcls713() {
		stvr = 670;
	}

	public int method713 (int var53, int var152) {
		if (var53>var152)
			return (var53-var152 + stvr);
		else
			return (var152-var53 - stvr);
	}
}

class mthdcls714 {

	static int stvr;

	static mthdcls714() {
		stvr = 522;
	}

	public int method714 (int var74, int var800) {
		if (var74>var800)
			return (var74-var800 + stvr);
		else
			return (var800-var74 - stvr);
	}
}

class mthdcls715 {

	static int stvr;

	static mthdcls715() {
		stvr = 348;
	}

	public int method715 (int var793, int var951) {
		if (var793>var951)
			return (var793-var951 + stvr);
		else
			return (var951-var793 - stvr);
	}
}

class mthdcls716 {

	static int stvr;

	static mthdcls716() {
		stvr = 711;
	}

	public int method716 (int var481, int var615) {
		if (var481>var615)
			return (var481*var615 + stvr);
		else
			return (var615*var481 - stvr);
	}
}

class mthdcls717 {

	static int stvr;

	static mthdcls717() {
		stvr = 765;
	}

	public int method717 (int var87, int var287) {
		if (var87>var287)
			return (var87*var287 + stvr);
		else
			return (var287*var87 - stvr);
	}
}

class mthdcls718 {

	static int stvr;

	static mthdcls718() {
		stvr = 719;
	}

	public int method718 (int var452, int var297) {
		if (var452>var297)
			return (var452-var297 + stvr);
		else
			return (var297-var452 - stvr);
	}
}

class mthdcls719 {

	static int stvr;

	static mthdcls719() {
		stvr = 996;
	}

	public int method719 (int var623, int var597) {
		if (var623>var597)
			return (var623*var597 + stvr);
		else
			return (var597*var623 - stvr);
	}
}

class mthdcls720 {

	static int stvr;

	static mthdcls720() {
		stvr = 9;
	}

	public int method720 (int var555, int var596) {
		if (var555>var596)
			return (var555-var596 + stvr);
		else
			return (var596-var555 - stvr);
	}
}

class mthdcls721 {

	static int stvr;

	static mthdcls721() {
		stvr = 905;
	}

	public int method721 (int var538, int var135) {
		if (var538>var135)
			return (var538-var135 + stvr);
		else
			return (var135-var538 - stvr);
	}
}

class mthdcls722 {

	static int stvr;

	static mthdcls722() {
		stvr = 162;
	}

	public int method722 (int var655, int var859) {
		if (var655>var859)
			return (var655+var859 + stvr);
		else
			return (var859+var655 - stvr);
	}
}

class mthdcls723 {

	static int stvr;

	static mthdcls723() {
		stvr = 204;
	}

	public int method723 (int var169, int var131) {
		if (var169>var131)
			return (var169*var131 + stvr);
		else
			return (var131*var169 - stvr);
	}
}

class mthdcls724 {

	static int stvr;

	static mthdcls724() {
		stvr = 433;
	}

	public int method724 (int var455, int var155) {
		if (var455>var155)
			return (var455-var155 + stvr);
		else
			return (var155-var455 - stvr);
	}
}

class mthdcls725 {

	static int stvr;

	static mthdcls725() {
		stvr = 138;
	}

	public int method725 (int var478, int var791) {
		if (var478>var791)
			return (var478*var791 + stvr);
		else
			return (var791*var478 - stvr);
	}
}

class mthdcls726 {

	static int stvr;

	static mthdcls726() {
		stvr = 957;
	}

	public int method726 (int var657, int var45) {
		if (var657>var45)
			return (var657*var45 + stvr);
		else
			return (var45*var657 - stvr);
	}
}

class mthdcls727 {

	static int stvr;

	static mthdcls727() {
		stvr = 772;
	}

	public int method727 (int var621, int var452) {
		if (var621>var452)
			return (var621*var452 + stvr);
		else
			return (var452*var621 - stvr);
	}
}

class mthdcls728 {

	static int stvr;

	static mthdcls728() {
		stvr = 548;
	}

	public int method728 (int var955, int var82) {
		if (var955>var82)
			return (var955-var82 + stvr);
		else
			return (var82-var955 - stvr);
	}
}

class mthdcls729 {

	static int stvr;

	static mthdcls729() {
		stvr = 70;
	}

	public int method729 (int var296, int var863) {
		if (var296>var863)
			return (var296-var863 + stvr);
		else
			return (var863-var296 - stvr);
	}
}

class mthdcls730 {

	static int stvr;

	static mthdcls730() {
		stvr = 981;
	}

	public int method730 (int var831, int var458) {
		if (var831>var458)
			return (var831-var458 + stvr);
		else
			return (var458-var831 - stvr);
	}
}

class mthdcls731 {

	static int stvr;

	static mthdcls731() {
		stvr = 981;
	}

	public int method731 (int var510, int var638) {
		if (var510>var638)
			return (var510+var638 + stvr);
		else
			return (var638+var510 - stvr);
	}
}

class mthdcls732 {

	static int stvr;

	static mthdcls732() {
		stvr = 690;
	}

	public int method732 (int var85, int var363) {
		if (var85>var363)
			return (var85-var363 + stvr);
		else
			return (var363-var85 - stvr);
	}
}

class mthdcls733 {

	static int stvr;

	static mthdcls733() {
		stvr = 514;
	}

	public int method733 (int var572, int var311) {
		if (var572>var311)
			return (var572-var311 + stvr);
		else
			return (var311-var572 - stvr);
	}
}

class mthdcls734 {

	static int stvr;

	static mthdcls734() {
		stvr = 590;
	}

	public int method734 (int var305, int var61) {
		if (var305>var61)
			return (var305-var61 + stvr);
		else
			return (var61-var305 - stvr);
	}
}

class mthdcls735 {

	static int stvr;

	static mthdcls735() {
		stvr = 806;
	}

	public int method735 (int var996, int var174) {
		if (var996>var174)
			return (var996*var174 + stvr);
		else
			return (var174*var996 - stvr);
	}
}

class mthdcls736 {

	static int stvr;

	static mthdcls736() {
		stvr = 158;
	}

	public int method736 (int var839, int var809) {
		if (var839>var809)
			return (var839*var809 + stvr);
		else
			return (var809*var839 - stvr);
	}
}

class mthdcls737 {

	static int stvr;

	static mthdcls737() {
		stvr = 447;
	}

	public int method737 (int var294, int var354) {
		if (var294>var354)
			return (var294-var354 + stvr);
		else
			return (var354-var294 - stvr);
	}
}

class mthdcls738 {

	static int stvr;

	static mthdcls738() {
		stvr = 291;
	}

	public int method738 (int var310, int var938) {
		if (var310>var938)
			return (var310-var938 + stvr);
		else
			return (var938-var310 - stvr);
	}
}

class mthdcls739 {

	static int stvr;

	static mthdcls739() {
		stvr = 955;
	}

	public int method739 (int var893, int var491) {
		if (var893>var491)
			return (var893+var491 + stvr);
		else
			return (var491+var893 - stvr);
	}
}

class mthdcls740 {

	static int stvr;

	static mthdcls740() {
		stvr = 777;
	}

	public int method740 (int var121, int var57) {
		if (var121>var57)
			return (var121*var57 + stvr);
		else
			return (var57*var121 - stvr);
	}
}

class mthdcls741 {

	static int stvr;

	static mthdcls741() {
		stvr = 698;
	}

	public int method741 (int var22, int var299) {
		if (var22>var299)
			return (var22*var299 + stvr);
		else
			return (var299*var22 - stvr);
	}
}

class mthdcls742 {

	static int stvr;

	static mthdcls742() {
		stvr = 386;
	}

	public int method742 (int var155, int var191) {
		if (var155>var191)
			return (var155*var191 + stvr);
		else
			return (var191*var155 - stvr);
	}
}

class mthdcls743 {

	static int stvr;

	static mthdcls743() {
		stvr = 143;
	}

	public int method743 (int var147, int var71) {
		if (var147>var71)
			return (var147+var71 + stvr);
		else
			return (var71+var147 - stvr);
	}
}

class mthdcls744 {

	static int stvr;

	static mthdcls744() {
		stvr = 630;
	}

	public int method744 (int var80, int var355) {
		if (var80>var355)
			return (var80*var355 + stvr);
		else
			return (var355*var80 - stvr);
	}
}

class mthdcls745 {

	static int stvr;

	static mthdcls745() {
		stvr = 508;
	}

	public int method745 (int var247, int var283) {
		if (var247>var283)
			return (var247+var283 + stvr);
		else
			return (var283+var247 - stvr);
	}
}

class mthdcls746 {

	static int stvr;

	static mthdcls746() {
		stvr = 701;
	}

	public int method746 (int var696, int var476) {
		if (var696>var476)
			return (var696*var476 + stvr);
		else
			return (var476*var696 - stvr);
	}
}

class mthdcls747 {

	static int stvr;

	static mthdcls747() {
		stvr = 9;
	}

	public int method747 (int var648, int var423) {
		if (var648>var423)
			return (var648+var423 + stvr);
		else
			return (var423+var648 - stvr);
	}
}

class mthdcls748 {

	static int stvr;

	static mthdcls748() {
		stvr = 513;
	}

	public int method748 (int var222, int var210) {
		if (var222>var210)
			return (var222*var210 + stvr);
		else
			return (var210*var222 - stvr);
	}
}

class mthdcls749 {

	static int stvr;

	static mthdcls749() {
		stvr = 270;
	}

	public int method749 (int var955, int var307) {
		if (var955>var307)
			return (var955*var307 + stvr);
		else
			return (var307*var955 - stvr);
	}
}

class mthdcls750 {

	static int stvr;

	static mthdcls750() {
		stvr = 149;
	}

	public int method750 (int var610, int var983) {
		if (var610>var983)
			return (var610*var983 + stvr);
		else
			return (var983*var610 - stvr);
	}
}

class mthdcls751 {

	static int stvr;

	static mthdcls751() {
		stvr = 546;
	}

	public int method751 (int var645, int var356) {
		if (var645>var356)
			return (var645+var356 + stvr);
		else
			return (var356+var645 - stvr);
	}
}

class mthdcls752 {

	static int stvr;

	static mthdcls752() {
		stvr = 836;
	}

	public int method752 (int var598, int var290) {
		if (var598>var290)
			return (var598+var290 + stvr);
		else
			return (var290+var598 - stvr);
	}
}

class mthdcls753 {

	static int stvr;

	static mthdcls753() {
		stvr = 328;
	}

	public int method753 (int var944, int var677) {
		if (var944>var677)
			return (var944-var677 + stvr);
		else
			return (var677-var944 - stvr);
	}
}

class mthdcls754 {

	static int stvr;

	static mthdcls754() {
		stvr = 142;
	}

	public int method754 (int var840, int var801) {
		if (var840>var801)
			return (var840-var801 + stvr);
		else
			return (var801-var840 - stvr);
	}
}

class mthdcls755 {

	static int stvr;

	static mthdcls755() {
		stvr = 488;
	}

	public int method755 (int var97, int var205) {
		if (var97>var205)
			return (var97*var205 + stvr);
		else
			return (var205*var97 - stvr);
	}
}

class mthdcls756 {

	static int stvr;

	static mthdcls756() {
		stvr = 802;
	}

	public int method756 (int var891, int var737) {
		if (var891>var737)
			return (var891+var737 + stvr);
		else
			return (var737+var891 - stvr);
	}
}

class mthdcls757 {

	static int stvr;

	static mthdcls757() {
		stvr = 972;
	}

	public int method757 (int var406, int var639) {
		if (var406>var639)
			return (var406-var639 + stvr);
		else
			return (var639-var406 - stvr);
	}
}

class mthdcls758 {

	static int stvr;

	static mthdcls758() {
		stvr = 610;
	}

	public int method758 (int var970, int var94) {
		if (var970>var94)
			return (var970-var94 + stvr);
		else
			return (var94-var970 - stvr);
	}
}

class mthdcls759 {

	static int stvr;

	static mthdcls759() {
		stvr = 907;
	}

	public int method759 (int var420, int var68) {
		if (var420>var68)
			return (var420*var68 + stvr);
		else
			return (var68*var420 - stvr);
	}
}

class mthdcls760 {

	static int stvr;

	static mthdcls760() {
		stvr = 13;
	}

	public int method760 (int var749, int var819) {
		if (var749>var819)
			return (var749*var819 + stvr);
		else
			return (var819*var749 - stvr);
	}
}

class mthdcls761 {

	static int stvr;

	static mthdcls761() {
		stvr = 479;
	}

	public int method761 (int var872, int var180) {
		if (var872>var180)
			return (var872*var180 + stvr);
		else
			return (var180*var872 - stvr);
	}
}

class mthdcls762 {

	static int stvr;

	static mthdcls762() {
		stvr = 154;
	}

	public int method762 (int var6, int var383) {
		if (var6>var383)
			return (var6*var383 + stvr);
		else
			return (var383*var6 - stvr);
	}
}

class mthdcls763 {

	static int stvr;

	static mthdcls763() {
		stvr = 442;
	}

	public int method763 (int var503, int var679) {
		if (var503>var679)
			return (var503*var679 + stvr);
		else
			return (var679*var503 - stvr);
	}
}

class mthdcls764 {

	static int stvr;

	static mthdcls764() {
		stvr = 199;
	}

	public int method764 (int var876, int var770) {
		if (var876>var770)
			return (var876+var770 + stvr);
		else
			return (var770+var876 - stvr);
	}
}

class mthdcls765 {

	static int stvr;

	static mthdcls765() {
		stvr = 41;
	}

	public int method765 (int var21, int var788) {
		if (var21>var788)
			return (var21*var788 + stvr);
		else
			return (var788*var21 - stvr);
	}
}

class mthdcls766 {

	static int stvr;

	static mthdcls766() {
		stvr = 487;
	}

	public int method766 (int var917, int var726) {
		if (var917>var726)
			return (var917+var726 + stvr);
		else
			return (var726+var917 - stvr);
	}
}

class mthdcls767 {

	static int stvr;

	static mthdcls767() {
		stvr = 527;
	}

	public int method767 (int var508, int var582) {
		if (var508>var582)
			return (var508-var582 + stvr);
		else
			return (var582-var508 - stvr);
	}
}

class mthdcls768 {

	static int stvr;

	static mthdcls768() {
		stvr = 779;
	}

	public int method768 (int var726, int var197) {
		if (var726>var197)
			return (var726+var197 + stvr);
		else
			return (var197+var726 - stvr);
	}
}

class mthdcls769 {

	static int stvr;

	static mthdcls769() {
		stvr = 863;
	}

	public int method769 (int var200, int var895) {
		if (var200>var895)
			return (var200+var895 + stvr);
		else
			return (var895+var200 - stvr);
	}
}

class mthdcls770 {

	static int stvr;

	static mthdcls770() {
		stvr = 737;
	}

	public int method770 (int var632, int var26) {
		if (var632>var26)
			return (var632*var26 + stvr);
		else
			return (var26*var632 - stvr);
	}
}

class mthdcls771 {

	static int stvr;

	static mthdcls771() {
		stvr = 114;
	}

	public int method771 (int var23, int var331) {
		if (var23>var331)
			return (var23*var331 + stvr);
		else
			return (var331*var23 - stvr);
	}
}

class mthdcls772 {

	static int stvr;

	static mthdcls772() {
		stvr = 975;
	}

	public int method772 (int var290, int var653) {
		if (var290>var653)
			return (var290-var653 + stvr);
		else
			return (var653-var290 - stvr);
	}
}

class mthdcls773 {

	static int stvr;

	static mthdcls773() {
		stvr = 181;
	}

	public int method773 (int var809, int var603) {
		if (var809>var603)
			return (var809+var603 + stvr);
		else
			return (var603+var809 - stvr);
	}
}

class mthdcls774 {

	static int stvr;

	static mthdcls774() {
		stvr = 764;
	}

	public int method774 (int var608, int var815) {
		if (var608>var815)
			return (var608+var815 + stvr);
		else
			return (var815+var608 - stvr);
	}
}

class mthdcls775 {

	static int stvr;

	static mthdcls775() {
		stvr = 124;
	}

	public int method775 (int var849, int var33) {
		if (var849>var33)
			return (var849*var33 + stvr);
		else
			return (var33*var849 - stvr);
	}
}

class mthdcls776 {

	static int stvr;

	static mthdcls776() {
		stvr = 835;
	}

	public int method776 (int var690, int var673) {
		if (var690>var673)
			return (var690*var673 + stvr);
		else
			return (var673*var690 - stvr);
	}
}

class mthdcls777 {

	static int stvr;

	static mthdcls777() {
		stvr = 126;
	}

	public int method777 (int var264, int var750) {
		if (var264>var750)
			return (var264*var750 + stvr);
		else
			return (var750*var264 - stvr);
	}
}

class mthdcls778 {

	static int stvr;

	static mthdcls778() {
		stvr = 807;
	}

	public int method778 (int var904, int var401) {
		if (var904>var401)
			return (var904-var401 + stvr);
		else
			return (var401-var904 - stvr);
	}
}

class mthdcls779 {

	static int stvr;

	static mthdcls779() {
		stvr = 100;
	}

	public int method779 (int var910, int var432) {
		if (var910>var432)
			return (var910*var432 + stvr);
		else
			return (var432*var910 - stvr);
	}
}

class mthdcls780 {

	static int stvr;

	static mthdcls780() {
		stvr = 853;
	}

	public int method780 (int var167, int var770) {
		if (var167>var770)
			return (var167+var770 + stvr);
		else
			return (var770+var167 - stvr);
	}
}

class mthdcls781 {

	static int stvr;

	static mthdcls781() {
		stvr = 496;
	}

	public int method781 (int var958, int var191) {
		if (var958>var191)
			return (var958*var191 + stvr);
		else
			return (var191*var958 - stvr);
	}
}

class mthdcls782 {

	static int stvr;

	static mthdcls782() {
		stvr = 845;
	}

	public int method782 (int var273, int var204) {
		if (var273>var204)
			return (var273*var204 + stvr);
		else
			return (var204*var273 - stvr);
	}
}

class mthdcls783 {

	static int stvr;

	static mthdcls783() {
		stvr = 437;
	}

	public int method783 (int var888, int var845) {
		if (var888>var845)
			return (var888*var845 + stvr);
		else
			return (var845*var888 - stvr);
	}
}

class mthdcls784 {

	static int stvr;

	static mthdcls784() {
		stvr = 993;
	}

	public int method784 (int var376, int var503) {
		if (var376>var503)
			return (var376*var503 + stvr);
		else
			return (var503*var376 - stvr);
	}
}

class mthdcls785 {

	static int stvr;

	static mthdcls785() {
		stvr = 780;
	}

	public int method785 (int var837, int var962) {
		if (var837>var962)
			return (var837+var962 + stvr);
		else
			return (var962+var837 - stvr);
	}
}

class mthdcls786 {

	static int stvr;

	static mthdcls786() {
		stvr = 815;
	}

	public int method786 (int var657, int var536) {
		if (var657>var536)
			return (var657-var536 + stvr);
		else
			return (var536-var657 - stvr);
	}
}

class mthdcls787 {

	static int stvr;

	static mthdcls787() {
		stvr = 714;
	}

	public int method787 (int var485, int var827) {
		if (var485>var827)
			return (var485*var827 + stvr);
		else
			return (var827*var485 - stvr);
	}
}

class mthdcls788 {

	static int stvr;

	static mthdcls788() {
		stvr = 794;
	}

	public int method788 (int var418, int var312) {
		if (var418>var312)
			return (var418-var312 + stvr);
		else
			return (var312-var418 - stvr);
	}
}

class mthdcls789 {

	static int stvr;

	static mthdcls789() {
		stvr = 792;
	}

	public int method789 (int var401, int var407) {
		if (var401>var407)
			return (var401+var407 + stvr);
		else
			return (var407+var401 - stvr);
	}
}

class mthdcls790 {

	static int stvr;

	static mthdcls790() {
		stvr = 245;
	}

	public int method790 (int var947, int var652) {
		if (var947>var652)
			return (var947-var652 + stvr);
		else
			return (var652-var947 - stvr);
	}
}

class mthdcls791 {

	static int stvr;

	static mthdcls791() {
		stvr = 313;
	}

	public int method791 (int var631, int var955) {
		if (var631>var955)
			return (var631-var955 + stvr);
		else
			return (var955-var631 - stvr);
	}
}

class mthdcls792 {

	static int stvr;

	static mthdcls792() {
		stvr = 279;
	}

	public int method792 (int var130, int var477) {
		if (var130>var477)
			return (var130-var477 + stvr);
		else
			return (var477-var130 - stvr);
	}
}

class mthdcls793 {

	static int stvr;

	static mthdcls793() {
		stvr = 298;
	}

	public int method793 (int var960, int var410) {
		if (var960>var410)
			return (var960-var410 + stvr);
		else
			return (var410-var960 - stvr);
	}
}

class mthdcls794 {

	static int stvr;

	static mthdcls794() {
		stvr = 698;
	}

	public int method794 (int var480, int var53) {
		if (var480>var53)
			return (var480*var53 + stvr);
		else
			return (var53*var480 - stvr);
	}
}

class mthdcls795 {

	static int stvr;

	static mthdcls795() {
		stvr = 632;
	}

	public int method795 (int var723, int var258) {
		if (var723>var258)
			return (var723+var258 + stvr);
		else
			return (var258+var723 - stvr);
	}
}

class mthdcls796 {

	static int stvr;

	static mthdcls796() {
		stvr = 216;
	}

	public int method796 (int var882, int var648) {
		if (var882>var648)
			return (var882-var648 + stvr);
		else
			return (var648-var882 - stvr);
	}
}

class mthdcls797 {

	static int stvr;

	static mthdcls797() {
		stvr = 381;
	}

	public int method797 (int var180, int var256) {
		if (var180>var256)
			return (var180-var256 + stvr);
		else
			return (var256-var180 - stvr);
	}
}

class mthdcls798 {

	static int stvr;

	static mthdcls798() {
		stvr = 301;
	}

	public int method798 (int var74, int var529) {
		if (var74>var529)
			return (var74-var529 + stvr);
		else
			return (var529-var74 - stvr);
	}
}

class mthdcls799 {

	static int stvr;

	static mthdcls799() {
		stvr = 655;
	}

	public int method799 (int var364, int var614) {
		if (var364>var614)
			return (var364+var614 + stvr);
		else
			return (var614+var364 - stvr);
	}
}

class mthdcls800 {

	static int stvr;

	static mthdcls800() {
		stvr = 825;
	}

	public int method800 (int var143, int var775) {
		if (var143>var775)
			return (var143-var775 + stvr);
		else
			return (var775-var143 - stvr);
	}
}

class mthdcls801 {

	static int stvr;

	static mthdcls801() {
		stvr = 728;
	}

	public int method801 (int var298, int var436) {
		if (var298>var436)
			return (var298-var436 + stvr);
		else
			return (var436-var298 - stvr);
	}
}

class mthdcls802 {

	static int stvr;

	static mthdcls802() {
		stvr = 828;
	}

	public int method802 (int var374, int var573) {
		if (var374>var573)
			return (var374*var573 + stvr);
		else
			return (var573*var374 - stvr);
	}
}

class mthdcls803 {

	static int stvr;

	static mthdcls803() {
		stvr = 685;
	}

	public int method803 (int var601, int var175) {
		if (var601>var175)
			return (var601*var175 + stvr);
		else
			return (var175*var601 - stvr);
	}
}

class mthdcls804 {

	static int stvr;

	static mthdcls804() {
		stvr = 522;
	}

	public int method804 (int var346, int var755) {
		if (var346>var755)
			return (var346-var755 + stvr);
		else
			return (var755-var346 - stvr);
	}
}

class mthdcls805 {

	static int stvr;

	static mthdcls805() {
		stvr = 125;
	}

	public int method805 (int var705, int var228) {
		if (var705>var228)
			return (var705+var228 + stvr);
		else
			return (var228+var705 - stvr);
	}
}

class mthdcls806 {

	static int stvr;

	static mthdcls806() {
		stvr = 968;
	}

	public int method806 (int var287, int var529) {
		if (var287>var529)
			return (var287*var529 + stvr);
		else
			return (var529*var287 - stvr);
	}
}

class mthdcls807 {

	static int stvr;

	static mthdcls807() {
		stvr = 112;
	}

	public int method807 (int var308, int var820) {
		if (var308>var820)
			return (var308*var820 + stvr);
		else
			return (var820*var308 - stvr);
	}
}

class mthdcls808 {

	static int stvr;

	static mthdcls808() {
		stvr = 342;
	}

	public int method808 (int var4, int var570) {
		if (var4>var570)
			return (var4+var570 + stvr);
		else
			return (var570+var4 - stvr);
	}
}

class mthdcls809 {

	static int stvr;

	static mthdcls809() {
		stvr = 874;
	}

	public int method809 (int var319, int var6) {
		if (var319>var6)
			return (var319*var6 + stvr);
		else
			return (var6*var319 - stvr);
	}
}

class mthdcls810 {

	static int stvr;

	static mthdcls810() {
		stvr = 825;
	}

	public int method810 (int var136, int var488) {
		if (var136>var488)
			return (var136*var488 + stvr);
		else
			return (var488*var136 - stvr);
	}
}

class mthdcls811 {

	static int stvr;

	static mthdcls811() {
		stvr = 725;
	}

	public int method811 (int var614, int var807) {
		if (var614>var807)
			return (var614+var807 + stvr);
		else
			return (var807+var614 - stvr);
	}
}

class mthdcls812 {

	static int stvr;

	static mthdcls812() {
		stvr = 476;
	}

	public int method812 (int var478, int var324) {
		if (var478>var324)
			return (var478-var324 + stvr);
		else
			return (var324-var478 - stvr);
	}
}

class mthdcls813 {

	static int stvr;

	static mthdcls813() {
		stvr = 30;
	}

	public int method813 (int var804, int var230) {
		if (var804>var230)
			return (var804+var230 + stvr);
		else
			return (var230+var804 - stvr);
	}
}

class mthdcls814 {

	static int stvr;

	static mthdcls814() {
		stvr = 571;
	}

	public int method814 (int var594, int var301) {
		if (var594>var301)
			return (var594-var301 + stvr);
		else
			return (var301-var594 - stvr);
	}
}

class mthdcls815 {

	static int stvr;

	static mthdcls815() {
		stvr = 221;
	}

	public int method815 (int var857, int var929) {
		if (var857>var929)
			return (var857+var929 + stvr);
		else
			return (var929+var857 - stvr);
	}
}

class mthdcls816 {

	static int stvr;

	static mthdcls816() {
		stvr = 346;
	}

	public int method816 (int var898, int var502) {
		if (var898>var502)
			return (var898+var502 + stvr);
		else
			return (var502+var898 - stvr);
	}
}

class mthdcls817 {

	static int stvr;

	static mthdcls817() {
		stvr = 398;
	}

	public int method817 (int var963, int var52) {
		if (var963>var52)
			return (var963*var52 + stvr);
		else
			return (var52*var963 - stvr);
	}
}

class mthdcls818 {

	static int stvr;

	static mthdcls818() {
		stvr = 122;
	}

	public int method818 (int var77, int var789) {
		if (var77>var789)
			return (var77*var789 + stvr);
		else
			return (var789*var77 - stvr);
	}
}

class mthdcls819 {

	static int stvr;

	static mthdcls819() {
		stvr = 993;
	}

	public int method819 (int var703, int var999) {
		if (var703>var999)
			return (var703*var999 + stvr);
		else
			return (var999*var703 - stvr);
	}
}

class mthdcls820 {

	static int stvr;

	static mthdcls820() {
		stvr = 144;
	}

	public int method820 (int var390, int var785) {
		if (var390>var785)
			return (var390*var785 + stvr);
		else
			return (var785*var390 - stvr);
	}
}

class mthdcls821 {

	static int stvr;

	static mthdcls821() {
		stvr = 997;
	}

	public int method821 (int var633, int var142) {
		if (var633>var142)
			return (var633*var142 + stvr);
		else
			return (var142*var633 - stvr);
	}
}

class mthdcls822 {

	static int stvr;

	static mthdcls822() {
		stvr = 487;
	}

	public int method822 (int var561, int var409) {
		if (var561>var409)
			return (var561+var409 + stvr);
		else
			return (var409+var561 - stvr);
	}
}

class mthdcls823 {

	static int stvr;

	static mthdcls823() {
		stvr = 168;
	}

	public int method823 (int var688, int var201) {
		if (var688>var201)
			return (var688*var201 + stvr);
		else
			return (var201*var688 - stvr);
	}
}

class mthdcls824 {

	static int stvr;

	static mthdcls824() {
		stvr = 785;
	}

	public int method824 (int var805, int var237) {
		if (var805>var237)
			return (var805+var237 + stvr);
		else
			return (var237+var805 - stvr);
	}
}

class mthdcls825 {

	static int stvr;

	static mthdcls825() {
		stvr = 321;
	}

	public int method825 (int var809, int var156) {
		if (var809>var156)
			return (var809-var156 + stvr);
		else
			return (var156-var809 - stvr);
	}
}

class mthdcls826 {

	static int stvr;

	static mthdcls826() {
		stvr = 915;
	}

	public int method826 (int var714, int var932) {
		if (var714>var932)
			return (var714+var932 + stvr);
		else
			return (var932+var714 - stvr);
	}
}

class mthdcls827 {

	static int stvr;

	static mthdcls827() {
		stvr = 554;
	}

	public int method827 (int var489, int var15) {
		if (var489>var15)
			return (var489+var15 + stvr);
		else
			return (var15+var489 - stvr);
	}
}

class mthdcls828 {

	static int stvr;

	static mthdcls828() {
		stvr = 328;
	}

	public int method828 (int var761, int var883) {
		if (var761>var883)
			return (var761*var883 + stvr);
		else
			return (var883*var761 - stvr);
	}
}

class mthdcls829 {

	static int stvr;

	static mthdcls829() {
		stvr = 389;
	}

	public int method829 (int var840, int var4) {
		if (var840>var4)
			return (var840+var4 + stvr);
		else
			return (var4+var840 - stvr);
	}
}

class mthdcls830 {

	static int stvr;

	static mthdcls830() {
		stvr = 434;
	}

	public int method830 (int var546, int var677) {
		if (var546>var677)
			return (var546*var677 + stvr);
		else
			return (var677*var546 - stvr);
	}
}

class mthdcls831 {

	static int stvr;

	static mthdcls831() {
		stvr = 149;
	}

	public int method831 (int var458, int var869) {
		if (var458>var869)
			return (var458+var869 + stvr);
		else
			return (var869+var458 - stvr);
	}
}

class mthdcls832 {

	static int stvr;

	static mthdcls832() {
		stvr = 91;
	}

	public int method832 (int var618, int var587) {
		if (var618>var587)
			return (var618+var587 + stvr);
		else
			return (var587+var618 - stvr);
	}
}

class mthdcls833 {

	static int stvr;

	static mthdcls833() {
		stvr = 409;
	}

	public int method833 (int var677, int var80) {
		if (var677>var80)
			return (var677-var80 + stvr);
		else
			return (var80-var677 - stvr);
	}
}

class mthdcls834 {

	static int stvr;

	static mthdcls834() {
		stvr = 414;
	}

	public int method834 (int var684, int var812) {
		if (var684>var812)
			return (var684-var812 + stvr);
		else
			return (var812-var684 - stvr);
	}
}

class mthdcls835 {

	static int stvr;

	static mthdcls835() {
		stvr = 459;
	}

	public int method835 (int var127, int var803) {
		if (var127>var803)
			return (var127*var803 + stvr);
		else
			return (var803*var127 - stvr);
	}
}

class mthdcls836 {

	static int stvr;

	static mthdcls836() {
		stvr = 376;
	}

	public int method836 (int var939, int var6) {
		if (var939>var6)
			return (var939*var6 + stvr);
		else
			return (var6*var939 - stvr);
	}
}

class mthdcls837 {

	static int stvr;

	static mthdcls837() {
		stvr = 916;
	}

	public int method837 (int var127, int var840) {
		if (var127>var840)
			return (var127*var840 + stvr);
		else
			return (var840*var127 - stvr);
	}
}

class mthdcls838 {

	static int stvr;

	static mthdcls838() {
		stvr = 946;
	}

	public int method838 (int var409, int var605) {
		if (var409>var605)
			return (var409+var605 + stvr);
		else
			return (var605+var409 - stvr);
	}
}

class mthdcls839 {

	static int stvr;

	static mthdcls839() {
		stvr = 950;
	}

	public int method839 (int var949, int var469) {
		if (var949>var469)
			return (var949-var469 + stvr);
		else
			return (var469-var949 - stvr);
	}
}

class mthdcls840 {

	static int stvr;

	static mthdcls840() {
		stvr = 197;
	}

	public int method840 (int var36, int var545) {
		if (var36>var545)
			return (var36-var545 + stvr);
		else
			return (var545-var36 - stvr);
	}
}

class mthdcls841 {

	static int stvr;

	static mthdcls841() {
		stvr = 176;
	}

	public int method841 (int var539, int var301) {
		if (var539>var301)
			return (var539*var301 + stvr);
		else
			return (var301*var539 - stvr);
	}
}

class mthdcls842 {

	static int stvr;

	static mthdcls842() {
		stvr = 127;
	}

	public int method842 (int var617, int var953) {
		if (var617>var953)
			return (var617*var953 + stvr);
		else
			return (var953*var617 - stvr);
	}
}

class mthdcls843 {

	static int stvr;

	static mthdcls843() {
		stvr = 268;
	}

	public int method843 (int var12, int var641) {
		if (var12>var641)
			return (var12-var641 + stvr);
		else
			return (var641-var12 - stvr);
	}
}

class mthdcls844 {

	static int stvr;

	static mthdcls844() {
		stvr = 944;
	}

	public int method844 (int var207, int var129) {
		if (var207>var129)
			return (var207-var129 + stvr);
		else
			return (var129-var207 - stvr);
	}
}

class mthdcls845 {

	static int stvr;

	static mthdcls845() {
		stvr = 754;
	}

	public int method845 (int var139, int var614) {
		if (var139>var614)
			return (var139-var614 + stvr);
		else
			return (var614-var139 - stvr);
	}
}

class mthdcls846 {

	static int stvr;

	static mthdcls846() {
		stvr = 432;
	}

	public int method846 (int var826, int var627) {
		if (var826>var627)
			return (var826+var627 + stvr);
		else
			return (var627+var826 - stvr);
	}
}

class mthdcls847 {

	static int stvr;

	static mthdcls847() {
		stvr = 56;
	}

	public int method847 (int var986, int var878) {
		if (var986>var878)
			return (var986-var878 + stvr);
		else
			return (var878-var986 - stvr);
	}
}

class mthdcls848 {

	static int stvr;

	static mthdcls848() {
		stvr = 540;
	}

	public int method848 (int var485, int var571) {
		if (var485>var571)
			return (var485+var571 + stvr);
		else
			return (var571+var485 - stvr);
	}
}

class mthdcls849 {

	static int stvr;

	static mthdcls849() {
		stvr = 394;
	}

	public int method849 (int var279, int var661) {
		if (var279>var661)
			return (var279+var661 + stvr);
		else
			return (var661+var279 - stvr);
	}
}

class mthdcls850 {

	static int stvr;

	static mthdcls850() {
		stvr = 323;
	}

	public int method850 (int var334, int var714) {
		if (var334>var714)
			return (var334-var714 + stvr);
		else
			return (var714-var334 - stvr);
	}
}

class mthdcls851 {

	static int stvr;

	static mthdcls851() {
		stvr = 420;
	}

	public int method851 (int var409, int var112) {
		if (var409>var112)
			return (var409*var112 + stvr);
		else
			return (var112*var409 - stvr);
	}
}

class mthdcls852 {

	static int stvr;

	static mthdcls852() {
		stvr = 810;
	}

	public int method852 (int var660, int var244) {
		if (var660>var244)
			return (var660+var244 + stvr);
		else
			return (var244+var660 - stvr);
	}
}

class mthdcls853 {

	static int stvr;

	static mthdcls853() {
		stvr = 129;
	}

	public int method853 (int var45, int var412) {
		if (var45>var412)
			return (var45+var412 + stvr);
		else
			return (var412+var45 - stvr);
	}
}

class mthdcls854 {

	static int stvr;

	static mthdcls854() {
		stvr = 491;
	}

	public int method854 (int var350, int var247) {
		if (var350>var247)
			return (var350+var247 + stvr);
		else
			return (var247+var350 - stvr);
	}
}

class mthdcls855 {

	static int stvr;

	static mthdcls855() {
		stvr = 552;
	}

	public int method855 (int var492, int var805) {
		if (var492>var805)
			return (var492*var805 + stvr);
		else
			return (var805*var492 - stvr);
	}
}

class mthdcls856 {

	static int stvr;

	static mthdcls856() {
		stvr = 573;
	}

	public int method856 (int var26, int var194) {
		if (var26>var194)
			return (var26+var194 + stvr);
		else
			return (var194+var26 - stvr);
	}
}

class mthdcls857 {

	static int stvr;

	static mthdcls857() {
		stvr = 694;
	}

	public int method857 (int var581, int var816) {
		if (var581>var816)
			return (var581+var816 + stvr);
		else
			return (var816+var581 - stvr);
	}
}

class mthdcls858 {

	static int stvr;

	static mthdcls858() {
		stvr = 155;
	}

	public int method858 (int var573, int var749) {
		if (var573>var749)
			return (var573+var749 + stvr);
		else
			return (var749+var573 - stvr);
	}
}

class mthdcls859 {

	static int stvr;

	static mthdcls859() {
		stvr = 648;
	}

	public int method859 (int var238, int var79) {
		if (var238>var79)
			return (var238*var79 + stvr);
		else
			return (var79*var238 - stvr);
	}
}

class mthdcls860 {

	static int stvr;

	static mthdcls860() {
		stvr = 71;
	}

	public int method860 (int var473, int var109) {
		if (var473>var109)
			return (var473*var109 + stvr);
		else
			return (var109*var473 - stvr);
	}
}

class mthdcls861 {

	static int stvr;

	static mthdcls861() {
		stvr = 462;
	}

	public int method861 (int var139, int var141) {
		if (var139>var141)
			return (var139+var141 + stvr);
		else
			return (var141+var139 - stvr);
	}
}

class mthdcls862 {

	static int stvr;

	static mthdcls862() {
		stvr = 571;
	}

	public int method862 (int var593, int var417) {
		if (var593>var417)
			return (var593+var417 + stvr);
		else
			return (var417+var593 - stvr);
	}
}

class mthdcls863 {

	static int stvr;

	static mthdcls863() {
		stvr = 57;
	}

	public int method863 (int var911, int var88) {
		if (var911>var88)
			return (var911+var88 + stvr);
		else
			return (var88+var911 - stvr);
	}
}

class mthdcls864 {

	static int stvr;

	static mthdcls864() {
		stvr = 819;
	}

	public int method864 (int var965, int var763) {
		if (var965>var763)
			return (var965+var763 + stvr);
		else
			return (var763+var965 - stvr);
	}
}

class mthdcls865 {

	static int stvr;

	static mthdcls865() {
		stvr = 351;
	}

	public int method865 (int var240, int var175) {
		if (var240>var175)
			return (var240-var175 + stvr);
		else
			return (var175-var240 - stvr);
	}
}

class mthdcls866 {

	static int stvr;

	static mthdcls866() {
		stvr = 572;
	}

	public int method866 (int var351, int var343) {
		if (var351>var343)
			return (var351+var343 + stvr);
		else
			return (var343+var351 - stvr);
	}
}

class mthdcls867 {

	static int stvr;

	static mthdcls867() {
		stvr = 824;
	}

	public int method867 (int var609, int var623) {
		if (var609>var623)
			return (var609+var623 + stvr);
		else
			return (var623+var609 - stvr);
	}
}

class mthdcls868 {

	static int stvr;

	static mthdcls868() {
		stvr = 893;
	}

	public int method868 (int var493, int var758) {
		if (var493>var758)
			return (var493*var758 + stvr);
		else
			return (var758*var493 - stvr);
	}
}

class mthdcls869 {

	static int stvr;

	static mthdcls869() {
		stvr = 180;
	}

	public int method869 (int var809, int var929) {
		if (var809>var929)
			return (var809-var929 + stvr);
		else
			return (var929-var809 - stvr);
	}
}

class mthdcls870 {

	static int stvr;

	static mthdcls870() {
		stvr = 213;
	}

	public int method870 (int var63, int var728) {
		if (var63>var728)
			return (var63+var728 + stvr);
		else
			return (var728+var63 - stvr);
	}
}

class mthdcls871 {

	static int stvr;

	static mthdcls871() {
		stvr = 397;
	}

	public int method871 (int var130, int var537) {
		if (var130>var537)
			return (var130*var537 + stvr);
		else
			return (var537*var130 - stvr);
	}
}

class mthdcls872 {

	static int stvr;

	static mthdcls872() {
		stvr = 792;
	}

	public int method872 (int var516, int var317) {
		if (var516>var317)
			return (var516-var317 + stvr);
		else
			return (var317-var516 - stvr);
	}
}

class mthdcls873 {

	static int stvr;

	static mthdcls873() {
		stvr = 412;
	}

	public int method873 (int var834, int var524) {
		if (var834>var524)
			return (var834*var524 + stvr);
		else
			return (var524*var834 - stvr);
	}
}

class mthdcls874 {

	static int stvr;

	static mthdcls874() {
		stvr = 443;
	}

	public int method874 (int var981, int var907) {
		if (var981>var907)
			return (var981*var907 + stvr);
		else
			return (var907*var981 - stvr);
	}
}

class mthdcls875 {

	static int stvr;

	static mthdcls875() {
		stvr = 59;
	}

	public int method875 (int var237, int var550) {
		if (var237>var550)
			return (var237-var550 + stvr);
		else
			return (var550-var237 - stvr);
	}
}

class mthdcls876 {

	static int stvr;

	static mthdcls876() {
		stvr = 253;
	}

	public int method876 (int var703, int var778) {
		if (var703>var778)
			return (var703-var778 + stvr);
		else
			return (var778-var703 - stvr);
	}
}

class mthdcls877 {

	static int stvr;

	static mthdcls877() {
		stvr = 94;
	}

	public int method877 (int var33, int var550) {
		if (var33>var550)
			return (var33-var550 + stvr);
		else
			return (var550-var33 - stvr);
	}
}

class mthdcls878 {

	static int stvr;

	static mthdcls878() {
		stvr = 541;
	}

	public int method878 (int var84, int var211) {
		if (var84>var211)
			return (var84*var211 + stvr);
		else
			return (var211*var84 - stvr);
	}
}

class mthdcls879 {

	static int stvr;

	static mthdcls879() {
		stvr = 789;
	}

	public int method879 (int var585, int var315) {
		if (var585>var315)
			return (var585-var315 + stvr);
		else
			return (var315-var585 - stvr);
	}
}

class mthdcls880 {

	static int stvr;

	static mthdcls880() {
		stvr = 779;
	}

	public int method880 (int var259, int var869) {
		if (var259>var869)
			return (var259-var869 + stvr);
		else
			return (var869-var259 - stvr);
	}
}

class mthdcls881 {

	static int stvr;

	static mthdcls881() {
		stvr = 626;
	}

	public int method881 (int var284, int var474) {
		if (var284>var474)
			return (var284+var474 + stvr);
		else
			return (var474+var284 - stvr);
	}
}

class mthdcls882 {

	static int stvr;

	static mthdcls882() {
		stvr = 877;
	}

	public int method882 (int var579, int var442) {
		if (var579>var442)
			return (var579+var442 + stvr);
		else
			return (var442+var579 - stvr);
	}
}

class mthdcls883 {

	static int stvr;

	static mthdcls883() {
		stvr = 505;
	}

	public int method883 (int var304, int var776) {
		if (var304>var776)
			return (var304+var776 + stvr);
		else
			return (var776+var304 - stvr);
	}
}

class mthdcls884 {

	static int stvr;

	static mthdcls884() {
		stvr = 880;
	}

	public int method884 (int var519, int var735) {
		if (var519>var735)
			return (var519*var735 + stvr);
		else
			return (var735*var519 - stvr);
	}
}

class mthdcls885 {

	static int stvr;

	static mthdcls885() {
		stvr = 838;
	}

	public int method885 (int var111, int var128) {
		if (var111>var128)
			return (var111*var128 + stvr);
		else
			return (var128*var111 - stvr);
	}
}

class mthdcls886 {

	static int stvr;

	static mthdcls886() {
		stvr = 332;
	}

	public int method886 (int var762, int var923) {
		if (var762>var923)
			return (var762+var923 + stvr);
		else
			return (var923+var762 - stvr);
	}
}

class mthdcls887 {

	static int stvr;

	static mthdcls887() {
		stvr = 364;
	}

	public int method887 (int var260, int var900) {
		if (var260>var900)
			return (var260+var900 + stvr);
		else
			return (var900+var260 - stvr);
	}
}

class mthdcls888 {

	static int stvr;

	static mthdcls888() {
		stvr = 136;
	}

	public int method888 (int var257, int var45) {
		if (var257>var45)
			return (var257+var45 + stvr);
		else
			return (var45+var257 - stvr);
	}
}

class mthdcls889 {

	static int stvr;

	static mthdcls889() {
		stvr = 487;
	}

	public int method889 (int var349, int var331) {
		if (var349>var331)
			return (var349*var331 + stvr);
		else
			return (var331*var349 - stvr);
	}
}

class mthdcls890 {

	static int stvr;

	static mthdcls890() {
		stvr = 677;
	}

	public int method890 (int var456, int var476) {
		if (var456>var476)
			return (var456*var476 + stvr);
		else
			return (var476*var456 - stvr);
	}
}

class mthdcls891 {

	static int stvr;

	static mthdcls891() {
		stvr = 917;
	}

	public int method891 (int var336, int var536) {
		if (var336>var536)
			return (var336*var536 + stvr);
		else
			return (var536*var336 - stvr);
	}
}

class mthdcls892 {

	static int stvr;

	static mthdcls892() {
		stvr = 802;
	}

	public int method892 (int var383, int var109) {
		if (var383>var109)
			return (var383+var109 + stvr);
		else
			return (var109+var383 - stvr);
	}
}

class mthdcls893 {

	static int stvr;

	static mthdcls893() {
		stvr = 927;
	}

	public int method893 (int var534, int var305) {
		if (var534>var305)
			return (var534*var305 + stvr);
		else
			return (var305*var534 - stvr);
	}
}

class mthdcls894 {

	static int stvr;

	static mthdcls894() {
		stvr = 339;
	}

	public int method894 (int var973, int var288) {
		if (var973>var288)
			return (var973-var288 + stvr);
		else
			return (var288-var973 - stvr);
	}
}

class mthdcls895 {

	static int stvr;

	static mthdcls895() {
		stvr = 690;
	}

	public int method895 (int var42, int var57) {
		if (var42>var57)
			return (var42-var57 + stvr);
		else
			return (var57-var42 - stvr);
	}
}

class mthdcls896 {

	static int stvr;

	static mthdcls896() {
		stvr = 662;
	}

	public int method896 (int var574, int var211) {
		if (var574>var211)
			return (var574-var211 + stvr);
		else
			return (var211-var574 - stvr);
	}
}

class mthdcls897 {

	static int stvr;

	static mthdcls897() {
		stvr = 334;
	}

	public int method897 (int var652, int var121) {
		if (var652>var121)
			return (var652-var121 + stvr);
		else
			return (var121-var652 - stvr);
	}
}

class mthdcls898 {

	static int stvr;

	static mthdcls898() {
		stvr = 164;
	}

	public int method898 (int var955, int var972) {
		if (var955>var972)
			return (var955-var972 + stvr);
		else
			return (var972-var955 - stvr);
	}
}

class mthdcls899 {

	static int stvr;

	static mthdcls899() {
		stvr = 587;
	}

	public int method899 (int var969, int var706) {
		if (var969>var706)
			return (var969+var706 + stvr);
		else
			return (var706+var969 - stvr);
	}
}

class mthdcls900 {

	static int stvr;

	static mthdcls900() {
		stvr = 900;
	}

	public int method900 (int var292, int var641) {
		if (var292>var641)
			return (var292-var641 + stvr);
		else
			return (var641-var292 - stvr);
	}
}

class mthdcls901 {

	static int stvr;

	static mthdcls901() {
		stvr = 298;
	}

	public int method901 (int var245, int var814) {
		if (var245>var814)
			return (var245*var814 + stvr);
		else
			return (var814*var245 - stvr);
	}
}

class mthdcls902 {

	static int stvr;

	static mthdcls902() {
		stvr = 338;
	}

	public int method902 (int var215, int var202) {
		if (var215>var202)
			return (var215*var202 + stvr);
		else
			return (var202*var215 - stvr);
	}
}

class mthdcls903 {

	static int stvr;

	static mthdcls903() {
		stvr = 391;
	}

	public int method903 (int var410, int var945) {
		if (var410>var945)
			return (var410*var945 + stvr);
		else
			return (var945*var410 - stvr);
	}
}

class mthdcls904 {

	static int stvr;

	static mthdcls904() {
		stvr = 91;
	}

	public int method904 (int var828, int var718) {
		if (var828>var718)
			return (var828-var718 + stvr);
		else
			return (var718-var828 - stvr);
	}
}

class mthdcls905 {

	static int stvr;

	static mthdcls905() {
		stvr = 958;
	}

	public int method905 (int var332, int var387) {
		if (var332>var387)
			return (var332+var387 + stvr);
		else
			return (var387+var332 - stvr);
	}
}

class mthdcls906 {

	static int stvr;

	static mthdcls906() {
		stvr = 458;
	}

	public int method906 (int var228, int var758) {
		if (var228>var758)
			return (var228+var758 + stvr);
		else
			return (var758+var228 - stvr);
	}
}

class mthdcls907 {

	static int stvr;

	static mthdcls907() {
		stvr = 381;
	}

	public int method907 (int var372, int var872) {
		if (var372>var872)
			return (var372-var872 + stvr);
		else
			return (var872-var372 - stvr);
	}
}

class mthdcls908 {

	static int stvr;

	static mthdcls908() {
		stvr = 90;
	}

	public int method908 (int var707, int var729) {
		if (var707>var729)
			return (var707*var729 + stvr);
		else
			return (var729*var707 - stvr);
	}
}

class mthdcls909 {

	static int stvr;

	static mthdcls909() {
		stvr = 610;
	}

	public int method909 (int var236, int var881) {
		if (var236>var881)
			return (var236*var881 + stvr);
		else
			return (var881*var236 - stvr);
	}
}

class mthdcls910 {

	static int stvr;

	static mthdcls910() {
		stvr = 871;
	}

	public int method910 (int var581, int var747) {
		if (var581>var747)
			return (var581+var747 + stvr);
		else
			return (var747+var581 - stvr);
	}
}

class mthdcls911 {

	static int stvr;

	static mthdcls911() {
		stvr = 127;
	}

	public int method911 (int var534, int var182) {
		if (var534>var182)
			return (var534+var182 + stvr);
		else
			return (var182+var534 - stvr);
	}
}

class mthdcls912 {

	static int stvr;

	static mthdcls912() {
		stvr = 696;
	}

	public int method912 (int var372, int var432) {
		if (var372>var432)
			return (var372+var432 + stvr);
		else
			return (var432+var372 - stvr);
	}
}

class mthdcls913 {

	static int stvr;

	static mthdcls913() {
		stvr = 73;
	}

	public int method913 (int var485, int var111) {
		if (var485>var111)
			return (var485*var111 + stvr);
		else
			return (var111*var485 - stvr);
	}
}

class mthdcls914 {

	static int stvr;

	static mthdcls914() {
		stvr = 100;
	}

	public int method914 (int var529, int var334) {
		if (var529>var334)
			return (var529+var334 + stvr);
		else
			return (var334+var529 - stvr);
	}
}

class mthdcls915 {

	static int stvr;

	static mthdcls915() {
		stvr = 122;
	}

	public int method915 (int var80, int var847) {
		if (var80>var847)
			return (var80*var847 + stvr);
		else
			return (var847*var80 - stvr);
	}
}

class mthdcls916 {

	static int stvr;

	static mthdcls916() {
		stvr = 148;
	}

	public int method916 (int var150, int var259) {
		if (var150>var259)
			return (var150*var259 + stvr);
		else
			return (var259*var150 - stvr);
	}
}

class mthdcls917 {

	static int stvr;

	static mthdcls917() {
		stvr = 774;
	}

	public int method917 (int var795, int var62) {
		if (var795>var62)
			return (var795-var62 + stvr);
		else
			return (var62-var795 - stvr);
	}
}

class mthdcls918 {

	static int stvr;

	static mthdcls918() {
		stvr = 167;
	}

	public int method918 (int var260, int var799) {
		if (var260>var799)
			return (var260*var799 + stvr);
		else
			return (var799*var260 - stvr);
	}
}

class mthdcls919 {

	static int stvr;

	static mthdcls919() {
		stvr = 268;
	}

	public int method919 (int var372, int var628) {
		if (var372>var628)
			return (var372-var628 + stvr);
		else
			return (var628-var372 - stvr);
	}
}

class mthdcls920 {

	static int stvr;

	static mthdcls920() {
		stvr = 475;
	}

	public int method920 (int var389, int var758) {
		if (var389>var758)
			return (var389+var758 + stvr);
		else
			return (var758+var389 - stvr);
	}
}

class mthdcls921 {

	static int stvr;

	static mthdcls921() {
		stvr = 616;
	}

	public int method921 (int var321, int var599) {
		if (var321>var599)
			return (var321-var599 + stvr);
		else
			return (var599-var321 - stvr);
	}
}

class mthdcls922 {

	static int stvr;

	static mthdcls922() {
		stvr = 461;
	}

	public int method922 (int var472, int var407) {
		if (var472>var407)
			return (var472+var407 + stvr);
		else
			return (var407+var472 - stvr);
	}
}

class mthdcls923 {

	static int stvr;

	static mthdcls923() {
		stvr = 634;
	}

	public int method923 (int var573, int var264) {
		if (var573>var264)
			return (var573-var264 + stvr);
		else
			return (var264-var573 - stvr);
	}
}

class mthdcls924 {

	static int stvr;

	static mthdcls924() {
		stvr = 539;
	}

	public int method924 (int var856, int var843) {
		if (var856>var843)
			return (var856*var843 + stvr);
		else
			return (var843*var856 - stvr);
	}
}

class mthdcls925 {

	static int stvr;

	static mthdcls925() {
		stvr = 396;
	}

	public int method925 (int var770, int var859) {
		if (var770>var859)
			return (var770-var859 + stvr);
		else
			return (var859-var770 - stvr);
	}
}

class mthdcls926 {

	static int stvr;

	static mthdcls926() {
		stvr = 649;
	}

	public int method926 (int var481, int var231) {
		if (var481>var231)
			return (var481-var231 + stvr);
		else
			return (var231-var481 - stvr);
	}
}

class mthdcls927 {

	static int stvr;

	static mthdcls927() {
		stvr = 422;
	}

	public int method927 (int var742, int var798) {
		if (var742>var798)
			return (var742*var798 + stvr);
		else
			return (var798*var742 - stvr);
	}
}

class mthdcls928 {

	static int stvr;

	static mthdcls928() {
		stvr = 109;
	}

	public int method928 (int var530, int var427) {
		if (var530>var427)
			return (var530+var427 + stvr);
		else
			return (var427+var530 - stvr);
	}
}

class mthdcls929 {

	static int stvr;

	static mthdcls929() {
		stvr = 939;
	}

	public int method929 (int var417, int var259) {
		if (var417>var259)
			return (var417+var259 + stvr);
		else
			return (var259+var417 - stvr);
	}
}

class mthdcls930 {

	static int stvr;

	static mthdcls930() {
		stvr = 718;
	}

	public int method930 (int var513, int var232) {
		if (var513>var232)
			return (var513*var232 + stvr);
		else
			return (var232*var513 - stvr);
	}
}

class mthdcls931 {

	static int stvr;

	static mthdcls931() {
		stvr = 963;
	}

	public int method931 (int var157, int var108) {
		if (var157>var108)
			return (var157-var108 + stvr);
		else
			return (var108-var157 - stvr);
	}
}

class mthdcls932 {

	static int stvr;

	static mthdcls932() {
		stvr = 561;
	}

	public int method932 (int var523, int var176) {
		if (var523>var176)
			return (var523-var176 + stvr);
		else
			return (var176-var523 - stvr);
	}
}

class mthdcls933 {

	static int stvr;

	static mthdcls933() {
		stvr = 529;
	}

	public int method933 (int var44, int var298) {
		if (var44>var298)
			return (var44-var298 + stvr);
		else
			return (var298-var44 - stvr);
	}
}

class mthdcls934 {

	static int stvr;

	static mthdcls934() {
		stvr = 723;
	}

	public int method934 (int var313, int var325) {
		if (var313>var325)
			return (var313-var325 + stvr);
		else
			return (var325-var313 - stvr);
	}
}

class mthdcls935 {

	static int stvr;

	static mthdcls935() {
		stvr = 417;
	}

	public int method935 (int var623, int var124) {
		if (var623>var124)
			return (var623+var124 + stvr);
		else
			return (var124+var623 - stvr);
	}
}

class mthdcls936 {

	static int stvr;

	static mthdcls936() {
		stvr = 805;
	}

	public int method936 (int var661, int var895) {
		if (var661>var895)
			return (var661*var895 + stvr);
		else
			return (var895*var661 - stvr);
	}
}

class mthdcls937 {

	static int stvr;

	static mthdcls937() {
		stvr = 98;
	}

	public int method937 (int var305, int var669) {
		if (var305>var669)
			return (var305-var669 + stvr);
		else
			return (var669-var305 - stvr);
	}
}

class mthdcls938 {

	static int stvr;

	static mthdcls938() {
		stvr = 924;
	}

	public int method938 (int var443, int var268) {
		if (var443>var268)
			return (var443*var268 + stvr);
		else
			return (var268*var443 - stvr);
	}
}

class mthdcls939 {

	static int stvr;

	static mthdcls939() {
		stvr = 691;
	}

	public int method939 (int var205, int var704) {
		if (var205>var704)
			return (var205+var704 + stvr);
		else
			return (var704+var205 - stvr);
	}
}

class mthdcls940 {

	static int stvr;

	static mthdcls940() {
		stvr = 215;
	}

	public int method940 (int var292, int var842) {
		if (var292>var842)
			return (var292+var842 + stvr);
		else
			return (var842+var292 - stvr);
	}
}

class mthdcls941 {

	static int stvr;

	static mthdcls941() {
		stvr = 697;
	}

	public int method941 (int var617, int var427) {
		if (var617>var427)
			return (var617*var427 + stvr);
		else
			return (var427*var617 - stvr);
	}
}

class mthdcls942 {

	static int stvr;

	static mthdcls942() {
		stvr = 543;
	}

	public int method942 (int var488, int var10) {
		if (var488>var10)
			return (var488-var10 + stvr);
		else
			return (var10-var488 - stvr);
	}
}

class mthdcls943 {

	static int stvr;

	static mthdcls943() {
		stvr = 290;
	}

	public int method943 (int var254, int var252) {
		if (var254>var252)
			return (var254+var252 + stvr);
		else
			return (var252+var254 - stvr);
	}
}

class mthdcls944 {

	static int stvr;

	static mthdcls944() {
		stvr = 818;
	}

	public int method944 (int var340, int var607) {
		if (var340>var607)
			return (var340+var607 + stvr);
		else
			return (var607+var340 - stvr);
	}
}

class mthdcls945 {

	static int stvr;

	static mthdcls945() {
		stvr = 805;
	}

	public int method945 (int var471, int var109) {
		if (var471>var109)
			return (var471+var109 + stvr);
		else
			return (var109+var471 - stvr);
	}
}

class mthdcls946 {

	static int stvr;

	static mthdcls946() {
		stvr = 181;
	}

	public int method946 (int var196, int var427) {
		if (var196>var427)
			return (var196-var427 + stvr);
		else
			return (var427-var196 - stvr);
	}
}

class mthdcls947 {

	static int stvr;

	static mthdcls947() {
		stvr = 284;
	}

	public int method947 (int var651, int var352) {
		if (var651>var352)
			return (var651+var352 + stvr);
		else
			return (var352+var651 - stvr);
	}
}

class mthdcls948 {

	static int stvr;

	static mthdcls948() {
		stvr = 155;
	}

	public int method948 (int var53, int var378) {
		if (var53>var378)
			return (var53-var378 + stvr);
		else
			return (var378-var53 - stvr);
	}
}

class mthdcls949 {

	static int stvr;

	static mthdcls949() {
		stvr = 30;
	}

	public int method949 (int var836, int var449) {
		if (var836>var449)
			return (var836+var449 + stvr);
		else
			return (var449+var836 - stvr);
	}
}

class mthdcls950 {

	static int stvr;

	static mthdcls950() {
		stvr = 859;
	}

	public int method950 (int var95, int var899) {
		if (var95>var899)
			return (var95*var899 + stvr);
		else
			return (var899*var95 - stvr);
	}
}

class mthdcls951 {

	static int stvr;

	static mthdcls951() {
		stvr = 689;
	}

	public int method951 (int var864, int var660) {
		if (var864>var660)
			return (var864-var660 + stvr);
		else
			return (var660-var864 - stvr);
	}
}

class mthdcls952 {

	static int stvr;

	static mthdcls952() {
		stvr = 761;
	}

	public int method952 (int var265, int var142) {
		if (var265>var142)
			return (var265-var142 + stvr);
		else
			return (var142-var265 - stvr);
	}
}

class mthdcls953 {

	static int stvr;

	static mthdcls953() {
		stvr = 10;
	}

	public int method953 (int var109, int var854) {
		if (var109>var854)
			return (var109+var854 + stvr);
		else
			return (var854+var109 - stvr);
	}
}

class mthdcls954 {

	static int stvr;

	static mthdcls954() {
		stvr = 511;
	}

	public int method954 (int var805, int var222) {
		if (var805>var222)
			return (var805+var222 + stvr);
		else
			return (var222+var805 - stvr);
	}
}

class mthdcls955 {

	static int stvr;

	static mthdcls955() {
		stvr = 711;
	}

	public int method955 (int var440, int var748) {
		if (var440>var748)
			return (var440-var748 + stvr);
		else
			return (var748-var440 - stvr);
	}
}

class mthdcls956 {

	static int stvr;

	static mthdcls956() {
		stvr = 391;
	}

	public int method956 (int var810, int var420) {
		if (var810>var420)
			return (var810+var420 + stvr);
		else
			return (var420+var810 - stvr);
	}
}

class mthdcls957 {

	static int stvr;

	static mthdcls957() {
		stvr = 578;
	}

	public int method957 (int var53, int var665) {
		if (var53>var665)
			return (var53-var665 + stvr);
		else
			return (var665-var53 - stvr);
	}
}

class mthdcls958 {

	static int stvr;

	static mthdcls958() {
		stvr = 492;
	}

	public int method958 (int var796, int var341) {
		if (var796>var341)
			return (var796+var341 + stvr);
		else
			return (var341+var796 - stvr);
	}
}

class mthdcls959 {

	static int stvr;

	static mthdcls959() {
		stvr = 348;
	}

	public int method959 (int var830, int var867) {
		if (var830>var867)
			return (var830+var867 + stvr);
		else
			return (var867+var830 - stvr);
	}
}

class mthdcls960 {

	static int stvr;

	static mthdcls960() {
		stvr = 543;
	}

	public int method960 (int var87, int var738) {
		if (var87>var738)
			return (var87*var738 + stvr);
		else
			return (var738*var87 - stvr);
	}
}

class mthdcls961 {

	static int stvr;

	static mthdcls961() {
		stvr = 961;
	}

	public int method961 (int var675, int var507) {
		if (var675>var507)
			return (var675+var507 + stvr);
		else
			return (var507+var675 - stvr);
	}
}

class mthdcls962 {

	static int stvr;

	static mthdcls962() {
		stvr = 309;
	}

	public int method962 (int var199, int var81) {
		if (var199>var81)
			return (var199*var81 + stvr);
		else
			return (var81*var199 - stvr);
	}
}

class mthdcls963 {

	static int stvr;

	static mthdcls963() {
		stvr = 75;
	}

	public int method963 (int var923, int var650) {
		if (var923>var650)
			return (var923+var650 + stvr);
		else
			return (var650+var923 - stvr);
	}
}

class mthdcls964 {

	static int stvr;

	static mthdcls964() {
		stvr = 267;
	}

	public int method964 (int var242, int var505) {
		if (var242>var505)
			return (var242-var505 + stvr);
		else
			return (var505-var242 - stvr);
	}
}

class mthdcls965 {

	static int stvr;

	static mthdcls965() {
		stvr = 358;
	}

	public int method965 (int var67, int var679) {
		if (var67>var679)
			return (var67-var679 + stvr);
		else
			return (var679-var67 - stvr);
	}
}

class mthdcls966 {

	static int stvr;

	static mthdcls966() {
		stvr = 942;
	}

	public int method966 (int var933, int var786) {
		if (var933>var786)
			return (var933*var786 + stvr);
		else
			return (var786*var933 - stvr);
	}
}

class mthdcls967 {

	static int stvr;

	static mthdcls967() {
		stvr = 373;
	}

	public int method967 (int var729, int var111) {
		if (var729>var111)
			return (var729+var111 + stvr);
		else
			return (var111+var729 - stvr);
	}
}

class mthdcls968 {

	static int stvr;

	static mthdcls968() {
		stvr = 803;
	}

	public int method968 (int var403, int var590) {
		if (var403>var590)
			return (var403*var590 + stvr);
		else
			return (var590*var403 - stvr);
	}
}

class mthdcls969 {

	static int stvr;

	static mthdcls969() {
		stvr = 989;
	}

	public int method969 (int var290, int var74) {
		if (var290>var74)
			return (var290-var74 + stvr);
		else
			return (var74-var290 - stvr);
	}
}

class mthdcls970 {

	static int stvr;

	static mthdcls970() {
		stvr = 868;
	}

	public int method970 (int var151, int var508) {
		if (var151>var508)
			return (var151-var508 + stvr);
		else
			return (var508-var151 - stvr);
	}
}

class mthdcls971 {

	static int stvr;

	static mthdcls971() {
		stvr = 409;
	}

	public int method971 (int var301, int var795) {
		if (var301>var795)
			return (var301+var795 + stvr);
		else
			return (var795+var301 - stvr);
	}
}

class mthdcls972 {

	static int stvr;

	static mthdcls972() {
		stvr = 201;
	}

	public int method972 (int var563, int var134) {
		if (var563>var134)
			return (var563-var134 + stvr);
		else
			return (var134-var563 - stvr);
	}
}

class mthdcls973 {

	static int stvr;

	static mthdcls973() {
		stvr = 243;
	}

	public int method973 (int var608, int var278) {
		if (var608>var278)
			return (var608+var278 + stvr);
		else
			return (var278+var608 - stvr);
	}
}

class mthdcls974 {

	static int stvr;

	static mthdcls974() {
		stvr = 549;
	}

	public int method974 (int var849, int var661) {
		if (var849>var661)
			return (var849*var661 + stvr);
		else
			return (var661*var849 - stvr);
	}
}

class mthdcls975 {

	static int stvr;

	static mthdcls975() {
		stvr = 640;
	}

	public int method975 (int var733, int var637) {
		if (var733>var637)
			return (var733*var637 + stvr);
		else
			return (var637*var733 - stvr);
	}
}

class mthdcls976 {

	static int stvr;

	static mthdcls976() {
		stvr = 632;
	}

	public int method976 (int var271, int var269) {
		if (var271>var269)
			return (var271+var269 + stvr);
		else
			return (var269+var271 - stvr);
	}
}

class mthdcls977 {

	static int stvr;

	static mthdcls977() {
		stvr = 589;
	}

	public int method977 (int var799, int var584) {
		if (var799>var584)
			return (var799*var584 + stvr);
		else
			return (var584*var799 - stvr);
	}
}

class mthdcls978 {

	static int stvr;

	static mthdcls978() {
		stvr = 507;
	}

	public int method978 (int var450, int var867) {
		if (var450>var867)
			return (var450+var867 + stvr);
		else
			return (var867+var450 - stvr);
	}
}

class mthdcls979 {

	static int stvr;

	static mthdcls979() {
		stvr = 787;
	}

	public int method979 (int var742, int var41) {
		if (var742>var41)
			return (var742+var41 + stvr);
		else
			return (var41+var742 - stvr);
	}
}

class mthdcls980 {

	static int stvr;

	static mthdcls980() {
		stvr = 840;
	}

	public int method980 (int var653, int var433) {
		if (var653>var433)
			return (var653-var433 + stvr);
		else
			return (var433-var653 - stvr);
	}
}

class mthdcls981 {

	static int stvr;

	static mthdcls981() {
		stvr = 984;
	}

	public int method981 (int var882, int var876) {
		if (var882>var876)
			return (var882*var876 + stvr);
		else
			return (var876*var882 - stvr);
	}
}

class mthdcls982 {

	static int stvr;

	static mthdcls982() {
		stvr = 225;
	}

	public int method982 (int var716, int var206) {
		if (var716>var206)
			return (var716+var206 + stvr);
		else
			return (var206+var716 - stvr);
	}
}

class mthdcls983 {

	static int stvr;

	static mthdcls983() {
		stvr = 289;
	}

	public int method983 (int var696, int var627) {
		if (var696>var627)
			return (var696+var627 + stvr);
		else
			return (var627+var696 - stvr);
	}
}

class mthdcls984 {

	static int stvr;

	static mthdcls984() {
		stvr = 340;
	}

	public int method984 (int var567, int var491) {
		if (var567>var491)
			return (var567-var491 + stvr);
		else
			return (var491-var567 - stvr);
	}
}

class mthdcls985 {

	static int stvr;

	static mthdcls985() {
		stvr = 883;
	}

	public int method985 (int var415, int var820) {
		if (var415>var820)
			return (var415-var820 + stvr);
		else
			return (var820-var415 - stvr);
	}
}

class mthdcls986 {

	static int stvr;

	static mthdcls986() {
		stvr = 565;
	}

	public int method986 (int var857, int var653) {
		if (var857>var653)
			return (var857-var653 + stvr);
		else
			return (var653-var857 - stvr);
	}
}

class mthdcls987 {

	static int stvr;

	static mthdcls987() {
		stvr = 725;
	}

	public int method987 (int var65, int var44) {
		if (var65>var44)
			return (var65+var44 + stvr);
		else
			return (var44+var65 - stvr);
	}
}

class mthdcls988 {

	static int stvr;

	static mthdcls988() {
		stvr = 802;
	}

	public int method988 (int var171, int var295) {
		if (var171>var295)
			return (var171+var295 + stvr);
		else
			return (var295+var171 - stvr);
	}
}

class mthdcls989 {

	static int stvr;

	static mthdcls989() {
		stvr = 59;
	}

	public int method989 (int var959, int var527) {
		if (var959>var527)
			return (var959+var527 + stvr);
		else
			return (var527+var959 - stvr);
	}
}

class mthdcls990 {

	static int stvr;

	static mthdcls990() {
		stvr = 259;
	}

	public int method990 (int var922, int var157) {
		if (var922>var157)
			return (var922-var157 + stvr);
		else
			return (var157-var922 - stvr);
	}
}

class mthdcls991 {

	static int stvr;

	static mthdcls991() {
		stvr = 248;
	}

	public int method991 (int var0, int var868) {
		if (var0>var868)
			return (var0-var868 + stvr);
		else
			return (var868-var0 - stvr);
	}
}

class mthdcls992 {

	static int stvr;

	static mthdcls992() {
		stvr = 247;
	}

	public int method992 (int var837, int var150) {
		if (var837>var150)
			return (var837*var150 + stvr);
		else
			return (var150*var837 - stvr);
	}
}

class mthdcls993 {

	static int stvr;

	static mthdcls993() {
		stvr = 248;
	}

	public int method993 (int var420, int var403) {
		if (var420>var403)
			return (var420-var403 + stvr);
		else
			return (var403-var420 - stvr);
	}
}

class mthdcls994 {

	static int stvr;

	static mthdcls994() {
		stvr = 570;
	}

	public int method994 (int var168, int var567) {
		if (var168>var567)
			return (var168-var567 + stvr);
		else
			return (var567-var168 - stvr);
	}
}

class mthdcls995 {

	static int stvr;

	static mthdcls995() {
		stvr = 717;
	}

	public int method995 (int var410, int var231) {
		if (var410>var231)
			return (var410+var231 + stvr);
		else
			return (var231+var410 - stvr);
	}
}

class mthdcls996 {

	static int stvr;

	static mthdcls996() {
		stvr = 362;
	}

	public int method996 (int var547, int var571) {
		if (var547>var571)
			return (var547*var571 + stvr);
		else
			return (var571*var547 - stvr);
	}
}

class mthdcls997 {

	static int stvr;

	static mthdcls997() {
		stvr = 344;
	}

	public int method997 (int var706, int var405) {
		if (var706>var405)
			return (var706*var405 + stvr);
		else
			return (var405*var706 - stvr);
	}
}

class mthdcls998 {

	static int stvr;

	static mthdcls998() {
		stvr = 70;
	}

	public int method998 (int var661, int var795) {
		if (var661>var795)
			return (var661-var795 + stvr);
		else
			return (var795-var661 - stvr);
	}
}

class mthdcls999 {

	static int stvr;

	static mthdcls999() {
		stvr = 165;
	}

	public int method999 (int var604, int var724) {
		if (var604>var724)
			return (var604+var724 + stvr);
		else
			return (var724+var604 - stvr);
	}
}

	}
}
