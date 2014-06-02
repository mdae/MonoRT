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

            int index = args[0].IndexOf(":");
            sHours = args[0].Substring(0, index);
            //            Console.WriteLine("hours: " + sHours);

            sMinutes = args[0].Substring((index + 1), 2);
            //sMinutes = args[0].Substring(3, 2);
            //            Console.WriteLine("minutes: " + sMinutes);

            sSeconds = args[0].Substring((index + 4), 2);
            //sSeconds = args[0].Substring(6, 2);
            //           Console.WriteLine("seconds: " + sSeconds);

            sMilliseconds = args[0].Substring((index + 7), 2);
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
					var923 = mthdcls0.method0(var931,var500);
					var750 = mthdcls1.method1(var880,var603);
					var215 = mthdcls2.method2(var888,var483);
					var479 = mthdcls3.method3(var887,var594);
					var746 = mthdcls4.method4(var450,var858);
					var762 = mthdcls5.method5(var347,var507);
					var481 = mthdcls6.method6(var906,var284);
					var836 = mthdcls7.method7(var470,var417);
					var464 = mthdcls8.method8(var817,var582);
					var559 = mthdcls9.method9(var848,var371);
					var359 = mthdcls10.method10(var722,var424);
					var174 = mthdcls11.method11(var925,var185);
					var69 = mthdcls12.method12(var182,var99);
					var984 = mthdcls13.method13(var367,var493);
					var467 = mthdcls14.method14(var683,var752);
					var321 = mthdcls15.method15(var755,var378);
					var528 = mthdcls16.method16(var760,var97);
					var713 = mthdcls17.method17(var668,var678);
					var381 = mthdcls18.method18(var676,var309);
					var362 = mthdcls19.method19(var140,var161);
					var439 = mthdcls20.method20(var600,var895);
					var149 = mthdcls21.method21(var661,var388);
					var288 = mthdcls22.method22(var821,var109);
					var135 = mthdcls23.method23(var703,var219);
					var341 = mthdcls24.method24(var748,var720);
					var562 = mthdcls25.method25(var45,var310);
					var743 = mthdcls26.method26(var498,var649);
					var217 = mthdcls27.method27(var831,var768);
					var394 = mthdcls28.method28(var931,var435);
					var620 = mthdcls29.method29(var674,var97);
					var419 = mthdcls30.method30(var349,var728);
					var105 = mthdcls31.method31(var632,var962);
					var932 = mthdcls32.method32(var810,var568);
					var922 = mthdcls33.method33(var330,var556);
					var206 = mthdcls34.method34(var964,var857);
					var41 = mthdcls35.method35(var713,var49);
					var401 = mthdcls36.method36(var759,var382);
					var598 = mthdcls37.method37(var587,var123);
					var815 = mthdcls38.method38(var909,var388);
					var123 = mthdcls39.method39(var980,var188);
					var292 = mthdcls40.method40(var684,var834);
					var790 = mthdcls41.method41(var54,var270);
					var529 = mthdcls42.method42(var675,var827);
					var76 = mthdcls43.method43(var510,var318);
					var533 = mthdcls44.method44(var818,var368);
					var509 = mthdcls45.method45(var982,var447);
					var518 = mthdcls46.method46(var883,var836);
					var539 = mthdcls47.method47(var501,var848);
					var434 = mthdcls48.method48(var181,var112);
					var637 = mthdcls49.method49(var538,var527);
					var225 = mthdcls50.method50(var564,var10);
					var77 = mthdcls51.method51(var141,var257);
					var391 = mthdcls52.method52(var504,var55);
					var440 = mthdcls53.method53(var687,var775);
					var249 = mthdcls54.method54(var652,var165);
					var417 = mthdcls55.method55(var731,var19);
					var449 = mthdcls56.method56(var262,var231);
					var432 = mthdcls57.method57(var177,var456);
					var426 = mthdcls58.method58(var864,var158);
					var541 = mthdcls59.method59(var760,var533);
					var634 = mthdcls60.method60(var183,var479);
					var84 = mthdcls61.method61(var116,var638);
					var732 = mthdcls62.method62(var849,var569);
					var106 = mthdcls63.method63(var350,var229);
					var138 = mthdcls64.method64(var145,var225);
					var599 = mthdcls65.method65(var497,var973);
					var869 = mthdcls66.method66(var871,var999);
					var602 = mthdcls67.method67(var658,var995);
					var920 = mthdcls68.method68(var315,var420);
					var625 = mthdcls69.method69(var669,var613);
					var124 = mthdcls70.method70(var85,var466);
					var577 = mthdcls71.method71(var959,var53);
					var994 = mthdcls72.method72(var287,var31);
					var157 = mthdcls73.method73(var874,var888);
					var263 = mthdcls74.method74(var163,var129);
					var491 = mthdcls75.method75(var969,var977);
					var724 = mthdcls76.method76(var483,var478);
					var772 = mthdcls77.method77(var270,var144);
					var151 = mthdcls78.method78(var938,var15);
					var339 = mthdcls79.method79(var98,var618);
					var607 = mthdcls80.method80(var835,var761);
					var166 = mthdcls81.method81(var25,var680);
					var590 = mthdcls82.method82(var936,var993);
					var896 = mthdcls83.method83(var343,var106);
					var933 = mthdcls84.method84(var527,var369);
					var619 = mthdcls85.method85(var954,var300);
					var679 = mthdcls86.method86(var195,var888);
					var707 = mthdcls87.method87(var863,var296);
					var572 = mthdcls88.method88(var192,var618);
					var73 = mthdcls89.method89(var634,var375);
					var550 = mthdcls90.method90(var951,var351);
					var650 = mthdcls91.method91(var189,var919);
					var258 = mthdcls92.method92(var819,var60);
					var391 = mthdcls93.method93(var755,var597);
					var263 = mthdcls94.method94(var569,var371);
					var952 = mthdcls95.method95(var45,var63);
					var386 = mthdcls96.method96(var41,var552);
					var692 = mthdcls97.method97(var916,var986);
					var268 = mthdcls98.method98(var549,var567);
					var563 = mthdcls99.method99(var544,var503);
					var932 = mthdcls100.method100(var319,var795);
					var911 = mthdcls101.method101(var250,var761);
					var805 = mthdcls102.method102(var576,var35);
					var941 = mthdcls103.method103(var458,var833);
					var682 = mthdcls104.method104(var801,var577);
					var626 = mthdcls105.method105(var374,var547);
					var886 = mthdcls106.method106(var741,var376);
					var843 = mthdcls107.method107(var346,var21);
					var763 = mthdcls108.method108(var794,var544);
					var103 = mthdcls109.method109(var605,var937);
					var359 = mthdcls110.method110(var750,var174);
					var290 = mthdcls111.method111(var611,var334);
					var662 = mthdcls112.method112(var826,var70);
					var701 = mthdcls113.method113(var156,var875);
					var555 = mthdcls114.method114(var1,var584);
					var147 = mthdcls115.method115(var155,var863);
					var217 = mthdcls116.method116(var284,var147);
					var168 = mthdcls117.method117(var221,var333);
					var138 = mthdcls118.method118(var751,var133);
					var673 = mthdcls119.method119(var391,var485);
					var185 = mthdcls120.method120(var375,var678);
					var199 = mthdcls121.method121(var865,var597);
					var576 = mthdcls122.method122(var259,var483);
					var437 = mthdcls123.method123(var716,var385);
					var611 = mthdcls124.method124(var423,var355);
					var938 = mthdcls125.method125(var942,var421);
					var640 = mthdcls126.method126(var695,var621);
					var956 = mthdcls127.method127(var10,var873);
					var424 = mthdcls128.method128(var324,var797);
					var718 = mthdcls129.method129(var147,var701);
					var672 = mthdcls130.method130(var724,var97);
					var282 = mthdcls131.method131(var390,var353);
					var111 = mthdcls132.method132(var438,var704);
					var435 = mthdcls133.method133(var475,var542);
					var950 = mthdcls134.method134(var354,var765);
					var147 = mthdcls135.method135(var450,var975);
					var586 = mthdcls136.method136(var759,var553);
					var433 = mthdcls137.method137(var995,var230);
					var311 = mthdcls138.method138(var917,var260);
					var507 = mthdcls139.method139(var946,var704);
					var744 = mthdcls140.method140(var267,var371);
					var862 = mthdcls141.method141(var422,var902);
					var738 = mthdcls142.method142(var38,var943);
					var714 = mthdcls143.method143(var705,var402);
					var412 = mthdcls144.method144(var180,var943);
					var882 = mthdcls145.method145(var407,var186);
					var693 = mthdcls146.method146(var437,var163);
					var613 = mthdcls147.method147(var119,var404);
					var615 = mthdcls148.method148(var727,var759);
					var736 = mthdcls149.method149(var270,var540);
					var346 = mthdcls150.method150(var372,var290);
					var113 = mthdcls151.method151(var823,var533);
					var223 = mthdcls152.method152(var823,var239);
					var333 = mthdcls153.method153(var585,var928);
					var651 = mthdcls154.method154(var644,var494);
					var686 = mthdcls155.method155(var632,var940);
					var691 = mthdcls156.method156(var570,var193);
					var591 = mthdcls157.method157(var579,var419);
					var956 = mthdcls158.method158(var120,var639);
					var74 = mthdcls159.method159(var601,var850);
					var785 = mthdcls160.method160(var519,var687);
					var433 = mthdcls161.method161(var772,var528);
					var35 = mthdcls162.method162(var188,var469);
					var678 = mthdcls163.method163(var961,var213);
					var416 = mthdcls164.method164(var170,var913);
					var749 = mthdcls165.method165(var931,var683);
					var37 = mthdcls166.method166(var720,var638);
					var151 = mthdcls167.method167(var155,var746);
					var608 = mthdcls168.method168(var305,var729);
					var953 = mthdcls169.method169(var978,var890);
					var154 = mthdcls170.method170(var23,var246);
					var830 = mthdcls171.method171(var488,var195);
					var82 = mthdcls172.method172(var919,var917);
					var450 = mthdcls173.method173(var694,var671);
					var910 = mthdcls174.method174(var382,var684);
					var819 = mthdcls175.method175(var549,var620);
					var34 = mthdcls176.method176(var446,var329);
					var131 = mthdcls177.method177(var724,var272);
					var87 = mthdcls178.method178(var994,var640);
					var480 = mthdcls179.method179(var988,var134);
					var810 = mthdcls180.method180(var256,var234);
					var336 = mthdcls181.method181(var196,var520);
					var270 = mthdcls182.method182(var283,var779);
					var847 = mthdcls183.method183(var166,var951);
					var935 = mthdcls184.method184(var251,var394);
					var8 = mthdcls185.method185(var206,var16);
					var108 = mthdcls186.method186(var660,var920);
					var358 = mthdcls187.method187(var474,var125);
					var111 = mthdcls188.method188(var401,var544);
					var702 = mthdcls189.method189(var454,var860);
					var510 = mthdcls190.method190(var78,var249);
					var716 = mthdcls191.method191(var65,var248);
					var886 = mthdcls192.method192(var979,var674);
					var630 = mthdcls193.method193(var659,var267);
					var144 = mthdcls194.method194(var833,var283);
					var494 = mthdcls195.method195(var65,var935);
					var772 = mthdcls196.method196(var701,var59);
					var449 = mthdcls197.method197(var885,var640);
					var365 = mthdcls198.method198(var415,var561);
					var576 = mthdcls199.method199(var356,var422);
					var515 = mthdcls200.method200(var86,var339);
					var979 = mthdcls201.method201(var59,var177);
					var628 = mthdcls202.method202(var842,var450);
					var5 = mthdcls203.method203(var975,var504);
					var713 = mthdcls204.method204(var834,var334);
					var489 = mthdcls205.method205(var891,var974);
					var463 = mthdcls206.method206(var587,var517);
					var680 = mthdcls207.method207(var208,var518);
					var204 = mthdcls208.method208(var440,var375);
					var60 = mthdcls209.method209(var960,var621);
					var988 = mthdcls210.method210(var224,var617);
					var395 = mthdcls211.method211(var749,var563);
					var951 = mthdcls212.method212(var974,var632);
					var676 = mthdcls213.method213(var214,var893);
					var882 = mthdcls214.method214(var899,var157);
					var998 = mthdcls215.method215(var217,var363);
					var854 = mthdcls216.method216(var226,var132);
					var579 = mthdcls217.method217(var754,var409);
					var882 = mthdcls218.method218(var359,var933);
					var215 = mthdcls219.method219(var760,var581);
					var704 = mthdcls220.method220(var618,var941);
					var209 = mthdcls221.method221(var301,var795);
					var585 = mthdcls222.method222(var148,var611);
					var380 = mthdcls223.method223(var866,var390);
					var341 = mthdcls224.method224(var258,var949);
					var533 = mthdcls225.method225(var802,var309);
					var269 = mthdcls226.method226(var14,var536);
					var4 = mthdcls227.method227(var591,var560);
					var314 = mthdcls228.method228(var8,var623);
					var836 = mthdcls229.method229(var496,var438);
					var884 = mthdcls230.method230(var874,var173);
					var221 = mthdcls231.method231(var606,var512);
					var90 = mthdcls232.method232(var918,var474);
					var755 = mthdcls233.method233(var990,var545);
					var303 = mthdcls234.method234(var932,var820);
					var465 = mthdcls235.method235(var298,var937);
					var264 = mthdcls236.method236(var737,var765);
					var645 = mthdcls237.method237(var784,var653);
					var168 = mthdcls238.method238(var30,var908);
					var47 = mthdcls239.method239(var319,var298);
					var710 = mthdcls240.method240(var604,var412);
					var125 = mthdcls241.method241(var262,var569);
					var744 = mthdcls242.method242(var886,var294);
					var850 = mthdcls243.method243(var653,var34);
					var705 = mthdcls244.method244(var142,var287);
					var559 = mthdcls245.method245(var192,var739);
					var208 = mthdcls246.method246(var870,var570);
					var864 = mthdcls247.method247(var283,var628);
					var188 = mthdcls248.method248(var934,var855);
					var447 = mthdcls249.method249(var283,var862);
					var31 = mthdcls250.method250(var622,var897);
					var224 = mthdcls251.method251(var460,var564);
					var822 = mthdcls252.method252(var37,var980);
					var454 = mthdcls253.method253(var15,var443);
					var415 = mthdcls254.method254(var477,var999);
					var814 = mthdcls255.method255(var285,var554);
					var854 = mthdcls256.method256(var671,var754);
					var429 = mthdcls257.method257(var573,var17);
					var320 = mthdcls258.method258(var249,var689);
					var737 = mthdcls259.method259(var723,var574);
					var455 = mthdcls260.method260(var92,var901);
					var469 = mthdcls261.method261(var342,var332);
					var79 = mthdcls262.method262(var183,var59);
					var854 = mthdcls263.method263(var288,var665);
					var302 = mthdcls264.method264(var647,var285);
					var723 = mthdcls265.method265(var840,var575);
					var582 = mthdcls266.method266(var887,var245);
					var546 = mthdcls267.method267(var101,var491);
					var398 = mthdcls268.method268(var815,var251);
					var431 = mthdcls269.method269(var266,var395);
					var369 = mthdcls270.method270(var107,var589);
					var850 = mthdcls271.method271(var176,var980);
					var667 = mthdcls272.method272(var802,var168);
					var177 = mthdcls273.method273(var473,var892);
					var694 = mthdcls274.method274(var85,var301);
					var910 = mthdcls275.method275(var65,var731);
					var814 = mthdcls276.method276(var952,var983);
					var437 = mthdcls277.method277(var489,var495);
					var980 = mthdcls278.method278(var483,var643);
					var662 = mthdcls279.method279(var101,var545);
					var192 = mthdcls280.method280(var159,var599);
					var190 = mthdcls281.method281(var426,var556);
					var0 = mthdcls282.method282(var298,var257);
					var828 = mthdcls283.method283(var906,var883);
					var126 = mthdcls284.method284(var105,var974);
					var513 = mthdcls285.method285(var879,var336);
					var609 = mthdcls286.method286(var8,var236);
					var283 = mthdcls287.method287(var466,var609);
					var84 = mthdcls288.method288(var3,var969);
					var236 = mthdcls289.method289(var825,var817);
					var825 = mthdcls290.method290(var878,var262);
					var975 = mthdcls291.method291(var615,var83);
					var873 = mthdcls292.method292(var634,var649);
					var817 = mthdcls293.method293(var79,var806);
					var74 = mthdcls294.method294(var596,var348);
					var189 = mthdcls295.method295(var731,var879);
					var473 = mthdcls296.method296(var379,var932);
					var931 = mthdcls297.method297(var267,var865);
					var232 = mthdcls298.method298(var339,var314);
					var62 = mthdcls299.method299(var257,var444);
					var933 = mthdcls300.method300(var640,var511);
					var277 = mthdcls301.method301(var878,var118);
					var529 = mthdcls302.method302(var590,var783);
					var894 = mthdcls303.method303(var550,var0);
					var646 = mthdcls304.method304(var922,var159);
					var553 = mthdcls305.method305(var825,var284);
					var700 = mthdcls306.method306(var9,var292);
					var801 = mthdcls307.method307(var928,var76);
					var66 = mthdcls308.method308(var757,var691);
					var837 = mthdcls309.method309(var329,var134);
					var733 = mthdcls310.method310(var10,var196);
					var714 = mthdcls311.method311(var40,var113);
					var638 = mthdcls312.method312(var304,var418);
					var455 = mthdcls313.method313(var516,var337);
					var573 = mthdcls314.method314(var753,var118);
					var40 = mthdcls315.method315(var789,var815);
					var857 = mthdcls316.method316(var773,var684);
					var835 = mthdcls317.method317(var960,var314);
					var284 = mthdcls318.method318(var854,var846);
					var369 = mthdcls319.method319(var768,var121);
					var435 = mthdcls320.method320(var539,var889);
					var887 = mthdcls321.method321(var287,var309);
					var900 = mthdcls322.method322(var917,var623);
					var493 = mthdcls323.method323(var174,var730);
					var725 = mthdcls324.method324(var341,var819);
					var670 = mthdcls325.method325(var345,var651);
					var869 = mthdcls326.method326(var879,var164);
					var628 = mthdcls327.method327(var50,var499);
					var852 = mthdcls328.method328(var200,var345);
					var296 = mthdcls329.method329(var640,var754);
					var47 = mthdcls330.method330(var342,var408);
					var289 = mthdcls331.method331(var969,var221);
					var984 = mthdcls332.method332(var967,var335);
					var139 = mthdcls333.method333(var71,var392);
					var686 = mthdcls334.method334(var689,var487);
					var991 = mthdcls335.method335(var668,var827);
					var870 = mthdcls336.method336(var280,var201);
					var884 = mthdcls337.method337(var760,var638);
					var356 = mthdcls338.method338(var852,var879);
					var205 = mthdcls339.method339(var580,var354);
					var192 = mthdcls340.method340(var474,var136);
					var58 = mthdcls341.method341(var830,var164);
					var329 = mthdcls342.method342(var65,var229);
					var755 = mthdcls343.method343(var993,var357);
					var985 = mthdcls344.method344(var555,var741);
					var763 = mthdcls345.method345(var640,var183);
					var774 = mthdcls346.method346(var860,var610);
					var12 = mthdcls347.method347(var561,var278);
					var359 = mthdcls348.method348(var421,var227);
					var912 = mthdcls349.method349(var833,var545);
					var294 = mthdcls350.method350(var646,var131);
					var997 = mthdcls351.method351(var998,var5);
					var78 = mthdcls352.method352(var18,var313);
					var568 = mthdcls353.method353(var792,var147);
					var115 = mthdcls354.method354(var714,var944);
					var917 = mthdcls355.method355(var331,var402);
					var770 = mthdcls356.method356(var583,var292);
					var996 = mthdcls357.method357(var359,var853);
					var477 = mthdcls358.method358(var722,var51);
					var72 = mthdcls359.method359(var390,var410);
					var744 = mthdcls360.method360(var896,var22);
					var643 = mthdcls361.method361(var947,var335);
					var651 = mthdcls362.method362(var644,var275);
					var837 = mthdcls363.method363(var186,var158);
					var169 = mthdcls364.method364(var408,var468);
					var926 = mthdcls365.method365(var615,var387);
					var273 = mthdcls366.method366(var417,var215);
					var149 = mthdcls367.method367(var200,var118);
					var62 = mthdcls368.method368(var300,var215);
					var493 = mthdcls369.method369(var216,var159);
					var413 = mthdcls370.method370(var883,var768);
					var433 = mthdcls371.method371(var238,var800);
					var448 = mthdcls372.method372(var633,var309);
					var241 = mthdcls373.method373(var210,var202);
					var833 = mthdcls374.method374(var722,var305);
					var453 = mthdcls375.method375(var119,var882);
					var230 = mthdcls376.method376(var391,var908);
					var753 = mthdcls377.method377(var920,var306);
					var960 = mthdcls378.method378(var834,var162);
					var373 = mthdcls379.method379(var177,var555);
					var584 = mthdcls380.method380(var493,var741);
					var746 = mthdcls381.method381(var998,var191);
					var70 = mthdcls382.method382(var824,var332);
					var462 = mthdcls383.method383(var238,var357);
					var923 = mthdcls384.method384(var934,var361);
					var864 = mthdcls385.method385(var623,var164);
					var49 = mthdcls386.method386(var816,var283);
					var463 = mthdcls387.method387(var203,var479);
					var652 = mthdcls388.method388(var481,var69);
					var656 = mthdcls389.method389(var644,var630);
					var467 = mthdcls390.method390(var974,var952);
					var56 = mthdcls391.method391(var682,var285);
					var784 = mthdcls392.method392(var346,var617);
					var713 = mthdcls393.method393(var352,var123);
					var841 = mthdcls394.method394(var259,var676);
					var341 = mthdcls395.method395(var547,var445);
					var356 = mthdcls396.method396(var358,var227);
					var181 = mthdcls397.method397(var674,var678);
					var149 = mthdcls398.method398(var15,var913);
					var909 = mthdcls399.method399(var811,var721);
					var975 = mthdcls400.method400(var24,var817);
					var861 = mthdcls401.method401(var931,var312);
					var125 = mthdcls402.method402(var711,var46);
					var462 = mthdcls403.method403(var955,var587);
					var824 = mthdcls404.method404(var937,var308);
					var773 = mthdcls405.method405(var473,var393);
					var371 = mthdcls406.method406(var593,var61);
					var490 = mthdcls407.method407(var192,var513);
					var133 = mthdcls408.method408(var965,var426);
					var85 = mthdcls409.method409(var489,var216);
					var533 = mthdcls410.method410(var290,var932);
					var901 = mthdcls411.method411(var204,var415);
					var643 = mthdcls412.method412(var320,var923);
					var320 = mthdcls413.method413(var529,var710);
					var890 = mthdcls414.method414(var852,var564);
					var846 = mthdcls415.method415(var823,var631);
					var177 = mthdcls416.method416(var756,var483);
					var54 = mthdcls417.method417(var382,var243);
					var293 = mthdcls418.method418(var988,var147);
					var152 = mthdcls419.method419(var864,var850);
					var703 = mthdcls420.method420(var209,var159);
					var345 = mthdcls421.method421(var689,var362);
					var787 = mthdcls422.method422(var669,var442);
					var434 = mthdcls423.method423(var834,var453);
					var996 = mthdcls424.method424(var944,var963);
					var51 = mthdcls425.method425(var551,var738);
					var616 = mthdcls426.method426(var714,var27);
					var183 = mthdcls427.method427(var20,var620);
					var64 = mthdcls428.method428(var894,var492);
					var388 = mthdcls429.method429(var796,var974);
					var759 = mthdcls430.method430(var538,var387);
					var330 = mthdcls431.method431(var692,var496);
					var371 = mthdcls432.method432(var433,var560);
					var680 = mthdcls433.method433(var830,var768);
					var145 = mthdcls434.method434(var264,var62);
					var301 = mthdcls435.method435(var565,var730);
					var909 = mthdcls436.method436(var903,var346);
					var503 = mthdcls437.method437(var761,var801);
					var573 = mthdcls438.method438(var530,var753);
					var871 = mthdcls439.method439(var908,var486);
					var569 = mthdcls440.method440(var762,var702);
					var719 = mthdcls441.method441(var54,var454);
					var985 = mthdcls442.method442(var588,var766);
					var292 = mthdcls443.method443(var213,var77);
					var965 = mthdcls444.method444(var857,var986);
					var820 = mthdcls445.method445(var587,var552);
					var863 = mthdcls446.method446(var797,var685);
					var110 = mthdcls447.method447(var714,var63);
					var279 = mthdcls448.method448(var473,var466);
					var272 = mthdcls449.method449(var517,var831);
					var938 = mthdcls450.method450(var488,var707);
					var940 = mthdcls451.method451(var213,var405);
					var666 = mthdcls452.method452(var956,var979);
					var797 = mthdcls453.method453(var772,var35);
					var483 = mthdcls454.method454(var228,var484);
					var781 = mthdcls455.method455(var936,var324);
					var650 = mthdcls456.method456(var277,var987);
					var272 = mthdcls457.method457(var863,var395);
					var190 = mthdcls458.method458(var30,var686);
					var790 = mthdcls459.method459(var780,var236);
					var314 = mthdcls460.method460(var456,var245);
					var932 = mthdcls461.method461(var126,var664);
					var822 = mthdcls462.method462(var188,var619);
					var245 = mthdcls463.method463(var967,var712);
					var298 = mthdcls464.method464(var676,var150);
					var423 = mthdcls465.method465(var625,var64);
					var642 = mthdcls466.method466(var522,var896);
					var839 = mthdcls467.method467(var908,var914);
					var405 = mthdcls468.method468(var295,var492);
					var691 = mthdcls469.method469(var356,var164);
					var979 = mthdcls470.method470(var310,var304);
					var439 = mthdcls471.method471(var770,var600);
					var388 = mthdcls472.method472(var164,var782);
					var940 = mthdcls473.method473(var328,var369);
					var50 = mthdcls474.method474(var949,var506);
					var435 = mthdcls475.method475(var307,var237);
					var209 = mthdcls476.method476(var309,var513);
					var528 = mthdcls477.method477(var942,var672);
					var287 = mthdcls478.method478(var986,var478);
					var685 = mthdcls479.method479(var735,var13);
					var471 = mthdcls480.method480(var947,var292);
					var473 = mthdcls481.method481(var607,var843);
					var86 = mthdcls482.method482(var183,var450);
					var828 = mthdcls483.method483(var222,var705);
					var22 = mthdcls484.method484(var317,var532);
					var84 = mthdcls485.method485(var864,var794);
					var692 = mthdcls486.method486(var835,var698);
					var854 = mthdcls487.method487(var762,var390);
					var863 = mthdcls488.method488(var322,var518);
					var479 = mthdcls489.method489(var15,var739);
					var286 = mthdcls490.method490(var196,var701);
					var857 = mthdcls491.method491(var807,var695);
					var294 = mthdcls492.method492(var643,var733);
					var881 = mthdcls493.method493(var251,var18);
					var83 = mthdcls494.method494(var969,var353);
					var127 = mthdcls495.method495(var827,var220);
					var897 = mthdcls496.method496(var253,var756);
					var364 = mthdcls497.method497(var897,var793);
					var22 = mthdcls498.method498(var889,var273);
					var983 = mthdcls499.method499(var543,var961);
					var751 = mthdcls500.method500(var729,var715);
					var634 = mthdcls501.method501(var562,var869);
					var425 = mthdcls502.method502(var264,var221);
					var650 = mthdcls503.method503(var842,var927);
					var173 = mthdcls504.method504(var812,var247);
					var823 = mthdcls505.method505(var152,var346);
					var891 = mthdcls506.method506(var4,var900);
					var617 = mthdcls507.method507(var455,var511);
					var543 = mthdcls508.method508(var88,var274);
					var176 = mthdcls509.method509(var378,var438);
					var80 = mthdcls510.method510(var943,var60);
					var74 = mthdcls511.method511(var640,var368);
					var997 = mthdcls512.method512(var269,var567);
					var926 = mthdcls513.method513(var506,var798);
					var185 = mthdcls514.method514(var626,var83);
					var385 = mthdcls515.method515(var491,var59);
					var184 = mthdcls516.method516(var277,var110);
					var768 = mthdcls517.method517(var286,var202);
					var814 = mthdcls518.method518(var977,var821);
					var226 = mthdcls519.method519(var839,var185);
					var818 = mthdcls520.method520(var274,var438);
					var69 = mthdcls521.method521(var20,var526);
					var904 = mthdcls522.method522(var997,var845);
					var610 = mthdcls523.method523(var151,var123);
					var129 = mthdcls524.method524(var82,var153);
					var414 = mthdcls525.method525(var529,var853);
					var450 = mthdcls526.method526(var292,var22);
					var436 = mthdcls527.method527(var778,var639);
					var722 = mthdcls528.method528(var86,var831);
					var881 = mthdcls529.method529(var907,var976);
					var148 = mthdcls530.method530(var28,var521);
					var871 = mthdcls531.method531(var673,var280);
					var526 = mthdcls532.method532(var528,var340);
					var402 = mthdcls533.method533(var406,var85);
					var552 = mthdcls534.method534(var352,var107);
					var138 = mthdcls535.method535(var618,var467);
					var849 = mthdcls536.method536(var817,var980);
					var280 = mthdcls537.method537(var450,var664);
					var556 = mthdcls538.method538(var624,var371);
					var126 = mthdcls539.method539(var447,var186);
					var740 = mthdcls540.method540(var670,var878);
					var639 = mthdcls541.method541(var21,var273);
					var237 = mthdcls542.method542(var14,var903);
					var627 = mthdcls543.method543(var525,var574);
					var472 = mthdcls544.method544(var896,var685);
					var546 = mthdcls545.method545(var833,var356);
					var788 = mthdcls546.method546(var669,var361);
					var766 = mthdcls547.method547(var64,var803);
					var114 = mthdcls548.method548(var93,var282);
					var990 = mthdcls549.method549(var941,var90);
					var345 = mthdcls550.method550(var84,var868);
					var903 = mthdcls551.method551(var830,var230);
					var836 = mthdcls552.method552(var701,var813);
					var680 = mthdcls553.method553(var121,var570);
					var555 = mthdcls554.method554(var785,var969);
					var30 = mthdcls555.method555(var331,var966);
					var668 = mthdcls556.method556(var819,var459);
					var622 = mthdcls557.method557(var743,var330);
					var59 = mthdcls558.method558(var983,var884);
					var153 = mthdcls559.method559(var234,var582);
					var113 = mthdcls560.method560(var576,var182);
					var33 = mthdcls561.method561(var471,var529);
					var424 = mthdcls562.method562(var463,var351);
					var319 = mthdcls563.method563(var346,var957);
					var24 = mthdcls564.method564(var885,var698);
					var677 = mthdcls565.method565(var995,var820);
					var587 = mthdcls566.method566(var236,var793);
					var88 = mthdcls567.method567(var98,var715);
					var360 = mthdcls568.method568(var506,var477);
					var11 = mthdcls569.method569(var619,var991);
					var794 = mthdcls570.method570(var574,var345);
					var66 = mthdcls571.method571(var334,var248);
					var395 = mthdcls572.method572(var647,var778);
					var146 = mthdcls573.method573(var484,var827);
					var215 = mthdcls574.method574(var676,var461);
					var460 = mthdcls575.method575(var910,var173);
					var668 = mthdcls576.method576(var777,var599);
					var280 = mthdcls577.method577(var623,var28);
					var489 = mthdcls578.method578(var51,var554);
					var849 = mthdcls579.method579(var336,var465);
					var21 = mthdcls580.method580(var117,var280);
					var638 = mthdcls581.method581(var805,var216);
					var837 = mthdcls582.method582(var699,var485);
					var338 = mthdcls583.method583(var368,var364);
					var84 = mthdcls584.method584(var294,var735);
					var484 = mthdcls585.method585(var912,var300);
					var625 = mthdcls586.method586(var661,var130);
					var845 = mthdcls587.method587(var21,var506);
					var838 = mthdcls588.method588(var761,var462);
					var571 = mthdcls589.method589(var805,var668);
					var692 = mthdcls590.method590(var305,var294);
					var138 = mthdcls591.method591(var115,var957);
					var425 = mthdcls592.method592(var892,var135);
					var490 = mthdcls593.method593(var444,var357);
					var278 = mthdcls594.method594(var519,var677);
					var233 = mthdcls595.method595(var411,var659);
					var7 = mthdcls596.method596(var179,var687);
					var230 = mthdcls597.method597(var248,var579);
					var868 = mthdcls598.method598(var842,var665);
					var422 = mthdcls599.method599(var856,var173);
					var382 = mthdcls600.method600(var611,var329);
					var787 = mthdcls601.method601(var95,var227);
					var753 = mthdcls602.method602(var283,var790);
					var575 = mthdcls603.method603(var420,var552);
					var437 = mthdcls604.method604(var451,var815);
					var693 = mthdcls605.method605(var101,var784);
					var510 = mthdcls606.method606(var524,var514);
					var656 = mthdcls607.method607(var262,var905);
					var765 = mthdcls608.method608(var393,var368);
					var836 = mthdcls609.method609(var239,var153);
					var742 = mthdcls610.method610(var235,var42);
					var554 = mthdcls611.method611(var478,var341);
					var332 = mthdcls612.method612(var141,var126);
					var200 = mthdcls613.method613(var911,var586);
					var845 = mthdcls614.method614(var935,var317);
					var258 = mthdcls615.method615(var988,var979);
					var541 = mthdcls616.method616(var555,var670);
					var865 = mthdcls617.method617(var74,var225);
					var119 = mthdcls618.method618(var673,var370);
					var900 = mthdcls619.method619(var873,var346);
					var678 = mthdcls620.method620(var579,var486);
					var3 = mthdcls621.method621(var917,var165);
					var852 = mthdcls622.method622(var813,var849);
					var373 = mthdcls623.method623(var79,var215);
					var116 = mthdcls624.method624(var368,var362);
					var577 = mthdcls625.method625(var467,var588);
					var462 = mthdcls626.method626(var547,var130);
					var907 = mthdcls627.method627(var669,var71);
					var82 = mthdcls628.method628(var503,var418);
					var614 = mthdcls629.method629(var900,var97);
					var438 = mthdcls630.method630(var301,var478);
					var496 = mthdcls631.method631(var757,var914);
					var211 = mthdcls632.method632(var822,var944);
					var966 = mthdcls633.method633(var677,var201);
					var496 = mthdcls634.method634(var983,var970);
					var302 = mthdcls635.method635(var265,var354);
					var375 = mthdcls636.method636(var547,var515);
					var582 = mthdcls637.method637(var457,var325);
					var157 = mthdcls638.method638(var540,var533);
					var501 = mthdcls639.method639(var910,var857);
					var51 = mthdcls640.method640(var147,var840);
					var366 = mthdcls641.method641(var805,var37);
					var127 = mthdcls642.method642(var870,var748);
					var317 = mthdcls643.method643(var640,var975);
					var144 = mthdcls644.method644(var938,var853);
					var256 = mthdcls645.method645(var3,var201);
					var771 = mthdcls646.method646(var797,var249);
					var311 = mthdcls647.method647(var395,var247);
					var855 = mthdcls648.method648(var100,var991);
					var947 = mthdcls649.method649(var714,var575);
					var403 = mthdcls650.method650(var577,var132);
					var200 = mthdcls651.method651(var321,var855);
					var769 = mthdcls652.method652(var735,var238);
					var599 = mthdcls653.method653(var461,var929);
					var291 = mthdcls654.method654(var740,var177);
					var858 = mthdcls655.method655(var323,var955);
					var467 = mthdcls656.method656(var170,var495);
					var439 = mthdcls657.method657(var653,var525);
					var169 = mthdcls658.method658(var117,var770);
					var404 = mthdcls659.method659(var739,var105);
					var506 = mthdcls660.method660(var508,var620);
					var537 = mthdcls661.method661(var924,var10);
					var632 = mthdcls662.method662(var820,var178);
					var275 = mthdcls663.method663(var922,var469);
					var408 = mthdcls664.method664(var14,var802);
					var917 = mthdcls665.method665(var116,var17);
					var229 = mthdcls666.method666(var729,var82);
					var923 = mthdcls667.method667(var4,var74);
					var108 = mthdcls668.method668(var356,var513);
					var47 = mthdcls669.method669(var33,var664);
					var762 = mthdcls670.method670(var481,var941);
					var736 = mthdcls671.method671(var409,var3);
					var888 = mthdcls672.method672(var40,var470);
					var815 = mthdcls673.method673(var100,var611);
					var400 = mthdcls674.method674(var264,var105);
					var877 = mthdcls675.method675(var977,var58);
					var57 = mthdcls676.method676(var697,var261);
					var185 = mthdcls677.method677(var60,var377);
					var126 = mthdcls678.method678(var762,var177);
					var300 = mthdcls679.method679(var916,var471);
					var329 = mthdcls680.method680(var818,var874);
					var127 = mthdcls681.method681(var97,var510);
					var299 = mthdcls682.method682(var815,var392);
					var847 = mthdcls683.method683(var604,var707);
					var354 = mthdcls684.method684(var179,var613);
					var109 = mthdcls685.method685(var86,var613);
					var711 = mthdcls686.method686(var651,var549);
					var973 = mthdcls687.method687(var514,var426);
					var964 = mthdcls688.method688(var289,var490);
					var130 = mthdcls689.method689(var454,var838);
					var342 = mthdcls690.method690(var82,var870);
					var950 = mthdcls691.method691(var291,var708);
					var50 = mthdcls692.method692(var525,var943);
					var943 = mthdcls693.method693(var957,var866);
					var853 = mthdcls694.method694(var584,var550);
					var966 = mthdcls695.method695(var56,var723);
					var473 = mthdcls696.method696(var310,var692);
					var653 = mthdcls697.method697(var416,var731);
					var128 = mthdcls698.method698(var88,var299);
					var143 = mthdcls699.method699(var655,var729);
					var798 = mthdcls700.method700(var964,var225);
					var547 = mthdcls701.method701(var370,var115);
					var816 = mthdcls702.method702(var180,var622);
					var801 = mthdcls703.method703(var421,var885);
					var953 = mthdcls704.method704(var782,var643);
					var148 = mthdcls705.method705(var52,var802);
					var727 = mthdcls706.method706(var978,var299);
					var410 = mthdcls707.method707(var496,var374);
					var768 = mthdcls708.method708(var369,var190);
					var255 = mthdcls709.method709(var301,var795);
					var356 = mthdcls710.method710(var909,var260);
					var268 = mthdcls711.method711(var678,var21);
					var361 = mthdcls712.method712(var320,var284);
					var245 = mthdcls713.method713(var232,var845);
					var196 = mthdcls714.method714(var855,var969);
					var114 = mthdcls715.method715(var814,var487);
					var823 = mthdcls716.method716(var712,var965);
					var153 = mthdcls717.method717(var207,var497);
					var421 = mthdcls718.method718(var323,var82);
					var807 = mthdcls719.method719(var569,var983);
					var781 = mthdcls720.method720(var444,var813);
					var381 = mthdcls721.method721(var560,var83);
					var546 = mthdcls722.method722(var478,var775);
					var147 = mthdcls723.method723(var588,var456);
					var488 = mthdcls724.method724(var937,var203);
					var871 = mthdcls725.method725(var452,var315);
					var538 = mthdcls726.method726(var839,var546);
					var851 = mthdcls727.method727(var285,var143);
					var309 = mthdcls728.method728(var490,var30);
					var666 = mthdcls729.method729(var898,var405);
					var223 = mthdcls730.method730(var28,var311);
					var335 = mthdcls731.method731(var45,var339);
					var785 = mthdcls732.method732(var242,var241);
					var717 = mthdcls733.method733(var698,var355);
					var135 = mthdcls734.method734(var322,var573);
					var893 = mthdcls735.method735(var185,var500);
					var255 = mthdcls736.method736(var747,var428);
					var253 = mthdcls737.method737(var411,var362);
					var151 = mthdcls738.method738(var961,var239);
					var734 = mthdcls739.method739(var616,var505);
					var704 = mthdcls740.method740(var224,var68);
					var391 = mthdcls741.method741(var958,var790);
					var234 = mthdcls742.method742(var283,var945);
					var645 = mthdcls743.method743(var993,var758);
					var876 = mthdcls744.method744(var350,var433);
					var311 = mthdcls745.method745(var722,var53);
					var537 = mthdcls746.method746(var16,var97);
					var306 = mthdcls747.method747(var397,var700);
					var87 = mthdcls748.method748(var289,var60);
					var540 = mthdcls749.method749(var506,var666);
					var870 = mthdcls750.method750(var78,var41);
					var100 = mthdcls751.method751(var640,var875);
					var423 = mthdcls752.method752(var222,var608);
					var309 = mthdcls753.method753(var108,var347);
					var136 = mthdcls754.method754(var779,var784);
					var418 = mthdcls755.method755(var284,var217);
					var412 = mthdcls756.method756(var866,var149);
					var893 = mthdcls757.method757(var118,var418);
					var926 = mthdcls758.method758(var210,var21);
					var412 = mthdcls759.method759(var945,var390);
					var880 = mthdcls760.method760(var318,var89);
					var978 = mthdcls761.method761(var415,var32);
					var876 = mthdcls762.method762(var193,var552);
					var613 = mthdcls763.method763(var548,var543);
					var152 = mthdcls764.method764(var831,var602);
					var227 = mthdcls765.method765(var929,var23);
					var342 = mthdcls766.method766(var289,var772);
					var129 = mthdcls767.method767(var931,var421);
					var902 = mthdcls768.method768(var590,var137);
					var670 = mthdcls769.method769(var669,var730);
					var714 = mthdcls770.method770(var318,var419);
					var890 = mthdcls771.method771(var488,var438);
					var868 = mthdcls772.method772(var731,var677);
					var815 = mthdcls773.method773(var458,var662);
					var415 = mthdcls774.method774(var498,var480);
					var744 = mthdcls775.method775(var363,var84);
					var479 = mthdcls776.method776(var233,var99);
					var657 = mthdcls777.method777(var54,var969);
					var963 = mthdcls778.method778(var870,var972);
					var114 = mthdcls779.method779(var565,var677);
					var874 = mthdcls780.method780(var273,var211);
					var186 = mthdcls781.method781(var57,var236);
					var110 = mthdcls782.method782(var903,var800);
					var11 = mthdcls783.method783(var675,var34);
					var355 = mthdcls784.method784(var548,var269);
					var374 = mthdcls785.method785(var873,var909);
					var857 = mthdcls786.method786(var404,var509);
					var272 = mthdcls787.method787(var604,var322);
					var388 = mthdcls788.method788(var577,var277);
					var352 = mthdcls789.method789(var408,var837);
					var878 = mthdcls790.method790(var550,var100);
					var679 = mthdcls791.method791(var96,var655);
					var13 = mthdcls792.method792(var568,var997);
					var293 = mthdcls793.method793(var72,var461);
					var885 = mthdcls794.method794(var634,var773);
					var705 = mthdcls795.method795(var828,var193);
					var24 = mthdcls796.method796(var250,var199);
					var996 = mthdcls797.method797(var938,var494);
					var535 = mthdcls798.method798(var701,var386);
					var580 = mthdcls799.method799(var836,var624);
					var519 = mthdcls800.method800(var874,var991);
					var899 = mthdcls801.method801(var494,var191);
					var552 = mthdcls802.method802(var27,var244);
					var412 = mthdcls803.method803(var898,var835);
					var14 = mthdcls804.method804(var399,var887);
					var516 = mthdcls805.method805(var818,var254);
					var48 = mthdcls806.method806(var123,var358);
					var173 = mthdcls807.method807(var967,var297);
					var82 = mthdcls808.method808(var746,var497);
					var415 = mthdcls809.method809(var294,var945);
					var235 = mthdcls810.method810(var800,var850);
					var422 = mthdcls811.method811(var675,var222);
					var652 = mthdcls812.method812(var263,var666);
					var663 = mthdcls813.method813(var657,var71);
					var792 = mthdcls814.method814(var244,var32);
					var78 = mthdcls815.method815(var896,var660);
					var791 = mthdcls816.method816(var444,var763);
					var476 = mthdcls817.method817(var160,var179);
					var746 = mthdcls818.method818(var623,var987);
					var155 = mthdcls819.method819(var597,var573);
					var330 = mthdcls820.method820(var113,var730);
					var888 = mthdcls821.method821(var400,var617);
					var954 = mthdcls822.method822(var52,var326);
					var818 = mthdcls823.method823(var785,var443);
					var53 = mthdcls824.method824(var227,var355);
					var520 = mthdcls825.method825(var625,var316);
					var932 = mthdcls826.method826(var553,var86);
					var768 = mthdcls827.method827(var670,var623);
					var290 = mthdcls828.method828(var980,var481);
					var78 = mthdcls829.method829(var875,var487);
					var390 = mthdcls830.method830(var536,var714);
					var640 = mthdcls831.method831(var553,var807);
					var691 = mthdcls832.method832(var434,var612);
					var829 = mthdcls833.method833(var903,var184);
					var823 = mthdcls834.method834(var750,var882);
					var321 = mthdcls835.method835(var742,var664);
					var661 = mthdcls836.method836(var790,var631);
					var145 = mthdcls837.method837(var889,var303);
					var535 = mthdcls838.method838(var920,var110);
					var796 = mthdcls839.method839(var413,var660);
					var730 = mthdcls840.method840(var336,var417);
					var348 = mthdcls841.method841(var881,var976);
					var318 = mthdcls842.method842(var691,var100);
					var729 = mthdcls843.method843(var597,var376);
					var0 = mthdcls844.method844(var793,var642);
					var757 = mthdcls845.method845(var394,var241);
					var704 = mthdcls846.method846(var275,var438);
					var554 = mthdcls847.method847(var302,var714);
					var432 = mthdcls848.method848(var190,var56);
					var12 = mthdcls849.method849(var66,var897);
					var789 = mthdcls850.method850(var837,var421);
					var131 = mthdcls851.method851(var909,var86);
					var834 = mthdcls852.method852(var225,var578);
					var357 = mthdcls853.method853(var527,var923);
					var226 = mthdcls854.method854(var554,var40);
					var909 = mthdcls855.method855(var118,var895);
					var262 = mthdcls856.method856(var592,var560);
					var467 = mthdcls857.method857(var329,var914);
					var808 = mthdcls858.method858(var749,var179);
					var884 = mthdcls859.method859(var921,var780);
					var211 = mthdcls860.method860(var664,var513);
					var804 = mthdcls861.method861(var687,var536);
					var794 = mthdcls862.method862(var715,var452);
					var430 = mthdcls863.method863(var93,var875);
					var613 = mthdcls864.method864(var419,var952);
					var25 = mthdcls865.method865(var165,var435);
					var13 = mthdcls866.method866(var913,var344);
					var722 = mthdcls867.method867(var235,var367);
					var759 = mthdcls868.method868(var325,var416);
					var688 = mthdcls869.method869(var802,var26);
					var979 = mthdcls870.method870(var140,var513);
					var303 = mthdcls871.method871(var748,var142);
					var735 = mthdcls872.method872(var265,var190);
					var198 = mthdcls873.method873(var545,var4);
					var904 = mthdcls874.method874(var188,var17);
					var999 = mthdcls875.method875(var734,var261);
					var735 = mthdcls876.method876(var311,var86);
					var789 = mthdcls877.method877(var126,var118);
					var683 = mthdcls878.method878(var687,var183);
					var966 = mthdcls879.method879(var890,var459);
					var9 = mthdcls880.method880(var156,var906);
					var236 = mthdcls881.method881(var632,var676);
					var589 = mthdcls882.method882(var105,var222);
					var12 = mthdcls883.method883(var900,var884);
					var456 = mthdcls884.method884(var826,var909);
					var781 = mthdcls885.method885(var251,var885);
					var255 = mthdcls886.method886(var33,var53);
					var309 = mthdcls887.method887(var371,var596);
					var598 = mthdcls888.method888(var912,var273);
					var722 = mthdcls889.method889(var361,var906);
					var855 = mthdcls890.method890(var260,var427);
					var344 = mthdcls891.method891(var866,var383);
					var431 = mthdcls892.method892(var149,var11);
					var580 = mthdcls893.method893(var88,var143);
					var558 = mthdcls894.method894(var994,var952);
					var910 = mthdcls895.method895(var315,var61);
					var250 = mthdcls896.method896(var962,var923);
					var555 = mthdcls897.method897(var18,var327);
					var395 = mthdcls898.method898(var759,var225);
					var671 = mthdcls899.method899(var797,var851);
					var475 = mthdcls900.method900(var59,var19);
					var460 = mthdcls901.method901(var280,var874);
					var661 = mthdcls902.method902(var311,var724);
					var805 = mthdcls903.method903(var888,var697);
					var864 = mthdcls904.method904(var668,var326);
					var195 = mthdcls905.method905(var585,var836);
					var674 = mthdcls906.method906(var952,var975);
					var627 = mthdcls907.method907(var862,var198);
					var332 = mthdcls908.method908(var641,var864);
					var510 = mthdcls909.method909(var172,var867);
					var97 = mthdcls910.method910(var255,var134);
					var823 = mthdcls911.method911(var741,var932);
					var85 = mthdcls912.method912(var273,var812);
					var169 = mthdcls913.method913(var989,var983);
					var726 = mthdcls914.method914(var378,var856);
					var770 = mthdcls915.method915(var701,var166);
					var214 = mthdcls916.method916(var469,var127);
					var64 = mthdcls917.method917(var955,var154);
					var582 = mthdcls918.method918(var52,var745);
					var415 = mthdcls919.method919(var847,var265);
					var226 = mthdcls920.method920(var597,var3);
					var91 = mthdcls921.method921(var496,var358);
					var427 = mthdcls922.method922(var395,var520);
					var107 = mthdcls923.method923(var911,var8);
					var672 = mthdcls924.method924(var81,var252);
					var326 = mthdcls925.method925(var84,var880);
					var46 = mthdcls926.method926(var214,var807);
					var897 = mthdcls927.method927(var486,var591);
					var950 = mthdcls928.method928(var461,var291);
					var593 = mthdcls929.method929(var254,var489);
					var796 = mthdcls930.method930(var46,var554);
					var629 = mthdcls931.method931(var69,var837);
					var6 = mthdcls932.method932(var530,var577);
					var949 = mthdcls933.method933(var779,var132);
					var646 = mthdcls934.method934(var542,var523);
					var903 = mthdcls935.method935(var103,var344);
					var598 = mthdcls936.method936(var473,var147);
					var281 = mthdcls937.method937(var938,var541);
					var75 = mthdcls938.method938(var722,var861);
					var134 = mthdcls939.method939(var101,var229);
					var568 = mthdcls940.method940(var264,var830);
					var583 = mthdcls941.method941(var487,var548);
					var863 = mthdcls942.method942(var817,var920);
					var972 = mthdcls943.method943(var550,var293);
					var971 = mthdcls944.method944(var832,var763);
					var935 = mthdcls945.method945(var736,var991);
					var961 = mthdcls946.method946(var312,var231);
					var195 = mthdcls947.method947(var644,var785);
					var679 = mthdcls948.method948(var706,var102);
					var130 = mthdcls949.method949(var793,var298);
					var502 = mthdcls950.method950(var315,var718);
					var3 = mthdcls951.method951(var804,var460);
					var760 = mthdcls952.method952(var549,var33);
					var905 = mthdcls953.method953(var585,var927);
					var585 = mthdcls954.method954(var124,var770);
					var356 = mthdcls955.method955(var754,var60);
					var315 = mthdcls956.method956(var605,var393);
					var546 = mthdcls957.method957(var488,var884);
					var71 = mthdcls958.method958(var214,var331);
					var830 = mthdcls959.method959(var405,var365);
					var726 = mthdcls960.method960(var106,var305);
					var287 = mthdcls961.method961(var30,var227);
					var390 = mthdcls962.method962(var497,var398);
					var246 = mthdcls963.method963(var809,var368);
					var244 = mthdcls964.method964(var503,var584);
					var974 = mthdcls965.method965(var54,var416);
					var822 = mthdcls966.method966(var926,var753);
					var298 = mthdcls967.method967(var897,var53);
					var734 = mthdcls968.method968(var272,var393);
					var508 = mthdcls969.method969(var250,var241);
					var360 = mthdcls970.method970(var889,var471);
					var514 = mthdcls971.method971(var830,var772);
					var391 = mthdcls972.method972(var405,var123);
					var107 = mthdcls973.method973(var467,var626);
					var372 = mthdcls974.method974(var32,var9);
					var522 = mthdcls975.method975(var976,var827);
					var137 = mthdcls976.method976(var508,var603);
					var294 = mthdcls977.method977(var538,var539);
					var112 = mthdcls978.method978(var179,var4);
					var947 = mthdcls979.method979(var948,var453);
					var553 = mthdcls980.method980(var720,var492);
					var375 = mthdcls981.method981(var77,var688);
					var135 = mthdcls982.method982(var884,var678);
					var956 = mthdcls983.method983(var702,var463);
					var777 = mthdcls984.method984(var291,var415);
					var882 = mthdcls985.method985(var824,var537);
					var851 = mthdcls986.method986(var402,var836);
					var92 = mthdcls987.method987(var549,var521);
					var896 = mthdcls988.method988(var125,var326);
					var20 = mthdcls989.method989(var124,var922);
					var731 = mthdcls990.method990(var311,var412);
					var655 = mthdcls991.method991(var715,var716);
					var327 = mthdcls992.method992(var601,var808);
					var856 = mthdcls993.method993(var903,var91);
					var823 = mthdcls994.method994(var366,var965);
					var56 = mthdcls995.method995(var564,var424);
					var152 = mthdcls996.method996(var367,var268);
					var47 = mthdcls997.method997(var748,var390);
					var964 = mthdcls998.method998(var813,var61);
					var967 = mthdcls999.method999(var633,var688);
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
					var579 = mthdcls0.method0(var469,var849);
					var558 = mthdcls1.method1(var492,var957);
					var601 = mthdcls2.method2(var791,var972);
					var959 = mthdcls3.method3(var572,var489);
					var77 = mthdcls4.method4(var174,var640);
					var287 = mthdcls5.method5(var347,var598);
					var363 = mthdcls6.method6(var904,var747);
					var82 = mthdcls7.method7(var644,var3);
					var22 = mthdcls8.method8(var338,var282);
					var297 = mthdcls9.method9(var411,var134);
					var926 = mthdcls10.method10(var31,var394);
					var6 = mthdcls11.method11(var961,var567);
					var486 = mthdcls12.method12(var249,var465);
					var865 = mthdcls13.method13(var20,var670);
					var167 = mthdcls14.method14(var142,var0);
					var307 = mthdcls15.method15(var800,var960);
					var791 = mthdcls16.method16(var722,var468);
					var670 = mthdcls17.method17(var222,var554);
					var396 = mthdcls18.method18(var547,var75);
					var843 = mthdcls19.method19(var596,var924);
					var470 = mthdcls20.method20(var351,var326);
					var106 = mthdcls21.method21(var939,var901);
					var322 = mthdcls22.method22(var934,var173);
					var332 = mthdcls23.method23(var487,var386);
					var806 = mthdcls24.method24(var641,var436);
					var77 = mthdcls25.method25(var860,var89);
					var607 = mthdcls26.method26(var474,var263);
					var439 = mthdcls27.method27(var700,var486);
					var663 = mthdcls28.method28(var575,var705);
					var288 = mthdcls29.method29(var66,var60);
					var245 = mthdcls30.method30(var552,var75);
					var132 = mthdcls31.method31(var378,var633);
					var864 = mthdcls32.method32(var525,var706);
					var923 = mthdcls33.method33(var446,var710);
					var353 = mthdcls34.method34(var316,var458);
					var29 = mthdcls35.method35(var969,var735);
					var890 = mthdcls36.method36(var821,var841);
					var787 = mthdcls37.method37(var776,var536);
					var679 = mthdcls38.method38(var918,var275);
					var194 = mthdcls39.method39(var728,var305);
					var36 = mthdcls40.method40(var796,var228);
					var250 = mthdcls41.method41(var886,var776);
					var33 = mthdcls42.method42(var489,var182);
					var406 = mthdcls43.method43(var107,var125);
					var199 = mthdcls44.method44(var785,var633);
					var476 = mthdcls45.method45(var662,var164);
					var806 = mthdcls46.method46(var745,var299);
					var511 = mthdcls47.method47(var559,var761);
					var24 = mthdcls48.method48(var448,var324);
					var825 = mthdcls49.method49(var246,var601);
					var600 = mthdcls50.method50(var375,var342);
					var299 = mthdcls51.method51(var815,var321);
					var510 = mthdcls52.method52(var567,var683);
					var982 = mthdcls53.method53(var366,var805);
					var928 = mthdcls54.method54(var144,var522);
					var330 = mthdcls55.method55(var227,var15);
					var512 = mthdcls56.method56(var231,var593);
					var450 = mthdcls57.method57(var947,var126);
					var705 = mthdcls58.method58(var661,var453);
					var928 = mthdcls59.method59(var434,var564);
					var266 = mthdcls60.method60(var465,var805);
					var200 = mthdcls61.method61(var40,var302);
					var196 = mthdcls62.method62(var54,var263);
					var302 = mthdcls63.method63(var249,var647);
					var652 = mthdcls64.method64(var575,var151);
					var848 = mthdcls65.method65(var563,var433);
					var56 = mthdcls66.method66(var362,var994);
					var395 = mthdcls67.method67(var390,var681);
					var335 = mthdcls68.method68(var135,var569);
					var142 = mthdcls69.method69(var259,var513);
					var124 = mthdcls70.method70(var456,var304);
					var381 = mthdcls71.method71(var733,var718);
					var152 = mthdcls72.method72(var352,var993);
					var673 = mthdcls73.method73(var766,var793);
					var958 = mthdcls74.method74(var149,var236);
					var198 = mthdcls75.method75(var60,var266);
					var791 = mthdcls76.method76(var569,var91);
					var310 = mthdcls77.method77(var669,var921);
					var440 = mthdcls78.method78(var809,var160);
					var424 = mthdcls79.method79(var466,var321);
					var150 = mthdcls80.method80(var843,var61);
					var589 = mthdcls81.method81(var535,var455);
					var688 = mthdcls82.method82(var503,var339);
					var953 = mthdcls83.method83(var788,var297);
					var642 = mthdcls84.method84(var486,var271);
					var683 = mthdcls85.method85(var726,var468);
					var240 = mthdcls86.method86(var525,var974);
					var144 = mthdcls87.method87(var676,var937);
					var362 = mthdcls88.method88(var280,var394);
					var714 = mthdcls89.method89(var845,var277);
					var29 = mthdcls90.method90(var649,var13);
					var39 = mthdcls91.method91(var884,var469);
					var151 = mthdcls92.method92(var472,var878);
					var552 = mthdcls93.method93(var471,var591);
					var25 = mthdcls94.method94(var265,var595);
					var946 = mthdcls95.method95(var634,var731);
					var558 = mthdcls96.method96(var160,var414);
					var445 = mthdcls97.method97(var579,var189);
					var291 = mthdcls98.method98(var500,var830);
					var22 = mthdcls99.method99(var705,var66);
					var304 = mthdcls100.method100(var216,var625);
					var787 = mthdcls101.method101(var482,var197);
					var272 = mthdcls102.method102(var376,var891);
					var324 = mthdcls103.method103(var49,var995);
					var909 = mthdcls104.method104(var80,var110);
					var528 = mthdcls105.method105(var565,var486);
					var646 = mthdcls106.method106(var862,var449);
					var372 = mthdcls107.method107(var9,var779);
					var973 = mthdcls108.method108(var813,var24);
					var226 = mthdcls109.method109(var556,var687);
					var196 = mthdcls110.method110(var775,var580);
					var553 = mthdcls111.method111(var503,var476);
					var681 = mthdcls112.method112(var944,var155);
					var66 = mthdcls113.method113(var381,var148);
					var85 = mthdcls114.method114(var696,var710);
					var42 = mthdcls115.method115(var436,var799);
					var216 = mthdcls116.method116(var478,var476);
					var604 = mthdcls117.method117(var465,var18);
					var869 = mthdcls118.method118(var529,var635);
					var71 = mthdcls119.method119(var283,var5);
					var515 = mthdcls120.method120(var327,var221);
					var825 = mthdcls121.method121(var943,var900);
					var909 = mthdcls122.method122(var212,var369);
					var68 = mthdcls123.method123(var92,var765);
					var269 = mthdcls124.method124(var167,var385);
					var845 = mthdcls125.method125(var907,var991);
					var909 = mthdcls126.method126(var444,var177);
					var952 = mthdcls127.method127(var942,var551);
					var171 = mthdcls128.method128(var869,var553);
					var755 = mthdcls129.method129(var610,var602);
					var566 = mthdcls130.method130(var468,var575);
					var86 = mthdcls131.method131(var973,var615);
					var878 = mthdcls132.method132(var917,var311);
					var865 = mthdcls133.method133(var134,var445);
					var890 = mthdcls134.method134(var772,var300);
					var523 = mthdcls135.method135(var661,var914);
					var846 = mthdcls136.method136(var999,var975);
					var880 = mthdcls137.method137(var460,var681);
					var439 = mthdcls138.method138(var46,var752);
					var134 = mthdcls139.method139(var851,var327);
					var22 = mthdcls140.method140(var992,var901);
					var504 = mthdcls141.method141(var933,var647);
					var875 = mthdcls142.method142(var497,var866);
					var861 = mthdcls143.method143(var183,var993);
					var144 = mthdcls144.method144(var910,var468);
					var297 = mthdcls145.method145(var492,var260);
					var112 = mthdcls146.method146(var125,var117);
					var419 = mthdcls147.method147(var904,var282);
					var579 = mthdcls148.method148(var574,var567);
					var70 = mthdcls149.method149(var153,var326);
					var739 = mthdcls150.method150(var380,var50);
					var450 = mthdcls151.method151(var681,var141);
					var300 = mthdcls152.method152(var980,var303);
					var3 = mthdcls153.method153(var31,var401);
					var801 = mthdcls154.method154(var721,var882);
					var555 = mthdcls155.method155(var975,var177);
					var101 = mthdcls156.method156(var864,var479);
					var681 = mthdcls157.method157(var981,var524);
					var588 = mthdcls158.method158(var641,var941);
					var451 = mthdcls159.method159(var822,var792);
					var347 = mthdcls160.method160(var895,var194);
					var863 = mthdcls161.method161(var829,var782);
					var191 = mthdcls162.method162(var423,var27);
					var913 = mthdcls163.method163(var321,var314);
					var158 = mthdcls164.method164(var247,var645);
					var436 = mthdcls165.method165(var437,var380);
					var694 = mthdcls166.method166(var937,var633);
					var116 = mthdcls167.method167(var248,var360);
					var979 = mthdcls168.method168(var843,var186);
					var187 = mthdcls169.method169(var620,var899);
					var949 = mthdcls170.method170(var876,var952);
					var390 = mthdcls171.method171(var682,var716);
					var242 = mthdcls172.method172(var554,var75);
					var446 = mthdcls173.method173(var118,var595);
					var482 = mthdcls174.method174(var163,var230);
					var363 = mthdcls175.method175(var433,var620);
					var544 = mthdcls176.method176(var744,var455);
					var753 = mthdcls177.method177(var831,var923);
					var842 = mthdcls178.method178(var470,var943);
					var803 = mthdcls179.method179(var180,var113);
					var540 = mthdcls180.method180(var637,var348);
					var581 = mthdcls181.method181(var794,var725);
					var831 = mthdcls182.method182(var995,var17);
					var282 = mthdcls183.method183(var2,var816);
					var835 = mthdcls184.method184(var949,var482);
					var879 = mthdcls185.method185(var284,var324);
					var518 = mthdcls186.method186(var509,var900);
					var383 = mthdcls187.method187(var7,var506);
					var358 = mthdcls188.method188(var312,var527);
					var370 = mthdcls189.method189(var595,var957);
					var884 = mthdcls190.method190(var247,var537);
					var792 = mthdcls191.method191(var443,var301);
					var760 = mthdcls192.method192(var532,var681);
					var351 = mthdcls193.method193(var78,var108);
					var102 = mthdcls194.method194(var35,var844);
					var71 = mthdcls195.method195(var746,var324);
					var564 = mthdcls196.method196(var529,var942);
					var572 = mthdcls197.method197(var207,var222);
					var228 = mthdcls198.method198(var293,var100);
					var555 = mthdcls199.method199(var138,var493);
					var964 = mthdcls200.method200(var299,var313);
					var665 = mthdcls201.method201(var204,var894);
					var714 = mthdcls202.method202(var800,var105);
					var410 = mthdcls203.method203(var133,var960);
					var760 = mthdcls204.method204(var988,var566);
					var328 = mthdcls205.method205(var175,var784);
					var278 = mthdcls206.method206(var65,var212);
					var972 = mthdcls207.method207(var232,var102);
					var992 = mthdcls208.method208(var585,var933);
					var871 = mthdcls209.method209(var587,var548);
					var587 = mthdcls210.method210(var960,var427);
					var270 = mthdcls211.method211(var218,var117);
					var347 = mthdcls212.method212(var113,var468);
					var515 = mthdcls213.method213(var896,var961);
					var45 = mthdcls214.method214(var499,var317);
					var970 = mthdcls215.method215(var339,var105);
					var229 = mthdcls216.method216(var643,var360);
					var228 = mthdcls217.method217(var967,var589);
					var905 = mthdcls218.method218(var4,var872);
					var43 = mthdcls219.method219(var447,var87);
					var547 = mthdcls220.method220(var600,var331);
					var589 = mthdcls221.method221(var514,var171);
					var914 = mthdcls222.method222(var261,var670);
					var596 = mthdcls223.method223(var988,var69);
					var555 = mthdcls224.method224(var635,var705);
					var983 = mthdcls225.method225(var4,var643);
					var196 = mthdcls226.method226(var988,var713);
					var889 = mthdcls227.method227(var424,var500);
					var0 = mthdcls228.method228(var986,var629);
					var837 = mthdcls229.method229(var756,var47);
					var202 = mthdcls230.method230(var85,var443);
					var872 = mthdcls231.method231(var527,var826);
					var406 = mthdcls232.method232(var410,var794);
					var334 = mthdcls233.method233(var966,var696);
					var908 = mthdcls234.method234(var241,var929);
					var470 = mthdcls235.method235(var804,var466);
					var588 = mthdcls236.method236(var919,var375);
					var35 = mthdcls237.method237(var287,var400);
					var884 = mthdcls238.method238(var461,var157);
					var458 = mthdcls239.method239(var62,var687);
					var764 = mthdcls240.method240(var504,var467);
					var336 = mthdcls241.method241(var629,var292);
					var161 = mthdcls242.method242(var314,var705);
					var234 = mthdcls243.method243(var178,var537);
					var54 = mthdcls244.method244(var277,var613);
					var678 = mthdcls245.method245(var601,var23);
					var616 = mthdcls246.method246(var539,var829);
					var170 = mthdcls247.method247(var774,var68);
					var282 = mthdcls248.method248(var698,var618);
					var106 = mthdcls249.method249(var242,var234);
					var665 = mthdcls250.method250(var92,var704);
					var559 = mthdcls251.method251(var156,var429);
					var641 = mthdcls252.method252(var631,var627);
					var250 = mthdcls253.method253(var869,var780);
					var850 = mthdcls254.method254(var49,var89);
					var204 = mthdcls255.method255(var260,var219);
					var117 = mthdcls256.method256(var186,var842);
					var50 = mthdcls257.method257(var215,var827);
					var22 = mthdcls258.method258(var672,var799);
					var907 = mthdcls259.method259(var179,var200);
					var650 = mthdcls260.method260(var529,var686);
					var454 = mthdcls261.method261(var365,var397);
					var686 = mthdcls262.method262(var5,var187);
					var408 = mthdcls263.method263(var417,var382);
					var906 = mthdcls264.method264(var429,var696);
					var778 = mthdcls265.method265(var954,var946);
					var46 = mthdcls266.method266(var609,var898);
					var710 = mthdcls267.method267(var927,var42);
					var584 = mthdcls268.method268(var135,var406);
					var249 = mthdcls269.method269(var194,var758);
					var742 = mthdcls270.method270(var636,var443);
					var219 = mthdcls271.method271(var833,var486);
					var874 = mthdcls272.method272(var420,var352);
					var310 = mthdcls273.method273(var250,var314);
					var172 = mthdcls274.method274(var507,var288);
					var132 = mthdcls275.method275(var123,var173);
					var243 = mthdcls276.method276(var886,var265);
					var550 = mthdcls277.method277(var712,var420);
					var458 = mthdcls278.method278(var14,var86);
					var466 = mthdcls279.method279(var621,var878);
					var522 = mthdcls280.method280(var265,var652);
					var876 = mthdcls281.method281(var158,var103);
					var209 = mthdcls282.method282(var398,var141);
					var564 = mthdcls283.method283(var655,var781);
					var702 = mthdcls284.method284(var159,var344);
					var347 = mthdcls285.method285(var997,var506);
					var584 = mthdcls286.method286(var569,var49);
					var940 = mthdcls287.method287(var627,var315);
					var236 = mthdcls288.method288(var476,var983);
					var567 = mthdcls289.method289(var61,var729);
					var276 = mthdcls290.method290(var476,var279);
					var788 = mthdcls291.method291(var655,var468);
					var611 = mthdcls292.method292(var13,var163);
					var940 = mthdcls293.method293(var134,var617);
					var589 = mthdcls294.method294(var674,var837);
					var325 = mthdcls295.method295(var922,var397);
					var184 = mthdcls296.method296(var981,var30);
					var519 = mthdcls297.method297(var404,var892);
					var602 = mthdcls298.method298(var45,var986);
					var864 = mthdcls299.method299(var221,var689);
					var492 = mthdcls300.method300(var196,var234);
					var200 = mthdcls301.method301(var430,var38);
					var192 = mthdcls302.method302(var28,var322);
					var18 = mthdcls303.method303(var425,var600);
					var321 = mthdcls304.method304(var603,var538);
					var530 = mthdcls305.method305(var535,var735);
					var713 = mthdcls306.method306(var190,var490);
					var119 = mthdcls307.method307(var345,var371);
					var236 = mthdcls308.method308(var79,var242);
					var78 = mthdcls309.method309(var358,var617);
					var275 = mthdcls310.method310(var582,var691);
					var145 = mthdcls311.method311(var515,var533);
					var295 = mthdcls312.method312(var985,var135);
					var307 = mthdcls313.method313(var790,var187);
					var683 = mthdcls314.method314(var993,var490);
					var911 = mthdcls315.method315(var173,var32);
					var655 = mthdcls316.method316(var522,var803);
					var907 = mthdcls317.method317(var506,var603);
					var413 = mthdcls318.method318(var910,var505);
					var88 = mthdcls319.method319(var685,var896);
					var742 = mthdcls320.method320(var206,var893);
					var14 = mthdcls321.method321(var228,var238);
					var917 = mthdcls322.method322(var327,var112);
					var627 = mthdcls323.method323(var357,var502);
					var79 = mthdcls324.method324(var191,var387);
					var582 = mthdcls325.method325(var613,var742);
					var958 = mthdcls326.method326(var326,var574);
					var153 = mthdcls327.method327(var392,var461);
					var875 = mthdcls328.method328(var69,var688);
					var676 = mthdcls329.method329(var917,var276);
					var616 = mthdcls330.method330(var968,var873);
					var507 = mthdcls331.method331(var949,var287);
					var107 = mthdcls332.method332(var491,var606);
					var907 = mthdcls333.method333(var298,var430);
					var74 = mthdcls334.method334(var329,var947);
					var649 = mthdcls335.method335(var514,var44);
					var728 = mthdcls336.method336(var344,var221);
					var829 = mthdcls337.method337(var171,var409);
					var280 = mthdcls338.method338(var773,var333);
					var386 = mthdcls339.method339(var64,var940);
					var130 = mthdcls340.method340(var425,var721);
					var205 = mthdcls341.method341(var329,var926);
					var427 = mthdcls342.method342(var749,var243);
					var738 = mthdcls343.method343(var67,var569);
					var13 = mthdcls344.method344(var613,var104);
					var745 = mthdcls345.method345(var982,var983);
					var181 = mthdcls346.method346(var101,var735);
					var302 = mthdcls347.method347(var611,var976);
					var145 = mthdcls348.method348(var190,var247);
					var668 = mthdcls349.method349(var178,var23);
					var860 = mthdcls350.method350(var358,var248);
					var868 = mthdcls351.method351(var839,var729);
					var417 = mthdcls352.method352(var461,var225);
					var202 = mthdcls353.method353(var667,var531);
					var862 = mthdcls354.method354(var627,var608);
					var919 = mthdcls355.method355(var217,var194);
					var263 = mthdcls356.method356(var90,var526);
					var665 = mthdcls357.method357(var207,var41);
					var79 = mthdcls358.method358(var772,var177);
					var852 = mthdcls359.method359(var365,var600);
					var508 = mthdcls360.method360(var966,var524);
					var41 = mthdcls361.method361(var71,var536);
					var706 = mthdcls362.method362(var385,var5);
					var184 = mthdcls363.method363(var527,var787);
					var719 = mthdcls364.method364(var91,var574);
					var70 = mthdcls365.method365(var94,var570);
					var896 = mthdcls366.method366(var373,var12);
					var394 = mthdcls367.method367(var302,var578);
					var514 = mthdcls368.method368(var894,var833);
					var206 = mthdcls369.method369(var797,var303);
					var22 = mthdcls370.method370(var31,var456);
					var40 = mthdcls371.method371(var674,var879);
					var811 = mthdcls372.method372(var771,var52);
					var537 = mthdcls373.method373(var825,var647);
					var297 = mthdcls374.method374(var890,var77);
					var132 = mthdcls375.method375(var362,var629);
					var526 = mthdcls376.method376(var185,var938);
					var970 = mthdcls377.method377(var55,var62);
					var577 = mthdcls378.method378(var477,var510);
					var483 = mthdcls379.method379(var367,var191);
					var724 = mthdcls380.method380(var934,var333);
					var467 = mthdcls381.method381(var359,var880);
					var489 = mthdcls382.method382(var829,var14);
					var442 = mthdcls383.method383(var708,var464);
					var43 = mthdcls384.method384(var710,var434);
					var42 = mthdcls385.method385(var338,var240);
					var0 = mthdcls386.method386(var37,var383);
					var350 = mthdcls387.method387(var839,var605);
					var578 = mthdcls388.method388(var88,var698);
					var988 = mthdcls389.method389(var680,var794);
					var389 = mthdcls390.method390(var981,var757);
					var609 = mthdcls391.method391(var829,var360);
					var604 = mthdcls392.method392(var586,var455);
					var34 = mthdcls393.method393(var793,var122);
					var629 = mthdcls394.method394(var489,var802);
					var588 = mthdcls395.method395(var131,var450);
					var484 = mthdcls396.method396(var488,var778);
					var521 = mthdcls397.method397(var803,var572);
					var801 = mthdcls398.method398(var742,var176);
					var723 = mthdcls399.method399(var637,var999);
					var276 = mthdcls400.method400(var903,var374);
					var979 = mthdcls401.method401(var649,var586);
					var835 = mthdcls402.method402(var554,var908);
					var845 = mthdcls403.method403(var910,var888);
					var756 = mthdcls404.method404(var512,var258);
					var862 = mthdcls405.method405(var547,var266);
					var803 = mthdcls406.method406(var835,var911);
					var975 = mthdcls407.method407(var351,var681);
					var518 = mthdcls408.method408(var486,var607);
					var778 = mthdcls409.method409(var960,var243);
					var525 = mthdcls410.method410(var49,var677);
					var609 = mthdcls411.method411(var123,var904);
					var365 = mthdcls412.method412(var117,var230);
					var939 = mthdcls413.method413(var41,var864);
					var646 = mthdcls414.method414(var648,var576);
					var803 = mthdcls415.method415(var170,var121);
					var54 = mthdcls416.method416(var315,var134);
					var398 = mthdcls417.method417(var762,var394);
					var474 = mthdcls418.method418(var226,var225);
					var764 = mthdcls419.method419(var855,var744);
					var220 = mthdcls420.method420(var718,var323);
					var969 = mthdcls421.method421(var804,var46);
					var242 = mthdcls422.method422(var107,var935);
					var454 = mthdcls423.method423(var692,var425);
					var211 = mthdcls424.method424(var488,var700);
					var513 = mthdcls425.method425(var212,var956);
					var207 = mthdcls426.method426(var291,var261);
					var843 = mthdcls427.method427(var923,var215);
					var23 = mthdcls428.method428(var806,var725);
					var708 = mthdcls429.method429(var805,var77);
					var662 = mthdcls430.method430(var258,var181);
					var775 = mthdcls431.method431(var246,var616);
					var652 = mthdcls432.method432(var157,var947);
					var63 = mthdcls433.method433(var591,var213);
					var914 = mthdcls434.method434(var763,var53);
					var291 = mthdcls435.method435(var475,var547);
					var371 = mthdcls436.method436(var667,var500);
					var516 = mthdcls437.method437(var959,var777);
					var82 = mthdcls438.method438(var962,var536);
					var547 = mthdcls439.method439(var723,var188);
					var393 = mthdcls440.method440(var84,var160);
					var871 = mthdcls441.method441(var862,var479);
					var510 = mthdcls442.method442(var448,var435);
					var409 = mthdcls443.method443(var38,var664);
					var585 = mthdcls444.method444(var539,var790);
					var744 = mthdcls445.method445(var884,var145);
					var132 = mthdcls446.method446(var61,var270);
					var177 = mthdcls447.method447(var985,var617);
					var683 = mthdcls448.method448(var577,var97);
					var310 = mthdcls449.method449(var913,var767);
					var105 = mthdcls450.method450(var204,var722);
					var538 = mthdcls451.method451(var25,var927);
					var628 = mthdcls452.method452(var375,var972);
					var308 = mthdcls453.method453(var406,var330);
					var414 = mthdcls454.method454(var310,var397);
					var323 = mthdcls455.method455(var531,var341);
					var94 = mthdcls456.method456(var505,var864);
					var226 = mthdcls457.method457(var634,var955);
					var82 = mthdcls458.method458(var189,var362);
					var621 = mthdcls459.method459(var846,var935);
					var851 = mthdcls460.method460(var135,var475);
					var126 = mthdcls461.method461(var2,var707);
					var249 = mthdcls462.method462(var274,var141);
					var467 = mthdcls463.method463(var213,var542);
					var51 = mthdcls464.method464(var627,var196);
					var44 = mthdcls465.method465(var543,var29);
					var535 = mthdcls466.method466(var494,var214);
					var476 = mthdcls467.method467(var464,var977);
					var916 = mthdcls468.method468(var970,var728);
					var595 = mthdcls469.method469(var536,var317);
					var677 = mthdcls470.method470(var353,var233);
					var505 = mthdcls471.method471(var95,var864);
					var279 = mthdcls472.method472(var787,var113);
					var353 = mthdcls473.method473(var780,var502);
					var806 = mthdcls474.method474(var600,var290);
					var387 = mthdcls475.method475(var761,var657);
					var39 = mthdcls476.method476(var111,var460);
					var767 = mthdcls477.method477(var85,var528);
					var257 = mthdcls478.method478(var497,var901);
					var970 = mthdcls479.method479(var31,var138);
					var428 = mthdcls480.method480(var462,var161);
					var788 = mthdcls481.method481(var687,var711);
					var736 = mthdcls482.method482(var450,var336);
					var809 = mthdcls483.method483(var282,var885);
					var989 = mthdcls484.method484(var424,var33);
					var447 = mthdcls485.method485(var391,var935);
					var719 = mthdcls486.method486(var418,var69);
					var758 = mthdcls487.method487(var563,var398);
					var888 = mthdcls488.method488(var215,var191);
					var444 = mthdcls489.method489(var818,var384);
					var127 = mthdcls490.method490(var828,var450);
					var304 = mthdcls491.method491(var71,var894);
					var512 = mthdcls492.method492(var382,var566);
					var843 = mthdcls493.method493(var996,var826);
					var937 = mthdcls494.method494(var621,var41);
					var702 = mthdcls495.method495(var203,var687);
					var639 = mthdcls496.method496(var42,var305);
					var456 = mthdcls497.method497(var151,var647);
					var10 = mthdcls498.method498(var600,var11);
					var856 = mthdcls499.method499(var717,var793);
					var198 = mthdcls500.method500(var353,var884);
					var493 = mthdcls501.method501(var812,var455);
					var948 = mthdcls502.method502(var368,var382);
					var331 = mthdcls503.method503(var243,var703);
					var295 = mthdcls504.method504(var677,var112);
					var612 = mthdcls505.method505(var606,var916);
					var388 = mthdcls506.method506(var287,var204);
					var334 = mthdcls507.method507(var727,var24);
					var185 = mthdcls508.method508(var774,var944);
					var957 = mthdcls509.method509(var492,var615);
					var945 = mthdcls510.method510(var143,var0);
					var235 = mthdcls511.method511(var599,var292);
					var530 = mthdcls512.method512(var259,var509);
					var429 = mthdcls513.method513(var95,var287);
					var299 = mthdcls514.method514(var352,var837);
					var971 = mthdcls515.method515(var729,var127);
					var462 = mthdcls516.method516(var235,var656);
					var54 = mthdcls517.method517(var364,var102);
					var847 = mthdcls518.method518(var55,var353);
					var648 = mthdcls519.method519(var893,var519);
					var925 = mthdcls520.method520(var689,var859);
					var953 = mthdcls521.method521(var235,var956);
					var404 = mthdcls522.method522(var943,var839);
					var141 = mthdcls523.method523(var882,var479);
					var454 = mthdcls524.method524(var152,var631);
					var149 = mthdcls525.method525(var969,var624);
					var176 = mthdcls526.method526(var130,var420);
					var295 = mthdcls527.method527(var63,var972);
					var690 = mthdcls528.method528(var256,var284);
					var47 = mthdcls529.method529(var999,var643);
					var888 = mthdcls530.method530(var587,var419);
					var367 = mthdcls531.method531(var546,var615);
					var832 = mthdcls532.method532(var146,var720);
					var688 = mthdcls533.method533(var1,var104);
					var950 = mthdcls534.method534(var331,var815);
					var360 = mthdcls535.method535(var990,var392);
					var412 = mthdcls536.method536(var591,var770);
					var306 = mthdcls537.method537(var648,var250);
					var631 = mthdcls538.method538(var337,var269);
					var491 = mthdcls539.method539(var406,var620);
					var124 = mthdcls540.method540(var257,var222);
					var151 = mthdcls541.method541(var140,var778);
					var529 = mthdcls542.method542(var122,var336);
					var270 = mthdcls543.method543(var159,var577);
					var212 = mthdcls544.method544(var585,var359);
					var114 = mthdcls545.method545(var646,var813);
					var341 = mthdcls546.method546(var352,var986);
					var792 = mthdcls547.method547(var640,var379);
					var519 = mthdcls548.method548(var630,var364);
					var267 = mthdcls549.method549(var227,var768);
					var86 = mthdcls550.method550(var709,var810);
					var450 = mthdcls551.method551(var528,var423);
					var892 = mthdcls552.method552(var365,var972);
					var701 = mthdcls553.method553(var714,var177);
					var50 = mthdcls554.method554(var59,var605);
					var977 = mthdcls555.method555(var666,var269);
					var731 = mthdcls556.method556(var0,var973);
					var1 = mthdcls557.method557(var264,var638);
					var533 = mthdcls558.method558(var414,var447);
					var772 = mthdcls559.method559(var622,var716);
					var885 = mthdcls560.method560(var164,var150);
					var635 = mthdcls561.method561(var556,var981);
					var526 = mthdcls562.method562(var152,var979);
					var381 = mthdcls563.method563(var447,var377);
					var81 = mthdcls564.method564(var340,var378);
					var984 = mthdcls565.method565(var528,var1);
					var845 = mthdcls566.method566(var104,var182);
					var591 = mthdcls567.method567(var645,var510);
					var882 = mthdcls568.method568(var922,var559);
					var174 = mthdcls569.method569(var893,var547);
					var896 = mthdcls570.method570(var739,var385);
					var590 = mthdcls571.method571(var253,var336);
					var95 = mthdcls572.method572(var710,var681);
					var620 = mthdcls573.method573(var449,var664);
					var423 = mthdcls574.method574(var626,var818);
					var382 = mthdcls575.method575(var356,var753);
					var755 = mthdcls576.method576(var611,var854);
					var272 = mthdcls577.method577(var879,var75);
					var820 = mthdcls578.method578(var146,var778);
					var559 = mthdcls579.method579(var381,var219);
					var886 = mthdcls580.method580(var816,var471);
					var358 = mthdcls581.method581(var932,var783);
					var953 = mthdcls582.method582(var455,var522);
					var996 = mthdcls583.method583(var628,var775);
					var246 = mthdcls584.method584(var233,var250);
					var910 = mthdcls585.method585(var712,var569);
					var690 = mthdcls586.method586(var735,var143);
					var999 = mthdcls587.method587(var792,var673);
					var660 = mthdcls588.method588(var79,var267);
					var26 = mthdcls589.method589(var657,var469);
					var383 = mthdcls590.method590(var639,var187);
					var684 = mthdcls591.method591(var992,var673);
					var418 = mthdcls592.method592(var190,var376);
					var907 = mthdcls593.method593(var669,var787);
					var63 = mthdcls594.method594(var19,var468);
					var855 = mthdcls595.method595(var479,var205);
					var414 = mthdcls596.method596(var740,var878);
					var751 = mthdcls597.method597(var901,var912);
					var836 = mthdcls598.method598(var246,var628);
					var787 = mthdcls599.method599(var366,var259);
					var365 = mthdcls600.method600(var763,var79);
					var614 = mthdcls601.method601(var327,var840);
					var712 = mthdcls602.method602(var226,var765);
					var394 = mthdcls603.method603(var430,var507);
					var154 = mthdcls604.method604(var949,var857);
					var392 = mthdcls605.method605(var97,var880);
					var837 = mthdcls606.method606(var414,var450);
					var480 = mthdcls607.method607(var660,var398);
					var104 = mthdcls608.method608(var619,var560);
					var573 = mthdcls609.method609(var357,var152);
					var961 = mthdcls610.method610(var192,var424);
					var981 = mthdcls611.method611(var477,var162);
					var633 = mthdcls612.method612(var113,var162);
					var76 = mthdcls613.method613(var757,var598);
					var367 = mthdcls614.method614(var0,var289);
					var398 = mthdcls615.method615(var91,var503);
					var807 = mthdcls616.method616(var216,var686);
					var55 = mthdcls617.method617(var429,var213);
					var297 = mthdcls618.method618(var173,var338);
					var97 = mthdcls619.method619(var137,var165);
					var207 = mthdcls620.method620(var598,var63);
					var689 = mthdcls621.method621(var637,var831);
					var139 = mthdcls622.method622(var153,var659);
					var459 = mthdcls623.method623(var300,var594);
					var72 = mthdcls624.method624(var620,var728);
					var395 = mthdcls625.method625(var50,var446);
					var101 = mthdcls626.method626(var930,var281);
					var462 = mthdcls627.method627(var435,var192);
					var945 = mthdcls628.method628(var362,var128);
					var734 = mthdcls629.method629(var344,var645);
					var22 = mthdcls630.method630(var479,var453);
					var703 = mthdcls631.method631(var775,var162);
					var525 = mthdcls632.method632(var747,var272);
					var894 = mthdcls633.method633(var347,var644);
					var401 = mthdcls634.method634(var877,var935);
					var223 = mthdcls635.method635(var620,var237);
					var268 = mthdcls636.method636(var934,var44);
					var603 = mthdcls637.method637(var753,var491);
					var142 = mthdcls638.method638(var727,var144);
					var359 = mthdcls639.method639(var914,var474);
					var305 = mthdcls640.method640(var392,var881);
					var765 = mthdcls641.method641(var112,var656);
					var192 = mthdcls642.method642(var195,var684);
					var504 = mthdcls643.method643(var774,var813);
					var177 = mthdcls644.method644(var166,var885);
					var677 = mthdcls645.method645(var708,var944);
					var49 = mthdcls646.method646(var217,var218);
					var768 = mthdcls647.method647(var820,var869);
					var340 = mthdcls648.method648(var630,var598);
					var688 = mthdcls649.method649(var591,var118);
					var970 = mthdcls650.method650(var330,var62);
					var767 = mthdcls651.method651(var119,var534);
					var466 = mthdcls652.method652(var235,var991);
					var258 = mthdcls653.method653(var514,var675);
					var188 = mthdcls654.method654(var51,var716);
					var276 = mthdcls655.method655(var783,var883);
					var151 = mthdcls656.method656(var512,var129);
					var456 = mthdcls657.method657(var768,var796);
					var504 = mthdcls658.method658(var975,var329);
					var113 = mthdcls659.method659(var645,var578);
					var190 = mthdcls660.method660(var957,var203);
					var426 = mthdcls661.method661(var989,var98);
					var625 = mthdcls662.method662(var126,var450);
					var609 = mthdcls663.method663(var894,var824);
					var792 = mthdcls664.method664(var537,var88);
					var761 = mthdcls665.method665(var0,var24);
					var365 = mthdcls666.method666(var364,var301);
					var484 = mthdcls667.method667(var43,var13);
					var927 = mthdcls668.method668(var12,var126);
					var636 = mthdcls669.method669(var777,var20);
					var908 = mthdcls670.method670(var339,var785);
					var381 = mthdcls671.method671(var364,var690);
					var883 = mthdcls672.method672(var650,var962);
					var954 = mthdcls673.method673(var275,var758);
					var518 = mthdcls674.method674(var786,var210);
					var644 = mthdcls675.method675(var413,var755);
					var868 = mthdcls676.method676(var491,var848);
					var692 = mthdcls677.method677(var335,var624);
					var669 = mthdcls678.method678(var851,var172);
					var822 = mthdcls679.method679(var61,var299);
					var215 = mthdcls680.method680(var974,var164);
					var495 = mthdcls681.method681(var333,var135);
					var306 = mthdcls682.method682(var5,var327);
					var444 = mthdcls683.method683(var348,var244);
					var156 = mthdcls684.method684(var873,var515);
					var609 = mthdcls685.method685(var148,var418);
					var343 = mthdcls686.method686(var76,var840);
					var304 = mthdcls687.method687(var574,var478);
					var805 = mthdcls688.method688(var934,var174);
					var289 = mthdcls689.method689(var48,var229);
					var383 = mthdcls690.method690(var877,var323);
					var518 = mthdcls691.method691(var606,var31);
					var602 = mthdcls692.method692(var644,var271);
					var601 = mthdcls693.method693(var495,var994);
					var411 = mthdcls694.method694(var791,var651);
					var544 = mthdcls695.method695(var117,var857);
					var819 = mthdcls696.method696(var734,var676);
					var882 = mthdcls697.method697(var773,var832);
					var915 = mthdcls698.method698(var338,var651);
					var646 = mthdcls699.method699(var889,var302);
					var532 = mthdcls700.method700(var661,var734);
					var725 = mthdcls701.method701(var948,var353);
					var832 = mthdcls702.method702(var364,var222);
					var971 = mthdcls703.method703(var491,var291);
					var598 = mthdcls704.method704(var609,var400);
					var957 = mthdcls705.method705(var530,var742);
					var562 = mthdcls706.method706(var467,var283);
					var528 = mthdcls707.method707(var400,var745);
					var696 = mthdcls708.method708(var721,var142);
					var597 = mthdcls709.method709(var569,var252);
					var198 = mthdcls710.method710(var238,var421);
					var299 = mthdcls711.method711(var109,var204);
					var395 = mthdcls712.method712(var802,var390);
					var693 = mthdcls713.method713(var14,var375);
					var294 = mthdcls714.method714(var352,var451);
					var147 = mthdcls715.method715(var482,var28);
					var135 = mthdcls716.method716(var193,var195);
					var53 = mthdcls717.method717(var76,var636);
					var103 = mthdcls718.method718(var294,var240);
					var434 = mthdcls719.method719(var615,var744);
					var957 = mthdcls720.method720(var30,var974);
					var529 = mthdcls721.method721(var957,var116);
					var996 = mthdcls722.method722(var246,var157);
					var253 = mthdcls723.method723(var475,var502);
					var606 = mthdcls724.method724(var368,var272);
					var229 = mthdcls725.method725(var451,var763);
					var642 = mthdcls726.method726(var402,var481);
					var708 = mthdcls727.method727(var982,var825);
					var294 = mthdcls728.method728(var168,var263);
					var892 = mthdcls729.method729(var342,var993);
					var207 = mthdcls730.method730(var149,var645);
					var137 = mthdcls731.method731(var217,var511);
					var768 = mthdcls732.method732(var925,var80);
					var221 = mthdcls733.method733(var695,var718);
					var385 = mthdcls734.method734(var733,var712);
					var486 = mthdcls735.method735(var70,var251);
					var342 = mthdcls736.method736(var934,var30);
					var348 = mthdcls737.method737(var92,var621);
					var537 = mthdcls738.method738(var808,var384);
					var836 = mthdcls739.method739(var311,var445);
					var347 = mthdcls740.method740(var71,var166);
					var935 = mthdcls741.method741(var557,var756);
					var116 = mthdcls742.method742(var873,var656);
					var785 = mthdcls743.method743(var159,var200);
					var420 = mthdcls744.method744(var707,var372);
					var133 = mthdcls745.method745(var616,var360);
					var287 = mthdcls746.method746(var485,var784);
					var426 = mthdcls747.method747(var580,var897);
					var646 = mthdcls748.method748(var136,var982);
					var709 = mthdcls749.method749(var580,var460);
					var394 = mthdcls750.method750(var895,var269);
					var295 = mthdcls751.method751(var62,var495);
					var297 = mthdcls752.method752(var677,var360);
					var579 = mthdcls753.method753(var870,var710);
					var963 = mthdcls754.method754(var856,var150);
					var603 = mthdcls755.method755(var768,var194);
					var975 = mthdcls756.method756(var400,var825);
					var674 = mthdcls757.method757(var256,var851);
					var50 = mthdcls758.method758(var451,var801);
					var871 = mthdcls759.method759(var872,var61);
					var458 = mthdcls760.method760(var439,var512);
					var77 = mthdcls761.method761(var914,var449);
					var236 = mthdcls762.method762(var564,var557);
					var768 = mthdcls763.method763(var364,var421);
					var384 = mthdcls764.method764(var886,var660);
					var109 = mthdcls765.method765(var170,var728);
					var846 = mthdcls766.method766(var194,var334);
					var111 = mthdcls767.method767(var836,var518);
					var1 = mthdcls768.method768(var955,var383);
					var192 = mthdcls769.method769(var380,var613);
					var259 = mthdcls770.method770(var733,var120);
					var592 = mthdcls771.method771(var214,var449);
					var325 = mthdcls772.method772(var76,var196);
					var41 = mthdcls773.method773(var433,var39);
					var348 = mthdcls774.method774(var781,var66);
					var714 = mthdcls775.method775(var837,var738);
					var849 = mthdcls776.method776(var95,var68);
					var609 = mthdcls777.method777(var490,var259);
					var802 = mthdcls778.method778(var725,var318);
					var920 = mthdcls779.method779(var862,var465);
					var123 = mthdcls780.method780(var159,var368);
					var515 = mthdcls781.method781(var335,var325);
					var72 = mthdcls782.method782(var603,var820);
					var945 = mthdcls783.method783(var271,var431);
					var879 = mthdcls784.method784(var750,var125);
					var724 = mthdcls785.method785(var620,var577);
					var716 = mthdcls786.method786(var276,var636);
					var462 = mthdcls787.method787(var329,var914);
					var489 = mthdcls788.method788(var99,var365);
					var604 = mthdcls789.method789(var256,var889);
					var376 = mthdcls790.method790(var721,var256);
					var250 = mthdcls791.method791(var770,var1);
					var160 = mthdcls792.method792(var598,var655);
					var341 = mthdcls793.method793(var93,var260);
					var22 = mthdcls794.method794(var573,var458);
					var606 = mthdcls795.method795(var279,var575);
					var769 = mthdcls796.method796(var702,var360);
					var713 = mthdcls797.method797(var663,var973);
					var88 = mthdcls798.method798(var402,var903);
					var117 = mthdcls799.method799(var745,var334);
					var165 = mthdcls800.method800(var474,var947);
					var478 = mthdcls801.method801(var852,var11);
					var409 = mthdcls802.method802(var306,var292);
					var519 = mthdcls803.method803(var445,var44);
					var807 = mthdcls804.method804(var13,var915);
					var923 = mthdcls805.method805(var798,var356);
					var826 = mthdcls806.method806(var87,var196);
					var247 = mthdcls807.method807(var859,var922);
					var723 = mthdcls808.method808(var902,var773);
					var778 = mthdcls809.method809(var398,var758);
					var591 = mthdcls810.method810(var853,var305);
					var136 = mthdcls811.method811(var896,var49);
					var452 = mthdcls812.method812(var8,var657);
					var535 = mthdcls813.method813(var807,var922);
					var749 = mthdcls814.method814(var682,var505);
					var113 = mthdcls815.method815(var854,var741);
					var249 = mthdcls816.method816(var186,var628);
					var124 = mthdcls817.method817(var719,var986);
					var742 = mthdcls818.method818(var311,var168);
					var811 = mthdcls819.method819(var581,var803);
					var558 = mthdcls820.method820(var401,var648);
					var757 = mthdcls821.method821(var711,var522);
					var294 = mthdcls822.method822(var125,var507);
					var802 = mthdcls823.method823(var68,var57);
					var107 = mthdcls824.method824(var640,var458);
					var71 = mthdcls825.method825(var527,var872);
					var179 = mthdcls826.method826(var412,var733);
					var962 = mthdcls827.method827(var196,var594);
					var200 = mthdcls828.method828(var190,var205);
					var548 = mthdcls829.method829(var425,var374);
					var754 = mthdcls830.method830(var327,var500);
					var855 = mthdcls831.method831(var466,var750);
					var815 = mthdcls832.method832(var109,var223);
					var434 = mthdcls833.method833(var585,var982);
					var561 = mthdcls834.method834(var836,var452);
					var666 = mthdcls835.method835(var928,var124);
					var786 = mthdcls836.method836(var552,var106);
					var620 = mthdcls837.method837(var386,var206);
					var49 = mthdcls838.method838(var231,var901);
					var793 = mthdcls839.method839(var290,var960);
					var706 = mthdcls840.method840(var185,var901);
					var73 = mthdcls841.method841(var216,var86);
					var495 = mthdcls842.method842(var270,var187);
					var792 = mthdcls843.method843(var143,var402);
					var86 = mthdcls844.method844(var627,var306);
					var112 = mthdcls845.method845(var575,var990);
					var545 = mthdcls846.method846(var969,var289);
					var411 = mthdcls847.method847(var257,var464);
					var668 = mthdcls848.method848(var568,var425);
					var426 = mthdcls849.method849(var638,var379);
					var255 = mthdcls850.method850(var544,var922);
					var431 = mthdcls851.method851(var290,var183);
					var895 = mthdcls852.method852(var934,var530);
					var339 = mthdcls853.method853(var90,var937);
					var578 = mthdcls854.method854(var817,var263);
					var694 = mthdcls855.method855(var363,var922);
					var538 = mthdcls856.method856(var480,var806);
					var474 = mthdcls857.method857(var154,var911);
					var705 = mthdcls858.method858(var162,var263);
					var470 = mthdcls859.method859(var783,var33);
					var190 = mthdcls860.method860(var560,var740);
					var847 = mthdcls861.method861(var701,var205);
					var823 = mthdcls862.method862(var269,var363);
					var612 = mthdcls863.method863(var749,var653);
					var451 = mthdcls864.method864(var65,var162);
					var814 = mthdcls865.method865(var257,var346);
					var758 = mthdcls866.method866(var505,var305);
					var954 = mthdcls867.method867(var643,var605);
					var189 = mthdcls868.method868(var694,var804);
					var74 = mthdcls869.method869(var729,var84);
					var359 = mthdcls870.method870(var626,var570);
					var917 = mthdcls871.method871(var590,var437);
					var486 = mthdcls872.method872(var513,var654);
					var448 = mthdcls873.method873(var436,var17);
					var163 = mthdcls874.method874(var32,var174);
					var851 = mthdcls875.method875(var830,var549);
					var721 = mthdcls876.method876(var10,var357);
					var188 = mthdcls877.method877(var741,var698);
					var673 = mthdcls878.method878(var564,var990);
					var822 = mthdcls879.method879(var256,var264);
					var719 = mthdcls880.method880(var310,var615);
					var915 = mthdcls881.method881(var175,var731);
					var490 = mthdcls882.method882(var418,var891);
					var311 = mthdcls883.method883(var983,var707);
					var624 = mthdcls884.method884(var748,var147);
					var117 = mthdcls885.method885(var213,var945);
					var931 = mthdcls886.method886(var624,var704);
					var981 = mthdcls887.method887(var817,var464);
					var365 = mthdcls888.method888(var49,var11);
					var655 = mthdcls889.method889(var742,var858);
					var946 = mthdcls890.method890(var67,var622);
					var342 = mthdcls891.method891(var465,var729);
					var393 = mthdcls892.method892(var414,var884);
					var57 = mthdcls893.method893(var638,var885);
					var618 = mthdcls894.method894(var97,var305);
					var376 = mthdcls895.method895(var370,var276);
					var333 = mthdcls896.method896(var624,var553);
					var334 = mthdcls897.method897(var80,var398);
					var317 = mthdcls898.method898(var652,var687);
					var272 = mthdcls899.method899(var450,var446);
					var338 = mthdcls900.method900(var75,var534);
					var833 = mthdcls901.method901(var672,var97);
					var89 = mthdcls902.method902(var527,var442);
					var771 = mthdcls903.method903(var746,var936);
					var611 = mthdcls904.method904(var307,var71);
					var369 = mthdcls905.method905(var900,var419);
					var147 = mthdcls906.method906(var712,var361);
					var738 = mthdcls907.method907(var205,var296);
					var520 = mthdcls908.method908(var871,var532);
					var788 = mthdcls909.method909(var670,var367);
					var640 = mthdcls910.method910(var865,var972);
					var113 = mthdcls911.method911(var311,var701);
					var273 = mthdcls912.method912(var310,var25);
					var936 = mthdcls913.method913(var475,var951);
					var129 = mthdcls914.method914(var620,var262);
					var814 = mthdcls915.method915(var129,var784);
					var877 = mthdcls916.method916(var446,var119);
					var899 = mthdcls917.method917(var602,var83);
					var805 = mthdcls918.method918(var472,var102);
					var420 = mthdcls919.method919(var522,var971);
					var823 = mthdcls920.method920(var778,var501);
					var506 = mthdcls921.method921(var295,var794);
					var807 = mthdcls922.method922(var991,var44);
					var257 = mthdcls923.method923(var239,var116);
					var541 = mthdcls924.method924(var700,var593);
					var462 = mthdcls925.method925(var136,var121);
					var490 = mthdcls926.method926(var47,var768);
					var111 = mthdcls927.method927(var266,var698);
					var543 = mthdcls928.method928(var861,var363);
					var466 = mthdcls929.method929(var818,var516);
					var894 = mthdcls930.method930(var282,var266);
					var768 = mthdcls931.method931(var696,var359);
					var409 = mthdcls932.method932(var428,var27);
					var800 = mthdcls933.method933(var678,var7);
					var294 = mthdcls934.method934(var830,var677);
					var7 = mthdcls935.method935(var632,var903);
					var539 = mthdcls936.method936(var944,var108);
					var636 = mthdcls937.method937(var602,var6);
					var77 = mthdcls938.method938(var541,var512);
					var733 = mthdcls939.method939(var809,var935);
					var385 = mthdcls940.method940(var378,var963);
					var243 = mthdcls941.method941(var579,var231);
					var821 = mthdcls942.method942(var711,var22);
					var585 = mthdcls943.method943(var829,var232);
					var581 = mthdcls944.method944(var545,var938);
					var132 = mthdcls945.method945(var509,var260);
					var621 = mthdcls946.method946(var1,var349);
					var629 = mthdcls947.method947(var656,var882);
					var130 = mthdcls948.method948(var515,var318);
					var22 = mthdcls949.method949(var188,var464);
					var538 = mthdcls950.method950(var697,var406);
					var441 = mthdcls951.method951(var971,var445);
					var425 = mthdcls952.method952(var748,var891);
					var545 = mthdcls953.method953(var498,var372);
					var281 = mthdcls954.method954(var538,var594);
					var478 = mthdcls955.method955(var979,var720);
					var875 = mthdcls956.method956(var561,var222);
					var489 = mthdcls957.method957(var544,var344);
					var396 = mthdcls958.method958(var687,var972);
					var522 = mthdcls959.method959(var272,var133);
					var805 = mthdcls960.method960(var72,var820);
					var477 = mthdcls961.method961(var87,var456);
					var950 = mthdcls962.method962(var43,var951);
					var459 = mthdcls963.method963(var153,var788);
					var384 = mthdcls964.method964(var60,var779);
					var860 = mthdcls965.method965(var85,var312);
					var485 = mthdcls966.method966(var442,var542);
					var796 = mthdcls967.method967(var749,var55);
					var658 = mthdcls968.method968(var465,var877);
					var928 = mthdcls969.method969(var354,var514);
					var494 = mthdcls970.method970(var382,var797);
					var431 = mthdcls971.method971(var392,var709);
					var987 = mthdcls972.method972(var221,var758);
					var734 = mthdcls973.method973(var393,var667);
					var234 = mthdcls974.method974(var432,var18);
					var426 = mthdcls975.method975(var739,var489);
					var686 = mthdcls976.method976(var931,var810);
					var43 = mthdcls977.method977(var167,var757);
					var639 = mthdcls978.method978(var423,var275);
					var388 = mthdcls979.method979(var85,var377);
					var468 = mthdcls980.method980(var729,var284);
					var216 = mthdcls981.method981(var66,var485);
					var554 = mthdcls982.method982(var952,var41);
					var353 = mthdcls983.method983(var120,var595);
					var626 = mthdcls984.method984(var554,var632);
					var498 = mthdcls985.method985(var628,var992);
					var416 = mthdcls986.method986(var235,var190);
					var488 = mthdcls987.method987(var843,var977);
					var46 = mthdcls988.method988(var764,var97);
					var580 = mthdcls989.method989(var365,var906);
					var155 = mthdcls990.method990(var35,var180);
					var405 = mthdcls991.method991(var614,var798);
					var41 = mthdcls992.method992(var680,var800);
					var519 = mthdcls993.method993(var797,var747);
					var73 = mthdcls994.method994(var450,var741);
					var321 = mthdcls995.method995(var200,var190);
					var711 = mthdcls996.method996(var874,var326);
					var695 = mthdcls997.method997(var23,var178);
					var222 = mthdcls998.method998(var432,var549);
					var879 = mthdcls999.method999(var602,var267);
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
	public static int method0 (int var931, int var500) {
		if (var931>var500)
			return (var931-var500);
		else
			return (var500-var931+1);
	}
}

class mthdcls1 {
	public static int method1 (int var880, int var603) {
		if (var880>var603)
			return (var880+var603);
		else
			return (var603+var880+1);
	}
}

class mthdcls2 {
	public static int method2 (int var888, int var483) {
		if (var888>var483)
			return (var888-var483);
		else
			return (var483-var888+1);
	}
}

class mthdcls3 {
	public static int method3 (int var887, int var594) {
		if (var887>var594)
			return (var887+var594);
		else
			return (var594+var887+1);
	}
}

class mthdcls4 {
	public static int method4 (int var450, int var858) {
		if (var450>var858)
			return (var450-var858);
		else
			return (var858-var450+1);
	}
}

class mthdcls5 {
	public static int method5 (int var347, int var507) {
		if (var347>var507)
			return (var347-var507);
		else
			return (var507-var347+1);
	}
}

class mthdcls6 {
	public static int method6 (int var906, int var284) {
		if (var906>var284)
			return (var906-var284);
		else
			return (var284-var906+1);
	}
}

class mthdcls7 {
	public static int method7 (int var470, int var417) {
		if (var470>var417)
			return (var470+var417);
		else
			return (var417+var470+1);
	}
}

class mthdcls8 {
	public static int method8 (int var817, int var582) {
		if (var817>var582)
			return (var817*var582);
		else
			return (var582*var817+1);
	}
}

class mthdcls9 {
	public static int method9 (int var848, int var371) {
		if (var848>var371)
			return (var848+var371);
		else
			return (var371+var848+1);
	}
}

class mthdcls10 {
	public static int method10 (int var722, int var424) {
		if (var722>var424)
			return (var722*var424);
		else
			return (var424*var722+1);
	}
}

class mthdcls11 {
	public static int method11 (int var925, int var185) {
		if (var925>var185)
			return (var925-var185);
		else
			return (var185-var925+1);
	}
}

class mthdcls12 {
	public static int method12 (int var182, int var99) {
		if (var182>var99)
			return (var182-var99);
		else
			return (var99-var182+1);
	}
}

class mthdcls13 {
	public static int method13 (int var367, int var493) {
		if (var367>var493)
			return (var367*var493);
		else
			return (var493*var367+1);
	}
}

class mthdcls14 {
	public static int method14 (int var683, int var752) {
		if (var683>var752)
			return (var683-var752);
		else
			return (var752-var683+1);
	}
}

class mthdcls15 {
	public static int method15 (int var755, int var378) {
		if (var755>var378)
			return (var755*var378);
		else
			return (var378*var755+1);
	}
}

class mthdcls16 {
	public static int method16 (int var760, int var97) {
		if (var760>var97)
			return (var760-var97);
		else
			return (var97-var760+1);
	}
}

class mthdcls17 {
	public static int method17 (int var668, int var678) {
		if (var668>var678)
			return (var668*var678);
		else
			return (var678*var668+1);
	}
}

class mthdcls18 {
	public static int method18 (int var676, int var309) {
		if (var676>var309)
			return (var676*var309);
		else
			return (var309*var676+1);
	}
}

class mthdcls19 {
	public static int method19 (int var140, int var161) {
		if (var140>var161)
			return (var140*var161);
		else
			return (var161*var140+1);
	}
}

class mthdcls20 {
	public static int method20 (int var600, int var895) {
		if (var600>var895)
			return (var600*var895);
		else
			return (var895*var600+1);
	}
}

class mthdcls21 {
	public static int method21 (int var661, int var388) {
		if (var661>var388)
			return (var661*var388);
		else
			return (var388*var661+1);
	}
}

class mthdcls22 {
	public static int method22 (int var821, int var109) {
		if (var821>var109)
			return (var821*var109);
		else
			return (var109*var821+1);
	}
}

class mthdcls23 {
	public static int method23 (int var703, int var219) {
		if (var703>var219)
			return (var703*var219);
		else
			return (var219*var703+1);
	}
}

class mthdcls24 {
	public static int method24 (int var748, int var720) {
		if (var748>var720)
			return (var748+var720);
		else
			return (var720+var748+1);
	}
}

class mthdcls25 {
	public static int method25 (int var45, int var310) {
		if (var45>var310)
			return (var45+var310);
		else
			return (var310+var45+1);
	}
}

class mthdcls26 {
	public static int method26 (int var498, int var649) {
		if (var498>var649)
			return (var498+var649);
		else
			return (var649+var498+1);
	}
}

class mthdcls27 {
	public static int method27 (int var831, int var768) {
		if (var831>var768)
			return (var831*var768);
		else
			return (var768*var831+1);
	}
}

class mthdcls28 {
	public static int method28 (int var931, int var435) {
		if (var931>var435)
			return (var931*var435);
		else
			return (var435*var931+1);
	}
}

class mthdcls29 {
	public static int method29 (int var674, int var97) {
		if (var674>var97)
			return (var674-var97);
		else
			return (var97-var674+1);
	}
}

class mthdcls30 {
	public static int method30 (int var349, int var728) {
		if (var349>var728)
			return (var349+var728);
		else
			return (var728+var349+1);
	}
}

class mthdcls31 {
	public static int method31 (int var632, int var962) {
		if (var632>var962)
			return (var632-var962);
		else
			return (var962-var632+1);
	}
}

class mthdcls32 {
	public static int method32 (int var810, int var568) {
		if (var810>var568)
			return (var810+var568);
		else
			return (var568+var810+1);
	}
}

class mthdcls33 {
	public static int method33 (int var330, int var556) {
		if (var330>var556)
			return (var330*var556);
		else
			return (var556*var330+1);
	}
}

class mthdcls34 {
	public static int method34 (int var964, int var857) {
		if (var964>var857)
			return (var964+var857);
		else
			return (var857+var964+1);
	}
}

class mthdcls35 {
	public static int method35 (int var713, int var49) {
		if (var713>var49)
			return (var713+var49);
		else
			return (var49+var713+1);
	}
}

class mthdcls36 {
	public static int method36 (int var759, int var382) {
		if (var759>var382)
			return (var759-var382);
		else
			return (var382-var759+1);
	}
}

class mthdcls37 {
	public static int method37 (int var587, int var123) {
		if (var587>var123)
			return (var587+var123);
		else
			return (var123+var587+1);
	}
}

class mthdcls38 {
	public static int method38 (int var909, int var388) {
		if (var909>var388)
			return (var909+var388);
		else
			return (var388+var909+1);
	}
}

class mthdcls39 {
	public static int method39 (int var980, int var188) {
		if (var980>var188)
			return (var980-var188);
		else
			return (var188-var980+1);
	}
}

class mthdcls40 {
	public static int method40 (int var684, int var834) {
		if (var684>var834)
			return (var684*var834);
		else
			return (var834*var684+1);
	}
}

class mthdcls41 {
	public static int method41 (int var54, int var270) {
		if (var54>var270)
			return (var54-var270);
		else
			return (var270-var54+1);
	}
}

class mthdcls42 {
	public static int method42 (int var675, int var827) {
		if (var675>var827)
			return (var675*var827);
		else
			return (var827*var675+1);
	}
}

class mthdcls43 {
	public static int method43 (int var510, int var318) {
		if (var510>var318)
			return (var510+var318);
		else
			return (var318+var510+1);
	}
}

class mthdcls44 {
	public static int method44 (int var818, int var368) {
		if (var818>var368)
			return (var818*var368);
		else
			return (var368*var818+1);
	}
}

class mthdcls45 {
	public static int method45 (int var982, int var447) {
		if (var982>var447)
			return (var982*var447);
		else
			return (var447*var982+1);
	}
}

class mthdcls46 {
	public static int method46 (int var883, int var836) {
		if (var883>var836)
			return (var883-var836);
		else
			return (var836-var883+1);
	}
}

class mthdcls47 {
	public static int method47 (int var501, int var848) {
		if (var501>var848)
			return (var501-var848);
		else
			return (var848-var501+1);
	}
}

class mthdcls48 {
	public static int method48 (int var181, int var112) {
		if (var181>var112)
			return (var181+var112);
		else
			return (var112+var181+1);
	}
}

class mthdcls49 {
	public static int method49 (int var538, int var527) {
		if (var538>var527)
			return (var538+var527);
		else
			return (var527+var538+1);
	}
}

class mthdcls50 {
	public static int method50 (int var564, int var10) {
		if (var564>var10)
			return (var564-var10);
		else
			return (var10-var564+1);
	}
}

class mthdcls51 {
	public static int method51 (int var141, int var257) {
		if (var141>var257)
			return (var141-var257);
		else
			return (var257-var141+1);
	}
}

class mthdcls52 {
	public static int method52 (int var504, int var55) {
		if (var504>var55)
			return (var504-var55);
		else
			return (var55-var504+1);
	}
}

class mthdcls53 {
	public static int method53 (int var687, int var775) {
		if (var687>var775)
			return (var687-var775);
		else
			return (var775-var687+1);
	}
}

class mthdcls54 {
	public static int method54 (int var652, int var165) {
		if (var652>var165)
			return (var652+var165);
		else
			return (var165+var652+1);
	}
}

class mthdcls55 {
	public static int method55 (int var731, int var19) {
		if (var731>var19)
			return (var731+var19);
		else
			return (var19+var731+1);
	}
}

class mthdcls56 {
	public static int method56 (int var262, int var231) {
		if (var262>var231)
			return (var262+var231);
		else
			return (var231+var262+1);
	}
}

class mthdcls57 {
	public static int method57 (int var177, int var456) {
		if (var177>var456)
			return (var177*var456);
		else
			return (var456*var177+1);
	}
}

class mthdcls58 {
	public static int method58 (int var864, int var158) {
		if (var864>var158)
			return (var864*var158);
		else
			return (var158*var864+1);
	}
}

class mthdcls59 {
	public static int method59 (int var760, int var533) {
		if (var760>var533)
			return (var760+var533);
		else
			return (var533+var760+1);
	}
}

class mthdcls60 {
	public static int method60 (int var183, int var479) {
		if (var183>var479)
			return (var183-var479);
		else
			return (var479-var183+1);
	}
}

class mthdcls61 {
	public static int method61 (int var116, int var638) {
		if (var116>var638)
			return (var116-var638);
		else
			return (var638-var116+1);
	}
}

class mthdcls62 {
	public static int method62 (int var849, int var569) {
		if (var849>var569)
			return (var849+var569);
		else
			return (var569+var849+1);
	}
}

class mthdcls63 {
	public static int method63 (int var350, int var229) {
		if (var350>var229)
			return (var350*var229);
		else
			return (var229*var350+1);
	}
}

class mthdcls64 {
	public static int method64 (int var145, int var225) {
		if (var145>var225)
			return (var145*var225);
		else
			return (var225*var145+1);
	}
}

class mthdcls65 {
	public static int method65 (int var497, int var973) {
		if (var497>var973)
			return (var497-var973);
		else
			return (var973-var497+1);
	}
}

class mthdcls66 {
	public static int method66 (int var871, int var999) {
		if (var871>var999)
			return (var871+var999);
		else
			return (var999+var871+1);
	}
}

class mthdcls67 {
	public static int method67 (int var658, int var995) {
		if (var658>var995)
			return (var658-var995);
		else
			return (var995-var658+1);
	}
}

class mthdcls68 {
	public static int method68 (int var315, int var420) {
		if (var315>var420)
			return (var315-var420);
		else
			return (var420-var315+1);
	}
}

class mthdcls69 {
	public static int method69 (int var669, int var613) {
		if (var669>var613)
			return (var669*var613);
		else
			return (var613*var669+1);
	}
}

class mthdcls70 {
	public static int method70 (int var85, int var466) {
		if (var85>var466)
			return (var85-var466);
		else
			return (var466-var85+1);
	}
}

class mthdcls71 {
	public static int method71 (int var959, int var53) {
		if (var959>var53)
			return (var959+var53);
		else
			return (var53+var959+1);
	}
}

class mthdcls72 {
	public static int method72 (int var287, int var31) {
		if (var287>var31)
			return (var287-var31);
		else
			return (var31-var287+1);
	}
}

class mthdcls73 {
	public static int method73 (int var874, int var888) {
		if (var874>var888)
			return (var874+var888);
		else
			return (var888+var874+1);
	}
}

class mthdcls74 {
	public static int method74 (int var163, int var129) {
		if (var163>var129)
			return (var163-var129);
		else
			return (var129-var163+1);
	}
}

class mthdcls75 {
	public static int method75 (int var969, int var977) {
		if (var969>var977)
			return (var969-var977);
		else
			return (var977-var969+1);
	}
}

class mthdcls76 {
	public static int method76 (int var483, int var478) {
		if (var483>var478)
			return (var483+var478);
		else
			return (var478+var483+1);
	}
}

class mthdcls77 {
	public static int method77 (int var270, int var144) {
		if (var270>var144)
			return (var270*var144);
		else
			return (var144*var270+1);
	}
}

class mthdcls78 {
	public static int method78 (int var938, int var15) {
		if (var938>var15)
			return (var938+var15);
		else
			return (var15+var938+1);
	}
}

class mthdcls79 {
	public static int method79 (int var98, int var618) {
		if (var98>var618)
			return (var98+var618);
		else
			return (var618+var98+1);
	}
}

class mthdcls80 {
	public static int method80 (int var835, int var761) {
		if (var835>var761)
			return (var835+var761);
		else
			return (var761+var835+1);
	}
}

class mthdcls81 {
	public static int method81 (int var25, int var680) {
		if (var25>var680)
			return (var25-var680);
		else
			return (var680-var25+1);
	}
}

class mthdcls82 {
	public static int method82 (int var936, int var993) {
		if (var936>var993)
			return (var936+var993);
		else
			return (var993+var936+1);
	}
}

class mthdcls83 {
	public static int method83 (int var343, int var106) {
		if (var343>var106)
			return (var343+var106);
		else
			return (var106+var343+1);
	}
}

class mthdcls84 {
	public static int method84 (int var527, int var369) {
		if (var527>var369)
			return (var527-var369);
		else
			return (var369-var527+1);
	}
}

class mthdcls85 {
	public static int method85 (int var954, int var300) {
		if (var954>var300)
			return (var954*var300);
		else
			return (var300*var954+1);
	}
}

class mthdcls86 {
	public static int method86 (int var195, int var888) {
		if (var195>var888)
			return (var195*var888);
		else
			return (var888*var195+1);
	}
}

class mthdcls87 {
	public static int method87 (int var863, int var296) {
		if (var863>var296)
			return (var863+var296);
		else
			return (var296+var863+1);
	}
}

class mthdcls88 {
	public static int method88 (int var192, int var618) {
		if (var192>var618)
			return (var192-var618);
		else
			return (var618-var192+1);
	}
}

class mthdcls89 {
	public static int method89 (int var634, int var375) {
		if (var634>var375)
			return (var634+var375);
		else
			return (var375+var634+1);
	}
}

class mthdcls90 {
	public static int method90 (int var951, int var351) {
		if (var951>var351)
			return (var951*var351);
		else
			return (var351*var951+1);
	}
}

class mthdcls91 {
	public static int method91 (int var189, int var919) {
		if (var189>var919)
			return (var189*var919);
		else
			return (var919*var189+1);
	}
}

class mthdcls92 {
	public static int method92 (int var819, int var60) {
		if (var819>var60)
			return (var819*var60);
		else
			return (var60*var819+1);
	}
}

class mthdcls93 {
	public static int method93 (int var755, int var597) {
		if (var755>var597)
			return (var755+var597);
		else
			return (var597+var755+1);
	}
}

class mthdcls94 {
	public static int method94 (int var569, int var371) {
		if (var569>var371)
			return (var569-var371);
		else
			return (var371-var569+1);
	}
}

class mthdcls95 {
	public static int method95 (int var45, int var63) {
		if (var45>var63)
			return (var45+var63);
		else
			return (var63+var45+1);
	}
}

class mthdcls96 {
	public static int method96 (int var41, int var552) {
		if (var41>var552)
			return (var41+var552);
		else
			return (var552+var41+1);
	}
}

class mthdcls97 {
	public static int method97 (int var916, int var986) {
		if (var916>var986)
			return (var916+var986);
		else
			return (var986+var916+1);
	}
}

class mthdcls98 {
	public static int method98 (int var549, int var567) {
		if (var549>var567)
			return (var549-var567);
		else
			return (var567-var549+1);
	}
}

class mthdcls99 {
	public static int method99 (int var544, int var503) {
		if (var544>var503)
			return (var544+var503);
		else
			return (var503+var544+1);
	}
}

class mthdcls100 {
	public static int method100 (int var319, int var795) {
		if (var319>var795)
			return (var319*var795);
		else
			return (var795*var319+1);
	}
}

class mthdcls101 {
	public static int method101 (int var250, int var761) {
		if (var250>var761)
			return (var250+var761);
		else
			return (var761+var250+1);
	}
}

class mthdcls102 {
	public static int method102 (int var576, int var35) {
		if (var576>var35)
			return (var576*var35);
		else
			return (var35*var576+1);
	}
}

class mthdcls103 {
	public static int method103 (int var458, int var833) {
		if (var458>var833)
			return (var458+var833);
		else
			return (var833+var458+1);
	}
}

class mthdcls104 {
	public static int method104 (int var801, int var577) {
		if (var801>var577)
			return (var801*var577);
		else
			return (var577*var801+1);
	}
}

class mthdcls105 {
	public static int method105 (int var374, int var547) {
		if (var374>var547)
			return (var374-var547);
		else
			return (var547-var374+1);
	}
}

class mthdcls106 {
	public static int method106 (int var741, int var376) {
		if (var741>var376)
			return (var741*var376);
		else
			return (var376*var741+1);
	}
}

class mthdcls107 {
	public static int method107 (int var346, int var21) {
		if (var346>var21)
			return (var346-var21);
		else
			return (var21-var346+1);
	}
}

class mthdcls108 {
	public static int method108 (int var794, int var544) {
		if (var794>var544)
			return (var794-var544);
		else
			return (var544-var794+1);
	}
}

class mthdcls109 {
	public static int method109 (int var605, int var937) {
		if (var605>var937)
			return (var605+var937);
		else
			return (var937+var605+1);
	}
}

class mthdcls110 {
	public static int method110 (int var750, int var174) {
		if (var750>var174)
			return (var750-var174);
		else
			return (var174-var750+1);
	}
}

class mthdcls111 {
	public static int method111 (int var611, int var334) {
		if (var611>var334)
			return (var611+var334);
		else
			return (var334+var611+1);
	}
}

class mthdcls112 {
	public static int method112 (int var826, int var70) {
		if (var826>var70)
			return (var826*var70);
		else
			return (var70*var826+1);
	}
}

class mthdcls113 {
	public static int method113 (int var156, int var875) {
		if (var156>var875)
			return (var156-var875);
		else
			return (var875-var156+1);
	}
}

class mthdcls114 {
	public static int method114 (int var1, int var584) {
		if (var1>var584)
			return (var1*var584);
		else
			return (var584*var1+1);
	}
}

class mthdcls115 {
	public static int method115 (int var155, int var863) {
		if (var155>var863)
			return (var155*var863);
		else
			return (var863*var155+1);
	}
}

class mthdcls116 {
	public static int method116 (int var284, int var147) {
		if (var284>var147)
			return (var284-var147);
		else
			return (var147-var284+1);
	}
}

class mthdcls117 {
	public static int method117 (int var221, int var333) {
		if (var221>var333)
			return (var221+var333);
		else
			return (var333+var221+1);
	}
}

class mthdcls118 {
	public static int method118 (int var751, int var133) {
		if (var751>var133)
			return (var751*var133);
		else
			return (var133*var751+1);
	}
}

class mthdcls119 {
	public static int method119 (int var391, int var485) {
		if (var391>var485)
			return (var391-var485);
		else
			return (var485-var391+1);
	}
}

class mthdcls120 {
	public static int method120 (int var375, int var678) {
		if (var375>var678)
			return (var375-var678);
		else
			return (var678-var375+1);
	}
}

class mthdcls121 {
	public static int method121 (int var865, int var597) {
		if (var865>var597)
			return (var865+var597);
		else
			return (var597+var865+1);
	}
}

class mthdcls122 {
	public static int method122 (int var259, int var483) {
		if (var259>var483)
			return (var259-var483);
		else
			return (var483-var259+1);
	}
}

class mthdcls123 {
	public static int method123 (int var716, int var385) {
		if (var716>var385)
			return (var716-var385);
		else
			return (var385-var716+1);
	}
}

class mthdcls124 {
	public static int method124 (int var423, int var355) {
		if (var423>var355)
			return (var423+var355);
		else
			return (var355+var423+1);
	}
}

class mthdcls125 {
	public static int method125 (int var942, int var421) {
		if (var942>var421)
			return (var942*var421);
		else
			return (var421*var942+1);
	}
}

class mthdcls126 {
	public static int method126 (int var695, int var621) {
		if (var695>var621)
			return (var695*var621);
		else
			return (var621*var695+1);
	}
}

class mthdcls127 {
	public static int method127 (int var10, int var873) {
		if (var10>var873)
			return (var10+var873);
		else
			return (var873+var10+1);
	}
}

class mthdcls128 {
	public static int method128 (int var324, int var797) {
		if (var324>var797)
			return (var324-var797);
		else
			return (var797-var324+1);
	}
}

class mthdcls129 {
	public static int method129 (int var147, int var701) {
		if (var147>var701)
			return (var147-var701);
		else
			return (var701-var147+1);
	}
}

class mthdcls130 {
	public static int method130 (int var724, int var97) {
		if (var724>var97)
			return (var724*var97);
		else
			return (var97*var724+1);
	}
}

class mthdcls131 {
	public static int method131 (int var390, int var353) {
		if (var390>var353)
			return (var390+var353);
		else
			return (var353+var390+1);
	}
}

class mthdcls132 {
	public static int method132 (int var438, int var704) {
		if (var438>var704)
			return (var438-var704);
		else
			return (var704-var438+1);
	}
}

class mthdcls133 {
	public static int method133 (int var475, int var542) {
		if (var475>var542)
			return (var475*var542);
		else
			return (var542*var475+1);
	}
}

class mthdcls134 {
	public static int method134 (int var354, int var765) {
		if (var354>var765)
			return (var354+var765);
		else
			return (var765+var354+1);
	}
}

class mthdcls135 {
	public static int method135 (int var450, int var975) {
		if (var450>var975)
			return (var450+var975);
		else
			return (var975+var450+1);
	}
}

class mthdcls136 {
	public static int method136 (int var759, int var553) {
		if (var759>var553)
			return (var759*var553);
		else
			return (var553*var759+1);
	}
}

class mthdcls137 {
	public static int method137 (int var995, int var230) {
		if (var995>var230)
			return (var995+var230);
		else
			return (var230+var995+1);
	}
}

class mthdcls138 {
	public static int method138 (int var917, int var260) {
		if (var917>var260)
			return (var917-var260);
		else
			return (var260-var917+1);
	}
}

class mthdcls139 {
	public static int method139 (int var946, int var704) {
		if (var946>var704)
			return (var946*var704);
		else
			return (var704*var946+1);
	}
}

class mthdcls140 {
	public static int method140 (int var267, int var371) {
		if (var267>var371)
			return (var267*var371);
		else
			return (var371*var267+1);
	}
}

class mthdcls141 {
	public static int method141 (int var422, int var902) {
		if (var422>var902)
			return (var422-var902);
		else
			return (var902-var422+1);
	}
}

class mthdcls142 {
	public static int method142 (int var38, int var943) {
		if (var38>var943)
			return (var38-var943);
		else
			return (var943-var38+1);
	}
}

class mthdcls143 {
	public static int method143 (int var705, int var402) {
		if (var705>var402)
			return (var705+var402);
		else
			return (var402+var705+1);
	}
}

class mthdcls144 {
	public static int method144 (int var180, int var943) {
		if (var180>var943)
			return (var180+var943);
		else
			return (var943+var180+1);
	}
}

class mthdcls145 {
	public static int method145 (int var407, int var186) {
		if (var407>var186)
			return (var407+var186);
		else
			return (var186+var407+1);
	}
}

class mthdcls146 {
	public static int method146 (int var437, int var163) {
		if (var437>var163)
			return (var437*var163);
		else
			return (var163*var437+1);
	}
}

class mthdcls147 {
	public static int method147 (int var119, int var404) {
		if (var119>var404)
			return (var119-var404);
		else
			return (var404-var119+1);
	}
}

class mthdcls148 {
	public static int method148 (int var727, int var759) {
		if (var727>var759)
			return (var727-var759);
		else
			return (var759-var727+1);
	}
}

class mthdcls149 {
	public static int method149 (int var270, int var540) {
		if (var270>var540)
			return (var270-var540);
		else
			return (var540-var270+1);
	}
}

class mthdcls150 {
	public static int method150 (int var372, int var290) {
		if (var372>var290)
			return (var372-var290);
		else
			return (var290-var372+1);
	}
}

class mthdcls151 {
	public static int method151 (int var823, int var533) {
		if (var823>var533)
			return (var823-var533);
		else
			return (var533-var823+1);
	}
}

class mthdcls152 {
	public static int method152 (int var823, int var239) {
		if (var823>var239)
			return (var823*var239);
		else
			return (var239*var823+1);
	}
}

class mthdcls153 {
	public static int method153 (int var585, int var928) {
		if (var585>var928)
			return (var585*var928);
		else
			return (var928*var585+1);
	}
}

class mthdcls154 {
	public static int method154 (int var644, int var494) {
		if (var644>var494)
			return (var644*var494);
		else
			return (var494*var644+1);
	}
}

class mthdcls155 {
	public static int method155 (int var632, int var940) {
		if (var632>var940)
			return (var632*var940);
		else
			return (var940*var632+1);
	}
}

class mthdcls156 {
	public static int method156 (int var570, int var193) {
		if (var570>var193)
			return (var570*var193);
		else
			return (var193*var570+1);
	}
}

class mthdcls157 {
	public static int method157 (int var579, int var419) {
		if (var579>var419)
			return (var579+var419);
		else
			return (var419+var579+1);
	}
}

class mthdcls158 {
	public static int method158 (int var120, int var639) {
		if (var120>var639)
			return (var120-var639);
		else
			return (var639-var120+1);
	}
}

class mthdcls159 {
	public static int method159 (int var601, int var850) {
		if (var601>var850)
			return (var601-var850);
		else
			return (var850-var601+1);
	}
}

class mthdcls160 {
	public static int method160 (int var519, int var687) {
		if (var519>var687)
			return (var519-var687);
		else
			return (var687-var519+1);
	}
}

class mthdcls161 {
	public static int method161 (int var772, int var528) {
		if (var772>var528)
			return (var772-var528);
		else
			return (var528-var772+1);
	}
}

class mthdcls162 {
	public static int method162 (int var188, int var469) {
		if (var188>var469)
			return (var188-var469);
		else
			return (var469-var188+1);
	}
}

class mthdcls163 {
	public static int method163 (int var961, int var213) {
		if (var961>var213)
			return (var961-var213);
		else
			return (var213-var961+1);
	}
}

class mthdcls164 {
	public static int method164 (int var170, int var913) {
		if (var170>var913)
			return (var170-var913);
		else
			return (var913-var170+1);
	}
}

class mthdcls165 {
	public static int method165 (int var931, int var683) {
		if (var931>var683)
			return (var931+var683);
		else
			return (var683+var931+1);
	}
}

class mthdcls166 {
	public static int method166 (int var720, int var638) {
		if (var720>var638)
			return (var720+var638);
		else
			return (var638+var720+1);
	}
}

class mthdcls167 {
	public static int method167 (int var155, int var746) {
		if (var155>var746)
			return (var155*var746);
		else
			return (var746*var155+1);
	}
}

class mthdcls168 {
	public static int method168 (int var305, int var729) {
		if (var305>var729)
			return (var305*var729);
		else
			return (var729*var305+1);
	}
}

class mthdcls169 {
	public static int method169 (int var978, int var890) {
		if (var978>var890)
			return (var978*var890);
		else
			return (var890*var978+1);
	}
}

class mthdcls170 {
	public static int method170 (int var23, int var246) {
		if (var23>var246)
			return (var23*var246);
		else
			return (var246*var23+1);
	}
}

class mthdcls171 {
	public static int method171 (int var488, int var195) {
		if (var488>var195)
			return (var488*var195);
		else
			return (var195*var488+1);
	}
}

class mthdcls172 {
	public static int method172 (int var919, int var917) {
		if (var919>var917)
			return (var919*var917);
		else
			return (var917*var919+1);
	}
}

class mthdcls173 {
	public static int method173 (int var694, int var671) {
		if (var694>var671)
			return (var694-var671);
		else
			return (var671-var694+1);
	}
}

class mthdcls174 {
	public static int method174 (int var382, int var684) {
		if (var382>var684)
			return (var382*var684);
		else
			return (var684*var382+1);
	}
}

class mthdcls175 {
	public static int method175 (int var549, int var620) {
		if (var549>var620)
			return (var549+var620);
		else
			return (var620+var549+1);
	}
}

class mthdcls176 {
	public static int method176 (int var446, int var329) {
		if (var446>var329)
			return (var446-var329);
		else
			return (var329-var446+1);
	}
}

class mthdcls177 {
	public static int method177 (int var724, int var272) {
		if (var724>var272)
			return (var724*var272);
		else
			return (var272*var724+1);
	}
}

class mthdcls178 {
	public static int method178 (int var994, int var640) {
		if (var994>var640)
			return (var994-var640);
		else
			return (var640-var994+1);
	}
}

class mthdcls179 {
	public static int method179 (int var988, int var134) {
		if (var988>var134)
			return (var988-var134);
		else
			return (var134-var988+1);
	}
}

class mthdcls180 {
	public static int method180 (int var256, int var234) {
		if (var256>var234)
			return (var256+var234);
		else
			return (var234+var256+1);
	}
}

class mthdcls181 {
	public static int method181 (int var196, int var520) {
		if (var196>var520)
			return (var196+var520);
		else
			return (var520+var196+1);
	}
}

class mthdcls182 {
	public static int method182 (int var283, int var779) {
		if (var283>var779)
			return (var283*var779);
		else
			return (var779*var283+1);
	}
}

class mthdcls183 {
	public static int method183 (int var166, int var951) {
		if (var166>var951)
			return (var166+var951);
		else
			return (var951+var166+1);
	}
}

class mthdcls184 {
	public static int method184 (int var251, int var394) {
		if (var251>var394)
			return (var251*var394);
		else
			return (var394*var251+1);
	}
}

class mthdcls185 {
	public static int method185 (int var206, int var16) {
		if (var206>var16)
			return (var206+var16);
		else
			return (var16+var206+1);
	}
}

class mthdcls186 {
	public static int method186 (int var660, int var920) {
		if (var660>var920)
			return (var660-var920);
		else
			return (var920-var660+1);
	}
}

class mthdcls187 {
	public static int method187 (int var474, int var125) {
		if (var474>var125)
			return (var474*var125);
		else
			return (var125*var474+1);
	}
}

class mthdcls188 {
	public static int method188 (int var401, int var544) {
		if (var401>var544)
			return (var401-var544);
		else
			return (var544-var401+1);
	}
}

class mthdcls189 {
	public static int method189 (int var454, int var860) {
		if (var454>var860)
			return (var454*var860);
		else
			return (var860*var454+1);
	}
}

class mthdcls190 {
	public static int method190 (int var78, int var249) {
		if (var78>var249)
			return (var78+var249);
		else
			return (var249+var78+1);
	}
}

class mthdcls191 {
	public static int method191 (int var65, int var248) {
		if (var65>var248)
			return (var65-var248);
		else
			return (var248-var65+1);
	}
}

class mthdcls192 {
	public static int method192 (int var979, int var674) {
		if (var979>var674)
			return (var979+var674);
		else
			return (var674+var979+1);
	}
}

class mthdcls193 {
	public static int method193 (int var659, int var267) {
		if (var659>var267)
			return (var659+var267);
		else
			return (var267+var659+1);
	}
}

class mthdcls194 {
	public static int method194 (int var833, int var283) {
		if (var833>var283)
			return (var833+var283);
		else
			return (var283+var833+1);
	}
}

class mthdcls195 {
	public static int method195 (int var65, int var935) {
		if (var65>var935)
			return (var65*var935);
		else
			return (var935*var65+1);
	}
}

class mthdcls196 {
	public static int method196 (int var701, int var59) {
		if (var701>var59)
			return (var701+var59);
		else
			return (var59+var701+1);
	}
}

class mthdcls197 {
	public static int method197 (int var885, int var640) {
		if (var885>var640)
			return (var885-var640);
		else
			return (var640-var885+1);
	}
}

class mthdcls198 {
	public static int method198 (int var415, int var561) {
		if (var415>var561)
			return (var415*var561);
		else
			return (var561*var415+1);
	}
}

class mthdcls199 {
	public static int method199 (int var356, int var422) {
		if (var356>var422)
			return (var356-var422);
		else
			return (var422-var356+1);
	}
}

class mthdcls200 {
	public static int method200 (int var86, int var339) {
		if (var86>var339)
			return (var86-var339);
		else
			return (var339-var86+1);
	}
}

class mthdcls201 {
	public static int method201 (int var59, int var177) {
		if (var59>var177)
			return (var59+var177);
		else
			return (var177+var59+1);
	}
}

class mthdcls202 {
	public static int method202 (int var842, int var450) {
		if (var842>var450)
			return (var842+var450);
		else
			return (var450+var842+1);
	}
}

class mthdcls203 {
	public static int method203 (int var975, int var504) {
		if (var975>var504)
			return (var975+var504);
		else
			return (var504+var975+1);
	}
}

class mthdcls204 {
	public static int method204 (int var834, int var334) {
		if (var834>var334)
			return (var834-var334);
		else
			return (var334-var834+1);
	}
}

class mthdcls205 {
	public static int method205 (int var891, int var974) {
		if (var891>var974)
			return (var891-var974);
		else
			return (var974-var891+1);
	}
}

class mthdcls206 {
	public static int method206 (int var587, int var517) {
		if (var587>var517)
			return (var587*var517);
		else
			return (var517*var587+1);
	}
}

class mthdcls207 {
	public static int method207 (int var208, int var518) {
		if (var208>var518)
			return (var208*var518);
		else
			return (var518*var208+1);
	}
}

class mthdcls208 {
	public static int method208 (int var440, int var375) {
		if (var440>var375)
			return (var440+var375);
		else
			return (var375+var440+1);
	}
}

class mthdcls209 {
	public static int method209 (int var960, int var621) {
		if (var960>var621)
			return (var960+var621);
		else
			return (var621+var960+1);
	}
}

class mthdcls210 {
	public static int method210 (int var224, int var617) {
		if (var224>var617)
			return (var224+var617);
		else
			return (var617+var224+1);
	}
}

class mthdcls211 {
	public static int method211 (int var749, int var563) {
		if (var749>var563)
			return (var749+var563);
		else
			return (var563+var749+1);
	}
}

class mthdcls212 {
	public static int method212 (int var974, int var632) {
		if (var974>var632)
			return (var974-var632);
		else
			return (var632-var974+1);
	}
}

class mthdcls213 {
	public static int method213 (int var214, int var893) {
		if (var214>var893)
			return (var214-var893);
		else
			return (var893-var214+1);
	}
}

class mthdcls214 {
	public static int method214 (int var899, int var157) {
		if (var899>var157)
			return (var899*var157);
		else
			return (var157*var899+1);
	}
}

class mthdcls215 {
	public static int method215 (int var217, int var363) {
		if (var217>var363)
			return (var217+var363);
		else
			return (var363+var217+1);
	}
}

class mthdcls216 {
	public static int method216 (int var226, int var132) {
		if (var226>var132)
			return (var226*var132);
		else
			return (var132*var226+1);
	}
}

class mthdcls217 {
	public static int method217 (int var754, int var409) {
		if (var754>var409)
			return (var754+var409);
		else
			return (var409+var754+1);
	}
}

class mthdcls218 {
	public static int method218 (int var359, int var933) {
		if (var359>var933)
			return (var359*var933);
		else
			return (var933*var359+1);
	}
}

class mthdcls219 {
	public static int method219 (int var760, int var581) {
		if (var760>var581)
			return (var760*var581);
		else
			return (var581*var760+1);
	}
}

class mthdcls220 {
	public static int method220 (int var618, int var941) {
		if (var618>var941)
			return (var618-var941);
		else
			return (var941-var618+1);
	}
}

class mthdcls221 {
	public static int method221 (int var301, int var795) {
		if (var301>var795)
			return (var301-var795);
		else
			return (var795-var301+1);
	}
}

class mthdcls222 {
	public static int method222 (int var148, int var611) {
		if (var148>var611)
			return (var148+var611);
		else
			return (var611+var148+1);
	}
}

class mthdcls223 {
	public static int method223 (int var866, int var390) {
		if (var866>var390)
			return (var866*var390);
		else
			return (var390*var866+1);
	}
}

class mthdcls224 {
	public static int method224 (int var258, int var949) {
		if (var258>var949)
			return (var258-var949);
		else
			return (var949-var258+1);
	}
}

class mthdcls225 {
	public static int method225 (int var802, int var309) {
		if (var802>var309)
			return (var802+var309);
		else
			return (var309+var802+1);
	}
}

class mthdcls226 {
	public static int method226 (int var14, int var536) {
		if (var14>var536)
			return (var14*var536);
		else
			return (var536*var14+1);
	}
}

class mthdcls227 {
	public static int method227 (int var591, int var560) {
		if (var591>var560)
			return (var591-var560);
		else
			return (var560-var591+1);
	}
}

class mthdcls228 {
	public static int method228 (int var8, int var623) {
		if (var8>var623)
			return (var8*var623);
		else
			return (var623*var8+1);
	}
}

class mthdcls229 {
	public static int method229 (int var496, int var438) {
		if (var496>var438)
			return (var496+var438);
		else
			return (var438+var496+1);
	}
}

class mthdcls230 {
	public static int method230 (int var874, int var173) {
		if (var874>var173)
			return (var874*var173);
		else
			return (var173*var874+1);
	}
}

class mthdcls231 {
	public static int method231 (int var606, int var512) {
		if (var606>var512)
			return (var606*var512);
		else
			return (var512*var606+1);
	}
}

class mthdcls232 {
	public static int method232 (int var918, int var474) {
		if (var918>var474)
			return (var918+var474);
		else
			return (var474+var918+1);
	}
}

class mthdcls233 {
	public static int method233 (int var990, int var545) {
		if (var990>var545)
			return (var990*var545);
		else
			return (var545*var990+1);
	}
}

class mthdcls234 {
	public static int method234 (int var932, int var820) {
		if (var932>var820)
			return (var932*var820);
		else
			return (var820*var932+1);
	}
}

class mthdcls235 {
	public static int method235 (int var298, int var937) {
		if (var298>var937)
			return (var298+var937);
		else
			return (var937+var298+1);
	}
}

class mthdcls236 {
	public static int method236 (int var737, int var765) {
		if (var737>var765)
			return (var737*var765);
		else
			return (var765*var737+1);
	}
}

class mthdcls237 {
	public static int method237 (int var784, int var653) {
		if (var784>var653)
			return (var784*var653);
		else
			return (var653*var784+1);
	}
}

class mthdcls238 {
	public static int method238 (int var30, int var908) {
		if (var30>var908)
			return (var30-var908);
		else
			return (var908-var30+1);
	}
}

class mthdcls239 {
	public static int method239 (int var319, int var298) {
		if (var319>var298)
			return (var319*var298);
		else
			return (var298*var319+1);
	}
}

class mthdcls240 {
	public static int method240 (int var604, int var412) {
		if (var604>var412)
			return (var604+var412);
		else
			return (var412+var604+1);
	}
}

class mthdcls241 {
	public static int method241 (int var262, int var569) {
		if (var262>var569)
			return (var262-var569);
		else
			return (var569-var262+1);
	}
}

class mthdcls242 {
	public static int method242 (int var886, int var294) {
		if (var886>var294)
			return (var886-var294);
		else
			return (var294-var886+1);
	}
}

class mthdcls243 {
	public static int method243 (int var653, int var34) {
		if (var653>var34)
			return (var653-var34);
		else
			return (var34-var653+1);
	}
}

class mthdcls244 {
	public static int method244 (int var142, int var287) {
		if (var142>var287)
			return (var142+var287);
		else
			return (var287+var142+1);
	}
}

class mthdcls245 {
	public static int method245 (int var192, int var739) {
		if (var192>var739)
			return (var192*var739);
		else
			return (var739*var192+1);
	}
}

class mthdcls246 {
	public static int method246 (int var870, int var570) {
		if (var870>var570)
			return (var870+var570);
		else
			return (var570+var870+1);
	}
}

class mthdcls247 {
	public static int method247 (int var283, int var628) {
		if (var283>var628)
			return (var283+var628);
		else
			return (var628+var283+1);
	}
}

class mthdcls248 {
	public static int method248 (int var934, int var855) {
		if (var934>var855)
			return (var934+var855);
		else
			return (var855+var934+1);
	}
}

class mthdcls249 {
	public static int method249 (int var283, int var862) {
		if (var283>var862)
			return (var283+var862);
		else
			return (var862+var283+1);
	}
}

class mthdcls250 {
	public static int method250 (int var622, int var897) {
		if (var622>var897)
			return (var622+var897);
		else
			return (var897+var622+1);
	}
}

class mthdcls251 {
	public static int method251 (int var460, int var564) {
		if (var460>var564)
			return (var460*var564);
		else
			return (var564*var460+1);
	}
}

class mthdcls252 {
	public static int method252 (int var37, int var980) {
		if (var37>var980)
			return (var37*var980);
		else
			return (var980*var37+1);
	}
}

class mthdcls253 {
	public static int method253 (int var15, int var443) {
		if (var15>var443)
			return (var15+var443);
		else
			return (var443+var15+1);
	}
}

class mthdcls254 {
	public static int method254 (int var477, int var999) {
		if (var477>var999)
			return (var477*var999);
		else
			return (var999*var477+1);
	}
}

class mthdcls255 {
	public static int method255 (int var285, int var554) {
		if (var285>var554)
			return (var285*var554);
		else
			return (var554*var285+1);
	}
}

class mthdcls256 {
	public static int method256 (int var671, int var754) {
		if (var671>var754)
			return (var671+var754);
		else
			return (var754+var671+1);
	}
}

class mthdcls257 {
	public static int method257 (int var573, int var17) {
		if (var573>var17)
			return (var573-var17);
		else
			return (var17-var573+1);
	}
}

class mthdcls258 {
	public static int method258 (int var249, int var689) {
		if (var249>var689)
			return (var249-var689);
		else
			return (var689-var249+1);
	}
}

class mthdcls259 {
	public static int method259 (int var723, int var574) {
		if (var723>var574)
			return (var723-var574);
		else
			return (var574-var723+1);
	}
}

class mthdcls260 {
	public static int method260 (int var92, int var901) {
		if (var92>var901)
			return (var92-var901);
		else
			return (var901-var92+1);
	}
}

class mthdcls261 {
	public static int method261 (int var342, int var332) {
		if (var342>var332)
			return (var342*var332);
		else
			return (var332*var342+1);
	}
}

class mthdcls262 {
	public static int method262 (int var183, int var59) {
		if (var183>var59)
			return (var183-var59);
		else
			return (var59-var183+1);
	}
}

class mthdcls263 {
	public static int method263 (int var288, int var665) {
		if (var288>var665)
			return (var288-var665);
		else
			return (var665-var288+1);
	}
}

class mthdcls264 {
	public static int method264 (int var647, int var285) {
		if (var647>var285)
			return (var647+var285);
		else
			return (var285+var647+1);
	}
}

class mthdcls265 {
	public static int method265 (int var840, int var575) {
		if (var840>var575)
			return (var840-var575);
		else
			return (var575-var840+1);
	}
}

class mthdcls266 {
	public static int method266 (int var887, int var245) {
		if (var887>var245)
			return (var887+var245);
		else
			return (var245+var887+1);
	}
}

class mthdcls267 {
	public static int method267 (int var101, int var491) {
		if (var101>var491)
			return (var101*var491);
		else
			return (var491*var101+1);
	}
}

class mthdcls268 {
	public static int method268 (int var815, int var251) {
		if (var815>var251)
			return (var815*var251);
		else
			return (var251*var815+1);
	}
}

class mthdcls269 {
	public static int method269 (int var266, int var395) {
		if (var266>var395)
			return (var266-var395);
		else
			return (var395-var266+1);
	}
}

class mthdcls270 {
	public static int method270 (int var107, int var589) {
		if (var107>var589)
			return (var107-var589);
		else
			return (var589-var107+1);
	}
}

class mthdcls271 {
	public static int method271 (int var176, int var980) {
		if (var176>var980)
			return (var176+var980);
		else
			return (var980+var176+1);
	}
}

class mthdcls272 {
	public static int method272 (int var802, int var168) {
		if (var802>var168)
			return (var802+var168);
		else
			return (var168+var802+1);
	}
}

class mthdcls273 {
	public static int method273 (int var473, int var892) {
		if (var473>var892)
			return (var473*var892);
		else
			return (var892*var473+1);
	}
}

class mthdcls274 {
	public static int method274 (int var85, int var301) {
		if (var85>var301)
			return (var85*var301);
		else
			return (var301*var85+1);
	}
}

class mthdcls275 {
	public static int method275 (int var65, int var731) {
		if (var65>var731)
			return (var65+var731);
		else
			return (var731+var65+1);
	}
}

class mthdcls276 {
	public static int method276 (int var952, int var983) {
		if (var952>var983)
			return (var952*var983);
		else
			return (var983*var952+1);
	}
}

class mthdcls277 {
	public static int method277 (int var489, int var495) {
		if (var489>var495)
			return (var489-var495);
		else
			return (var495-var489+1);
	}
}

class mthdcls278 {
	public static int method278 (int var483, int var643) {
		if (var483>var643)
			return (var483+var643);
		else
			return (var643+var483+1);
	}
}

class mthdcls279 {
	public static int method279 (int var101, int var545) {
		if (var101>var545)
			return (var101-var545);
		else
			return (var545-var101+1);
	}
}

class mthdcls280 {
	public static int method280 (int var159, int var599) {
		if (var159>var599)
			return (var159*var599);
		else
			return (var599*var159+1);
	}
}

class mthdcls281 {
	public static int method281 (int var426, int var556) {
		if (var426>var556)
			return (var426+var556);
		else
			return (var556+var426+1);
	}
}

class mthdcls282 {
	public static int method282 (int var298, int var257) {
		if (var298>var257)
			return (var298*var257);
		else
			return (var257*var298+1);
	}
}

class mthdcls283 {
	public static int method283 (int var906, int var883) {
		if (var906>var883)
			return (var906+var883);
		else
			return (var883+var906+1);
	}
}

class mthdcls284 {
	public static int method284 (int var105, int var974) {
		if (var105>var974)
			return (var105+var974);
		else
			return (var974+var105+1);
	}
}

class mthdcls285 {
	public static int method285 (int var879, int var336) {
		if (var879>var336)
			return (var879*var336);
		else
			return (var336*var879+1);
	}
}

class mthdcls286 {
	public static int method286 (int var8, int var236) {
		if (var8>var236)
			return (var8+var236);
		else
			return (var236+var8+1);
	}
}

class mthdcls287 {
	public static int method287 (int var466, int var609) {
		if (var466>var609)
			return (var466+var609);
		else
			return (var609+var466+1);
	}
}

class mthdcls288 {
	public static int method288 (int var3, int var969) {
		if (var3>var969)
			return (var3+var969);
		else
			return (var969+var3+1);
	}
}

class mthdcls289 {
	public static int method289 (int var825, int var817) {
		if (var825>var817)
			return (var825+var817);
		else
			return (var817+var825+1);
	}
}

class mthdcls290 {
	public static int method290 (int var878, int var262) {
		if (var878>var262)
			return (var878-var262);
		else
			return (var262-var878+1);
	}
}

class mthdcls291 {
	public static int method291 (int var615, int var83) {
		if (var615>var83)
			return (var615+var83);
		else
			return (var83+var615+1);
	}
}

class mthdcls292 {
	public static int method292 (int var634, int var649) {
		if (var634>var649)
			return (var634+var649);
		else
			return (var649+var634+1);
	}
}

class mthdcls293 {
	public static int method293 (int var79, int var806) {
		if (var79>var806)
			return (var79*var806);
		else
			return (var806*var79+1);
	}
}

class mthdcls294 {
	public static int method294 (int var596, int var348) {
		if (var596>var348)
			return (var596-var348);
		else
			return (var348-var596+1);
	}
}

class mthdcls295 {
	public static int method295 (int var731, int var879) {
		if (var731>var879)
			return (var731*var879);
		else
			return (var879*var731+1);
	}
}

class mthdcls296 {
	public static int method296 (int var379, int var932) {
		if (var379>var932)
			return (var379-var932);
		else
			return (var932-var379+1);
	}
}

class mthdcls297 {
	public static int method297 (int var267, int var865) {
		if (var267>var865)
			return (var267-var865);
		else
			return (var865-var267+1);
	}
}

class mthdcls298 {
	public static int method298 (int var339, int var314) {
		if (var339>var314)
			return (var339+var314);
		else
			return (var314+var339+1);
	}
}

class mthdcls299 {
	public static int method299 (int var257, int var444) {
		if (var257>var444)
			return (var257-var444);
		else
			return (var444-var257+1);
	}
}

class mthdcls300 {
	public static int method300 (int var640, int var511) {
		if (var640>var511)
			return (var640+var511);
		else
			return (var511+var640+1);
	}
}

class mthdcls301 {
	public static int method301 (int var878, int var118) {
		if (var878>var118)
			return (var878+var118);
		else
			return (var118+var878+1);
	}
}

class mthdcls302 {
	public static int method302 (int var590, int var783) {
		if (var590>var783)
			return (var590+var783);
		else
			return (var783+var590+1);
	}
}

class mthdcls303 {
	public static int method303 (int var550, int var0) {
		if (var550>var0)
			return (var550-var0);
		else
			return (var0-var550+1);
	}
}

class mthdcls304 {
	public static int method304 (int var922, int var159) {
		if (var922>var159)
			return (var922*var159);
		else
			return (var159*var922+1);
	}
}

class mthdcls305 {
	public static int method305 (int var825, int var284) {
		if (var825>var284)
			return (var825-var284);
		else
			return (var284-var825+1);
	}
}

class mthdcls306 {
	public static int method306 (int var9, int var292) {
		if (var9>var292)
			return (var9*var292);
		else
			return (var292*var9+1);
	}
}

class mthdcls307 {
	public static int method307 (int var928, int var76) {
		if (var928>var76)
			return (var928*var76);
		else
			return (var76*var928+1);
	}
}

class mthdcls308 {
	public static int method308 (int var757, int var691) {
		if (var757>var691)
			return (var757*var691);
		else
			return (var691*var757+1);
	}
}

class mthdcls309 {
	public static int method309 (int var329, int var134) {
		if (var329>var134)
			return (var329*var134);
		else
			return (var134*var329+1);
	}
}

class mthdcls310 {
	public static int method310 (int var10, int var196) {
		if (var10>var196)
			return (var10-var196);
		else
			return (var196-var10+1);
	}
}

class mthdcls311 {
	public static int method311 (int var40, int var113) {
		if (var40>var113)
			return (var40+var113);
		else
			return (var113+var40+1);
	}
}

class mthdcls312 {
	public static int method312 (int var304, int var418) {
		if (var304>var418)
			return (var304*var418);
		else
			return (var418*var304+1);
	}
}

class mthdcls313 {
	public static int method313 (int var516, int var337) {
		if (var516>var337)
			return (var516+var337);
		else
			return (var337+var516+1);
	}
}

class mthdcls314 {
	public static int method314 (int var753, int var118) {
		if (var753>var118)
			return (var753+var118);
		else
			return (var118+var753+1);
	}
}

class mthdcls315 {
	public static int method315 (int var789, int var815) {
		if (var789>var815)
			return (var789-var815);
		else
			return (var815-var789+1);
	}
}

class mthdcls316 {
	public static int method316 (int var773, int var684) {
		if (var773>var684)
			return (var773*var684);
		else
			return (var684*var773+1);
	}
}

class mthdcls317 {
	public static int method317 (int var960, int var314) {
		if (var960>var314)
			return (var960+var314);
		else
			return (var314+var960+1);
	}
}

class mthdcls318 {
	public static int method318 (int var854, int var846) {
		if (var854>var846)
			return (var854-var846);
		else
			return (var846-var854+1);
	}
}

class mthdcls319 {
	public static int method319 (int var768, int var121) {
		if (var768>var121)
			return (var768+var121);
		else
			return (var121+var768+1);
	}
}

class mthdcls320 {
	public static int method320 (int var539, int var889) {
		if (var539>var889)
			return (var539+var889);
		else
			return (var889+var539+1);
	}
}

class mthdcls321 {
	public static int method321 (int var287, int var309) {
		if (var287>var309)
			return (var287-var309);
		else
			return (var309-var287+1);
	}
}

class mthdcls322 {
	public static int method322 (int var917, int var623) {
		if (var917>var623)
			return (var917-var623);
		else
			return (var623-var917+1);
	}
}

class mthdcls323 {
	public static int method323 (int var174, int var730) {
		if (var174>var730)
			return (var174+var730);
		else
			return (var730+var174+1);
	}
}

class mthdcls324 {
	public static int method324 (int var341, int var819) {
		if (var341>var819)
			return (var341*var819);
		else
			return (var819*var341+1);
	}
}

class mthdcls325 {
	public static int method325 (int var345, int var651) {
		if (var345>var651)
			return (var345+var651);
		else
			return (var651+var345+1);
	}
}

class mthdcls326 {
	public static int method326 (int var879, int var164) {
		if (var879>var164)
			return (var879-var164);
		else
			return (var164-var879+1);
	}
}

class mthdcls327 {
	public static int method327 (int var50, int var499) {
		if (var50>var499)
			return (var50+var499);
		else
			return (var499+var50+1);
	}
}

class mthdcls328 {
	public static int method328 (int var200, int var345) {
		if (var200>var345)
			return (var200-var345);
		else
			return (var345-var200+1);
	}
}

class mthdcls329 {
	public static int method329 (int var640, int var754) {
		if (var640>var754)
			return (var640+var754);
		else
			return (var754+var640+1);
	}
}

class mthdcls330 {
	public static int method330 (int var342, int var408) {
		if (var342>var408)
			return (var342-var408);
		else
			return (var408-var342+1);
	}
}

class mthdcls331 {
	public static int method331 (int var969, int var221) {
		if (var969>var221)
			return (var969-var221);
		else
			return (var221-var969+1);
	}
}

class mthdcls332 {
	public static int method332 (int var967, int var335) {
		if (var967>var335)
			return (var967+var335);
		else
			return (var335+var967+1);
	}
}

class mthdcls333 {
	public static int method333 (int var71, int var392) {
		if (var71>var392)
			return (var71-var392);
		else
			return (var392-var71+1);
	}
}

class mthdcls334 {
	public static int method334 (int var689, int var487) {
		if (var689>var487)
			return (var689-var487);
		else
			return (var487-var689+1);
	}
}

class mthdcls335 {
	public static int method335 (int var668, int var827) {
		if (var668>var827)
			return (var668+var827);
		else
			return (var827+var668+1);
	}
}

class mthdcls336 {
	public static int method336 (int var280, int var201) {
		if (var280>var201)
			return (var280+var201);
		else
			return (var201+var280+1);
	}
}

class mthdcls337 {
	public static int method337 (int var760, int var638) {
		if (var760>var638)
			return (var760*var638);
		else
			return (var638*var760+1);
	}
}

class mthdcls338 {
	public static int method338 (int var852, int var879) {
		if (var852>var879)
			return (var852+var879);
		else
			return (var879+var852+1);
	}
}

class mthdcls339 {
	public static int method339 (int var580, int var354) {
		if (var580>var354)
			return (var580-var354);
		else
			return (var354-var580+1);
	}
}

class mthdcls340 {
	public static int method340 (int var474, int var136) {
		if (var474>var136)
			return (var474*var136);
		else
			return (var136*var474+1);
	}
}

class mthdcls341 {
	public static int method341 (int var830, int var164) {
		if (var830>var164)
			return (var830+var164);
		else
			return (var164+var830+1);
	}
}

class mthdcls342 {
	public static int method342 (int var65, int var229) {
		if (var65>var229)
			return (var65-var229);
		else
			return (var229-var65+1);
	}
}

class mthdcls343 {
	public static int method343 (int var993, int var357) {
		if (var993>var357)
			return (var993+var357);
		else
			return (var357+var993+1);
	}
}

class mthdcls344 {
	public static int method344 (int var555, int var741) {
		if (var555>var741)
			return (var555-var741);
		else
			return (var741-var555+1);
	}
}

class mthdcls345 {
	public static int method345 (int var640, int var183) {
		if (var640>var183)
			return (var640-var183);
		else
			return (var183-var640+1);
	}
}

class mthdcls346 {
	public static int method346 (int var860, int var610) {
		if (var860>var610)
			return (var860+var610);
		else
			return (var610+var860+1);
	}
}

class mthdcls347 {
	public static int method347 (int var561, int var278) {
		if (var561>var278)
			return (var561+var278);
		else
			return (var278+var561+1);
	}
}

class mthdcls348 {
	public static int method348 (int var421, int var227) {
		if (var421>var227)
			return (var421-var227);
		else
			return (var227-var421+1);
	}
}

class mthdcls349 {
	public static int method349 (int var833, int var545) {
		if (var833>var545)
			return (var833*var545);
		else
			return (var545*var833+1);
	}
}

class mthdcls350 {
	public static int method350 (int var646, int var131) {
		if (var646>var131)
			return (var646-var131);
		else
			return (var131-var646+1);
	}
}

class mthdcls351 {
	public static int method351 (int var998, int var5) {
		if (var998>var5)
			return (var998+var5);
		else
			return (var5+var998+1);
	}
}

class mthdcls352 {
	public static int method352 (int var18, int var313) {
		if (var18>var313)
			return (var18*var313);
		else
			return (var313*var18+1);
	}
}

class mthdcls353 {
	public static int method353 (int var792, int var147) {
		if (var792>var147)
			return (var792-var147);
		else
			return (var147-var792+1);
	}
}

class mthdcls354 {
	public static int method354 (int var714, int var944) {
		if (var714>var944)
			return (var714-var944);
		else
			return (var944-var714+1);
	}
}

class mthdcls355 {
	public static int method355 (int var331, int var402) {
		if (var331>var402)
			return (var331*var402);
		else
			return (var402*var331+1);
	}
}

class mthdcls356 {
	public static int method356 (int var583, int var292) {
		if (var583>var292)
			return (var583*var292);
		else
			return (var292*var583+1);
	}
}

class mthdcls357 {
	public static int method357 (int var359, int var853) {
		if (var359>var853)
			return (var359*var853);
		else
			return (var853*var359+1);
	}
}

class mthdcls358 {
	public static int method358 (int var722, int var51) {
		if (var722>var51)
			return (var722-var51);
		else
			return (var51-var722+1);
	}
}

class mthdcls359 {
	public static int method359 (int var390, int var410) {
		if (var390>var410)
			return (var390*var410);
		else
			return (var410*var390+1);
	}
}

class mthdcls360 {
	public static int method360 (int var896, int var22) {
		if (var896>var22)
			return (var896-var22);
		else
			return (var22-var896+1);
	}
}

class mthdcls361 {
	public static int method361 (int var947, int var335) {
		if (var947>var335)
			return (var947*var335);
		else
			return (var335*var947+1);
	}
}

class mthdcls362 {
	public static int method362 (int var644, int var275) {
		if (var644>var275)
			return (var644+var275);
		else
			return (var275+var644+1);
	}
}

class mthdcls363 {
	public static int method363 (int var186, int var158) {
		if (var186>var158)
			return (var186*var158);
		else
			return (var158*var186+1);
	}
}

class mthdcls364 {
	public static int method364 (int var408, int var468) {
		if (var408>var468)
			return (var408-var468);
		else
			return (var468-var408+1);
	}
}

class mthdcls365 {
	public static int method365 (int var615, int var387) {
		if (var615>var387)
			return (var615+var387);
		else
			return (var387+var615+1);
	}
}

class mthdcls366 {
	public static int method366 (int var417, int var215) {
		if (var417>var215)
			return (var417*var215);
		else
			return (var215*var417+1);
	}
}

class mthdcls367 {
	public static int method367 (int var200, int var118) {
		if (var200>var118)
			return (var200*var118);
		else
			return (var118*var200+1);
	}
}

class mthdcls368 {
	public static int method368 (int var300, int var215) {
		if (var300>var215)
			return (var300*var215);
		else
			return (var215*var300+1);
	}
}

class mthdcls369 {
	public static int method369 (int var216, int var159) {
		if (var216>var159)
			return (var216-var159);
		else
			return (var159-var216+1);
	}
}

class mthdcls370 {
	public static int method370 (int var883, int var768) {
		if (var883>var768)
			return (var883-var768);
		else
			return (var768-var883+1);
	}
}

class mthdcls371 {
	public static int method371 (int var238, int var800) {
		if (var238>var800)
			return (var238+var800);
		else
			return (var800+var238+1);
	}
}

class mthdcls372 {
	public static int method372 (int var633, int var309) {
		if (var633>var309)
			return (var633*var309);
		else
			return (var309*var633+1);
	}
}

class mthdcls373 {
	public static int method373 (int var210, int var202) {
		if (var210>var202)
			return (var210+var202);
		else
			return (var202+var210+1);
	}
}

class mthdcls374 {
	public static int method374 (int var722, int var305) {
		if (var722>var305)
			return (var722-var305);
		else
			return (var305-var722+1);
	}
}

class mthdcls375 {
	public static int method375 (int var119, int var882) {
		if (var119>var882)
			return (var119+var882);
		else
			return (var882+var119+1);
	}
}

class mthdcls376 {
	public static int method376 (int var391, int var908) {
		if (var391>var908)
			return (var391*var908);
		else
			return (var908*var391+1);
	}
}

class mthdcls377 {
	public static int method377 (int var920, int var306) {
		if (var920>var306)
			return (var920-var306);
		else
			return (var306-var920+1);
	}
}

class mthdcls378 {
	public static int method378 (int var834, int var162) {
		if (var834>var162)
			return (var834+var162);
		else
			return (var162+var834+1);
	}
}

class mthdcls379 {
	public static int method379 (int var177, int var555) {
		if (var177>var555)
			return (var177-var555);
		else
			return (var555-var177+1);
	}
}

class mthdcls380 {
	public static int method380 (int var493, int var741) {
		if (var493>var741)
			return (var493+var741);
		else
			return (var741+var493+1);
	}
}

class mthdcls381 {
	public static int method381 (int var998, int var191) {
		if (var998>var191)
			return (var998*var191);
		else
			return (var191*var998+1);
	}
}

class mthdcls382 {
	public static int method382 (int var824, int var332) {
		if (var824>var332)
			return (var824*var332);
		else
			return (var332*var824+1);
	}
}

class mthdcls383 {
	public static int method383 (int var238, int var357) {
		if (var238>var357)
			return (var238+var357);
		else
			return (var357+var238+1);
	}
}

class mthdcls384 {
	public static int method384 (int var934, int var361) {
		if (var934>var361)
			return (var934-var361);
		else
			return (var361-var934+1);
	}
}

class mthdcls385 {
	public static int method385 (int var623, int var164) {
		if (var623>var164)
			return (var623+var164);
		else
			return (var164+var623+1);
	}
}

class mthdcls386 {
	public static int method386 (int var816, int var283) {
		if (var816>var283)
			return (var816-var283);
		else
			return (var283-var816+1);
	}
}

class mthdcls387 {
	public static int method387 (int var203, int var479) {
		if (var203>var479)
			return (var203+var479);
		else
			return (var479+var203+1);
	}
}

class mthdcls388 {
	public static int method388 (int var481, int var69) {
		if (var481>var69)
			return (var481*var69);
		else
			return (var69*var481+1);
	}
}

class mthdcls389 {
	public static int method389 (int var644, int var630) {
		if (var644>var630)
			return (var644*var630);
		else
			return (var630*var644+1);
	}
}

class mthdcls390 {
	public static int method390 (int var974, int var952) {
		if (var974>var952)
			return (var974*var952);
		else
			return (var952*var974+1);
	}
}

class mthdcls391 {
	public static int method391 (int var682, int var285) {
		if (var682>var285)
			return (var682*var285);
		else
			return (var285*var682+1);
	}
}

class mthdcls392 {
	public static int method392 (int var346, int var617) {
		if (var346>var617)
			return (var346+var617);
		else
			return (var617+var346+1);
	}
}

class mthdcls393 {
	public static int method393 (int var352, int var123) {
		if (var352>var123)
			return (var352+var123);
		else
			return (var123+var352+1);
	}
}

class mthdcls394 {
	public static int method394 (int var259, int var676) {
		if (var259>var676)
			return (var259-var676);
		else
			return (var676-var259+1);
	}
}

class mthdcls395 {
	public static int method395 (int var547, int var445) {
		if (var547>var445)
			return (var547+var445);
		else
			return (var445+var547+1);
	}
}

class mthdcls396 {
	public static int method396 (int var358, int var227) {
		if (var358>var227)
			return (var358*var227);
		else
			return (var227*var358+1);
	}
}

class mthdcls397 {
	public static int method397 (int var674, int var678) {
		if (var674>var678)
			return (var674+var678);
		else
			return (var678+var674+1);
	}
}

class mthdcls398 {
	public static int method398 (int var15, int var913) {
		if (var15>var913)
			return (var15+var913);
		else
			return (var913+var15+1);
	}
}

class mthdcls399 {
	public static int method399 (int var811, int var721) {
		if (var811>var721)
			return (var811*var721);
		else
			return (var721*var811+1);
	}
}

class mthdcls400 {
	public static int method400 (int var24, int var817) {
		if (var24>var817)
			return (var24+var817);
		else
			return (var817+var24+1);
	}
}

class mthdcls401 {
	public static int method401 (int var931, int var312) {
		if (var931>var312)
			return (var931+var312);
		else
			return (var312+var931+1);
	}
}

class mthdcls402 {
	public static int method402 (int var711, int var46) {
		if (var711>var46)
			return (var711-var46);
		else
			return (var46-var711+1);
	}
}

class mthdcls403 {
	public static int method403 (int var955, int var587) {
		if (var955>var587)
			return (var955-var587);
		else
			return (var587-var955+1);
	}
}

class mthdcls404 {
	public static int method404 (int var937, int var308) {
		if (var937>var308)
			return (var937*var308);
		else
			return (var308*var937+1);
	}
}

class mthdcls405 {
	public static int method405 (int var473, int var393) {
		if (var473>var393)
			return (var473-var393);
		else
			return (var393-var473+1);
	}
}

class mthdcls406 {
	public static int method406 (int var593, int var61) {
		if (var593>var61)
			return (var593+var61);
		else
			return (var61+var593+1);
	}
}

class mthdcls407 {
	public static int method407 (int var192, int var513) {
		if (var192>var513)
			return (var192+var513);
		else
			return (var513+var192+1);
	}
}

class mthdcls408 {
	public static int method408 (int var965, int var426) {
		if (var965>var426)
			return (var965+var426);
		else
			return (var426+var965+1);
	}
}

class mthdcls409 {
	public static int method409 (int var489, int var216) {
		if (var489>var216)
			return (var489+var216);
		else
			return (var216+var489+1);
	}
}

class mthdcls410 {
	public static int method410 (int var290, int var932) {
		if (var290>var932)
			return (var290*var932);
		else
			return (var932*var290+1);
	}
}

class mthdcls411 {
	public static int method411 (int var204, int var415) {
		if (var204>var415)
			return (var204-var415);
		else
			return (var415-var204+1);
	}
}

class mthdcls412 {
	public static int method412 (int var320, int var923) {
		if (var320>var923)
			return (var320*var923);
		else
			return (var923*var320+1);
	}
}

class mthdcls413 {
	public static int method413 (int var529, int var710) {
		if (var529>var710)
			return (var529+var710);
		else
			return (var710+var529+1);
	}
}

class mthdcls414 {
	public static int method414 (int var852, int var564) {
		if (var852>var564)
			return (var852+var564);
		else
			return (var564+var852+1);
	}
}

class mthdcls415 {
	public static int method415 (int var823, int var631) {
		if (var823>var631)
			return (var823-var631);
		else
			return (var631-var823+1);
	}
}

class mthdcls416 {
	public static int method416 (int var756, int var483) {
		if (var756>var483)
			return (var756-var483);
		else
			return (var483-var756+1);
	}
}

class mthdcls417 {
	public static int method417 (int var382, int var243) {
		if (var382>var243)
			return (var382*var243);
		else
			return (var243*var382+1);
	}
}

class mthdcls418 {
	public static int method418 (int var988, int var147) {
		if (var988>var147)
			return (var988-var147);
		else
			return (var147-var988+1);
	}
}

class mthdcls419 {
	public static int method419 (int var864, int var850) {
		if (var864>var850)
			return (var864-var850);
		else
			return (var850-var864+1);
	}
}

class mthdcls420 {
	public static int method420 (int var209, int var159) {
		if (var209>var159)
			return (var209-var159);
		else
			return (var159-var209+1);
	}
}

class mthdcls421 {
	public static int method421 (int var689, int var362) {
		if (var689>var362)
			return (var689+var362);
		else
			return (var362+var689+1);
	}
}

class mthdcls422 {
	public static int method422 (int var669, int var442) {
		if (var669>var442)
			return (var669+var442);
		else
			return (var442+var669+1);
	}
}

class mthdcls423 {
	public static int method423 (int var834, int var453) {
		if (var834>var453)
			return (var834*var453);
		else
			return (var453*var834+1);
	}
}

class mthdcls424 {
	public static int method424 (int var944, int var963) {
		if (var944>var963)
			return (var944*var963);
		else
			return (var963*var944+1);
	}
}

class mthdcls425 {
	public static int method425 (int var551, int var738) {
		if (var551>var738)
			return (var551*var738);
		else
			return (var738*var551+1);
	}
}

class mthdcls426 {
	public static int method426 (int var714, int var27) {
		if (var714>var27)
			return (var714+var27);
		else
			return (var27+var714+1);
	}
}

class mthdcls427 {
	public static int method427 (int var20, int var620) {
		if (var20>var620)
			return (var20*var620);
		else
			return (var620*var20+1);
	}
}

class mthdcls428 {
	public static int method428 (int var894, int var492) {
		if (var894>var492)
			return (var894*var492);
		else
			return (var492*var894+1);
	}
}

class mthdcls429 {
	public static int method429 (int var796, int var974) {
		if (var796>var974)
			return (var796+var974);
		else
			return (var974+var796+1);
	}
}

class mthdcls430 {
	public static int method430 (int var538, int var387) {
		if (var538>var387)
			return (var538+var387);
		else
			return (var387+var538+1);
	}
}

class mthdcls431 {
	public static int method431 (int var692, int var496) {
		if (var692>var496)
			return (var692*var496);
		else
			return (var496*var692+1);
	}
}

class mthdcls432 {
	public static int method432 (int var433, int var560) {
		if (var433>var560)
			return (var433-var560);
		else
			return (var560-var433+1);
	}
}

class mthdcls433 {
	public static int method433 (int var830, int var768) {
		if (var830>var768)
			return (var830-var768);
		else
			return (var768-var830+1);
	}
}

class mthdcls434 {
	public static int method434 (int var264, int var62) {
		if (var264>var62)
			return (var264*var62);
		else
			return (var62*var264+1);
	}
}

class mthdcls435 {
	public static int method435 (int var565, int var730) {
		if (var565>var730)
			return (var565+var730);
		else
			return (var730+var565+1);
	}
}

class mthdcls436 {
	public static int method436 (int var903, int var346) {
		if (var903>var346)
			return (var903+var346);
		else
			return (var346+var903+1);
	}
}

class mthdcls437 {
	public static int method437 (int var761, int var801) {
		if (var761>var801)
			return (var761+var801);
		else
			return (var801+var761+1);
	}
}

class mthdcls438 {
	public static int method438 (int var530, int var753) {
		if (var530>var753)
			return (var530-var753);
		else
			return (var753-var530+1);
	}
}

class mthdcls439 {
	public static int method439 (int var908, int var486) {
		if (var908>var486)
			return (var908*var486);
		else
			return (var486*var908+1);
	}
}

class mthdcls440 {
	public static int method440 (int var762, int var702) {
		if (var762>var702)
			return (var762+var702);
		else
			return (var702+var762+1);
	}
}

class mthdcls441 {
	public static int method441 (int var54, int var454) {
		if (var54>var454)
			return (var54*var454);
		else
			return (var454*var54+1);
	}
}

class mthdcls442 {
	public static int method442 (int var588, int var766) {
		if (var588>var766)
			return (var588+var766);
		else
			return (var766+var588+1);
	}
}

class mthdcls443 {
	public static int method443 (int var213, int var77) {
		if (var213>var77)
			return (var213+var77);
		else
			return (var77+var213+1);
	}
}

class mthdcls444 {
	public static int method444 (int var857, int var986) {
		if (var857>var986)
			return (var857-var986);
		else
			return (var986-var857+1);
	}
}

class mthdcls445 {
	public static int method445 (int var587, int var552) {
		if (var587>var552)
			return (var587*var552);
		else
			return (var552*var587+1);
	}
}

class mthdcls446 {
	public static int method446 (int var797, int var685) {
		if (var797>var685)
			return (var797+var685);
		else
			return (var685+var797+1);
	}
}

class mthdcls447 {
	public static int method447 (int var714, int var63) {
		if (var714>var63)
			return (var714+var63);
		else
			return (var63+var714+1);
	}
}

class mthdcls448 {
	public static int method448 (int var473, int var466) {
		if (var473>var466)
			return (var473-var466);
		else
			return (var466-var473+1);
	}
}

class mthdcls449 {
	public static int method449 (int var517, int var831) {
		if (var517>var831)
			return (var517*var831);
		else
			return (var831*var517+1);
	}
}

class mthdcls450 {
	public static int method450 (int var488, int var707) {
		if (var488>var707)
			return (var488+var707);
		else
			return (var707+var488+1);
	}
}

class mthdcls451 {
	public static int method451 (int var213, int var405) {
		if (var213>var405)
			return (var213*var405);
		else
			return (var405*var213+1);
	}
}

class mthdcls452 {
	public static int method452 (int var956, int var979) {
		if (var956>var979)
			return (var956*var979);
		else
			return (var979*var956+1);
	}
}

class mthdcls453 {
	public static int method453 (int var772, int var35) {
		if (var772>var35)
			return (var772-var35);
		else
			return (var35-var772+1);
	}
}

class mthdcls454 {
	public static int method454 (int var228, int var484) {
		if (var228>var484)
			return (var228*var484);
		else
			return (var484*var228+1);
	}
}

class mthdcls455 {
	public static int method455 (int var936, int var324) {
		if (var936>var324)
			return (var936+var324);
		else
			return (var324+var936+1);
	}
}

class mthdcls456 {
	public static int method456 (int var277, int var987) {
		if (var277>var987)
			return (var277-var987);
		else
			return (var987-var277+1);
	}
}

class mthdcls457 {
	public static int method457 (int var863, int var395) {
		if (var863>var395)
			return (var863-var395);
		else
			return (var395-var863+1);
	}
}

class mthdcls458 {
	public static int method458 (int var30, int var686) {
		if (var30>var686)
			return (var30+var686);
		else
			return (var686+var30+1);
	}
}

class mthdcls459 {
	public static int method459 (int var780, int var236) {
		if (var780>var236)
			return (var780*var236);
		else
			return (var236*var780+1);
	}
}

class mthdcls460 {
	public static int method460 (int var456, int var245) {
		if (var456>var245)
			return (var456+var245);
		else
			return (var245+var456+1);
	}
}

class mthdcls461 {
	public static int method461 (int var126, int var664) {
		if (var126>var664)
			return (var126-var664);
		else
			return (var664-var126+1);
	}
}

class mthdcls462 {
	public static int method462 (int var188, int var619) {
		if (var188>var619)
			return (var188+var619);
		else
			return (var619+var188+1);
	}
}

class mthdcls463 {
	public static int method463 (int var967, int var712) {
		if (var967>var712)
			return (var967*var712);
		else
			return (var712*var967+1);
	}
}

class mthdcls464 {
	public static int method464 (int var676, int var150) {
		if (var676>var150)
			return (var676*var150);
		else
			return (var150*var676+1);
	}
}

class mthdcls465 {
	public static int method465 (int var625, int var64) {
		if (var625>var64)
			return (var625-var64);
		else
			return (var64-var625+1);
	}
}

class mthdcls466 {
	public static int method466 (int var522, int var896) {
		if (var522>var896)
			return (var522+var896);
		else
			return (var896+var522+1);
	}
}

class mthdcls467 {
	public static int method467 (int var908, int var914) {
		if (var908>var914)
			return (var908*var914);
		else
			return (var914*var908+1);
	}
}

class mthdcls468 {
	public static int method468 (int var295, int var492) {
		if (var295>var492)
			return (var295+var492);
		else
			return (var492+var295+1);
	}
}

class mthdcls469 {
	public static int method469 (int var356, int var164) {
		if (var356>var164)
			return (var356-var164);
		else
			return (var164-var356+1);
	}
}

class mthdcls470 {
	public static int method470 (int var310, int var304) {
		if (var310>var304)
			return (var310*var304);
		else
			return (var304*var310+1);
	}
}

class mthdcls471 {
	public static int method471 (int var770, int var600) {
		if (var770>var600)
			return (var770+var600);
		else
			return (var600+var770+1);
	}
}

class mthdcls472 {
	public static int method472 (int var164, int var782) {
		if (var164>var782)
			return (var164+var782);
		else
			return (var782+var164+1);
	}
}

class mthdcls473 {
	public static int method473 (int var328, int var369) {
		if (var328>var369)
			return (var328*var369);
		else
			return (var369*var328+1);
	}
}

class mthdcls474 {
	public static int method474 (int var949, int var506) {
		if (var949>var506)
			return (var949+var506);
		else
			return (var506+var949+1);
	}
}

class mthdcls475 {
	public static int method475 (int var307, int var237) {
		if (var307>var237)
			return (var307-var237);
		else
			return (var237-var307+1);
	}
}

class mthdcls476 {
	public static int method476 (int var309, int var513) {
		if (var309>var513)
			return (var309*var513);
		else
			return (var513*var309+1);
	}
}

class mthdcls477 {
	public static int method477 (int var942, int var672) {
		if (var942>var672)
			return (var942-var672);
		else
			return (var672-var942+1);
	}
}

class mthdcls478 {
	public static int method478 (int var986, int var478) {
		if (var986>var478)
			return (var986-var478);
		else
			return (var478-var986+1);
	}
}

class mthdcls479 {
	public static int method479 (int var735, int var13) {
		if (var735>var13)
			return (var735-var13);
		else
			return (var13-var735+1);
	}
}

class mthdcls480 {
	public static int method480 (int var947, int var292) {
		if (var947>var292)
			return (var947+var292);
		else
			return (var292+var947+1);
	}
}

class mthdcls481 {
	public static int method481 (int var607, int var843) {
		if (var607>var843)
			return (var607+var843);
		else
			return (var843+var607+1);
	}
}

class mthdcls482 {
	public static int method482 (int var183, int var450) {
		if (var183>var450)
			return (var183-var450);
		else
			return (var450-var183+1);
	}
}

class mthdcls483 {
	public static int method483 (int var222, int var705) {
		if (var222>var705)
			return (var222-var705);
		else
			return (var705-var222+1);
	}
}

class mthdcls484 {
	public static int method484 (int var317, int var532) {
		if (var317>var532)
			return (var317*var532);
		else
			return (var532*var317+1);
	}
}

class mthdcls485 {
	public static int method485 (int var864, int var794) {
		if (var864>var794)
			return (var864+var794);
		else
			return (var794+var864+1);
	}
}

class mthdcls486 {
	public static int method486 (int var835, int var698) {
		if (var835>var698)
			return (var835*var698);
		else
			return (var698*var835+1);
	}
}

class mthdcls487 {
	public static int method487 (int var762, int var390) {
		if (var762>var390)
			return (var762*var390);
		else
			return (var390*var762+1);
	}
}

class mthdcls488 {
	public static int method488 (int var322, int var518) {
		if (var322>var518)
			return (var322+var518);
		else
			return (var518+var322+1);
	}
}

class mthdcls489 {
	public static int method489 (int var15, int var739) {
		if (var15>var739)
			return (var15+var739);
		else
			return (var739+var15+1);
	}
}

class mthdcls490 {
	public static int method490 (int var196, int var701) {
		if (var196>var701)
			return (var196-var701);
		else
			return (var701-var196+1);
	}
}

class mthdcls491 {
	public static int method491 (int var807, int var695) {
		if (var807>var695)
			return (var807*var695);
		else
			return (var695*var807+1);
	}
}

class mthdcls492 {
	public static int method492 (int var643, int var733) {
		if (var643>var733)
			return (var643-var733);
		else
			return (var733-var643+1);
	}
}

class mthdcls493 {
	public static int method493 (int var251, int var18) {
		if (var251>var18)
			return (var251-var18);
		else
			return (var18-var251+1);
	}
}

class mthdcls494 {
	public static int method494 (int var969, int var353) {
		if (var969>var353)
			return (var969-var353);
		else
			return (var353-var969+1);
	}
}

class mthdcls495 {
	public static int method495 (int var827, int var220) {
		if (var827>var220)
			return (var827-var220);
		else
			return (var220-var827+1);
	}
}

class mthdcls496 {
	public static int method496 (int var253, int var756) {
		if (var253>var756)
			return (var253*var756);
		else
			return (var756*var253+1);
	}
}

class mthdcls497 {
	public static int method497 (int var897, int var793) {
		if (var897>var793)
			return (var897+var793);
		else
			return (var793+var897+1);
	}
}

class mthdcls498 {
	public static int method498 (int var889, int var273) {
		if (var889>var273)
			return (var889-var273);
		else
			return (var273-var889+1);
	}
}

class mthdcls499 {
	public static int method499 (int var543, int var961) {
		if (var543>var961)
			return (var543*var961);
		else
			return (var961*var543+1);
	}
}

class mthdcls500 {
	public static int method500 (int var729, int var715) {
		if (var729>var715)
			return (var729-var715);
		else
			return (var715-var729+1);
	}
}

class mthdcls501 {
	public static int method501 (int var562, int var869) {
		if (var562>var869)
			return (var562-var869);
		else
			return (var869-var562+1);
	}
}

class mthdcls502 {
	public static int method502 (int var264, int var221) {
		if (var264>var221)
			return (var264*var221);
		else
			return (var221*var264+1);
	}
}

class mthdcls503 {
	public static int method503 (int var842, int var927) {
		if (var842>var927)
			return (var842-var927);
		else
			return (var927-var842+1);
	}
}

class mthdcls504 {
	public static int method504 (int var812, int var247) {
		if (var812>var247)
			return (var812-var247);
		else
			return (var247-var812+1);
	}
}

class mthdcls505 {
	public static int method505 (int var152, int var346) {
		if (var152>var346)
			return (var152+var346);
		else
			return (var346+var152+1);
	}
}

class mthdcls506 {
	public static int method506 (int var4, int var900) {
		if (var4>var900)
			return (var4-var900);
		else
			return (var900-var4+1);
	}
}

class mthdcls507 {
	public static int method507 (int var455, int var511) {
		if (var455>var511)
			return (var455-var511);
		else
			return (var511-var455+1);
	}
}

class mthdcls508 {
	public static int method508 (int var88, int var274) {
		if (var88>var274)
			return (var88+var274);
		else
			return (var274+var88+1);
	}
}

class mthdcls509 {
	public static int method509 (int var378, int var438) {
		if (var378>var438)
			return (var378*var438);
		else
			return (var438*var378+1);
	}
}

class mthdcls510 {
	public static int method510 (int var943, int var60) {
		if (var943>var60)
			return (var943+var60);
		else
			return (var60+var943+1);
	}
}

class mthdcls511 {
	public static int method511 (int var640, int var368) {
		if (var640>var368)
			return (var640*var368);
		else
			return (var368*var640+1);
	}
}

class mthdcls512 {
	public static int method512 (int var269, int var567) {
		if (var269>var567)
			return (var269-var567);
		else
			return (var567-var269+1);
	}
}

class mthdcls513 {
	public static int method513 (int var506, int var798) {
		if (var506>var798)
			return (var506*var798);
		else
			return (var798*var506+1);
	}
}

class mthdcls514 {
	public static int method514 (int var626, int var83) {
		if (var626>var83)
			return (var626+var83);
		else
			return (var83+var626+1);
	}
}

class mthdcls515 {
	public static int method515 (int var491, int var59) {
		if (var491>var59)
			return (var491*var59);
		else
			return (var59*var491+1);
	}
}

class mthdcls516 {
	public static int method516 (int var277, int var110) {
		if (var277>var110)
			return (var277+var110);
		else
			return (var110+var277+1);
	}
}

class mthdcls517 {
	public static int method517 (int var286, int var202) {
		if (var286>var202)
			return (var286*var202);
		else
			return (var202*var286+1);
	}
}

class mthdcls518 {
	public static int method518 (int var977, int var821) {
		if (var977>var821)
			return (var977-var821);
		else
			return (var821-var977+1);
	}
}

class mthdcls519 {
	public static int method519 (int var839, int var185) {
		if (var839>var185)
			return (var839+var185);
		else
			return (var185+var839+1);
	}
}

class mthdcls520 {
	public static int method520 (int var274, int var438) {
		if (var274>var438)
			return (var274+var438);
		else
			return (var438+var274+1);
	}
}

class mthdcls521 {
	public static int method521 (int var20, int var526) {
		if (var20>var526)
			return (var20-var526);
		else
			return (var526-var20+1);
	}
}

class mthdcls522 {
	public static int method522 (int var997, int var845) {
		if (var997>var845)
			return (var997-var845);
		else
			return (var845-var997+1);
	}
}

class mthdcls523 {
	public static int method523 (int var151, int var123) {
		if (var151>var123)
			return (var151+var123);
		else
			return (var123+var151+1);
	}
}

class mthdcls524 {
	public static int method524 (int var82, int var153) {
		if (var82>var153)
			return (var82+var153);
		else
			return (var153+var82+1);
	}
}

class mthdcls525 {
	public static int method525 (int var529, int var853) {
		if (var529>var853)
			return (var529*var853);
		else
			return (var853*var529+1);
	}
}

class mthdcls526 {
	public static int method526 (int var292, int var22) {
		if (var292>var22)
			return (var292+var22);
		else
			return (var22+var292+1);
	}
}

class mthdcls527 {
	public static int method527 (int var778, int var639) {
		if (var778>var639)
			return (var778+var639);
		else
			return (var639+var778+1);
	}
}

class mthdcls528 {
	public static int method528 (int var86, int var831) {
		if (var86>var831)
			return (var86+var831);
		else
			return (var831+var86+1);
	}
}

class mthdcls529 {
	public static int method529 (int var907, int var976) {
		if (var907>var976)
			return (var907*var976);
		else
			return (var976*var907+1);
	}
}

class mthdcls530 {
	public static int method530 (int var28, int var521) {
		if (var28>var521)
			return (var28*var521);
		else
			return (var521*var28+1);
	}
}

class mthdcls531 {
	public static int method531 (int var673, int var280) {
		if (var673>var280)
			return (var673-var280);
		else
			return (var280-var673+1);
	}
}

class mthdcls532 {
	public static int method532 (int var528, int var340) {
		if (var528>var340)
			return (var528*var340);
		else
			return (var340*var528+1);
	}
}

class mthdcls533 {
	public static int method533 (int var406, int var85) {
		if (var406>var85)
			return (var406*var85);
		else
			return (var85*var406+1);
	}
}

class mthdcls534 {
	public static int method534 (int var352, int var107) {
		if (var352>var107)
			return (var352*var107);
		else
			return (var107*var352+1);
	}
}

class mthdcls535 {
	public static int method535 (int var618, int var467) {
		if (var618>var467)
			return (var618-var467);
		else
			return (var467-var618+1);
	}
}

class mthdcls536 {
	public static int method536 (int var817, int var980) {
		if (var817>var980)
			return (var817-var980);
		else
			return (var980-var817+1);
	}
}

class mthdcls537 {
	public static int method537 (int var450, int var664) {
		if (var450>var664)
			return (var450-var664);
		else
			return (var664-var450+1);
	}
}

class mthdcls538 {
	public static int method538 (int var624, int var371) {
		if (var624>var371)
			return (var624*var371);
		else
			return (var371*var624+1);
	}
}

class mthdcls539 {
	public static int method539 (int var447, int var186) {
		if (var447>var186)
			return (var447-var186);
		else
			return (var186-var447+1);
	}
}

class mthdcls540 {
	public static int method540 (int var670, int var878) {
		if (var670>var878)
			return (var670*var878);
		else
			return (var878*var670+1);
	}
}

class mthdcls541 {
	public static int method541 (int var21, int var273) {
		if (var21>var273)
			return (var21*var273);
		else
			return (var273*var21+1);
	}
}

class mthdcls542 {
	public static int method542 (int var14, int var903) {
		if (var14>var903)
			return (var14*var903);
		else
			return (var903*var14+1);
	}
}

class mthdcls543 {
	public static int method543 (int var525, int var574) {
		if (var525>var574)
			return (var525-var574);
		else
			return (var574-var525+1);
	}
}

class mthdcls544 {
	public static int method544 (int var896, int var685) {
		if (var896>var685)
			return (var896+var685);
		else
			return (var685+var896+1);
	}
}

class mthdcls545 {
	public static int method545 (int var833, int var356) {
		if (var833>var356)
			return (var833*var356);
		else
			return (var356*var833+1);
	}
}

class mthdcls546 {
	public static int method546 (int var669, int var361) {
		if (var669>var361)
			return (var669*var361);
		else
			return (var361*var669+1);
	}
}

class mthdcls547 {
	public static int method547 (int var64, int var803) {
		if (var64>var803)
			return (var64-var803);
		else
			return (var803-var64+1);
	}
}

class mthdcls548 {
	public static int method548 (int var93, int var282) {
		if (var93>var282)
			return (var93+var282);
		else
			return (var282+var93+1);
	}
}

class mthdcls549 {
	public static int method549 (int var941, int var90) {
		if (var941>var90)
			return (var941*var90);
		else
			return (var90*var941+1);
	}
}

class mthdcls550 {
	public static int method550 (int var84, int var868) {
		if (var84>var868)
			return (var84-var868);
		else
			return (var868-var84+1);
	}
}

class mthdcls551 {
	public static int method551 (int var830, int var230) {
		if (var830>var230)
			return (var830-var230);
		else
			return (var230-var830+1);
	}
}

class mthdcls552 {
	public static int method552 (int var701, int var813) {
		if (var701>var813)
			return (var701+var813);
		else
			return (var813+var701+1);
	}
}

class mthdcls553 {
	public static int method553 (int var121, int var570) {
		if (var121>var570)
			return (var121*var570);
		else
			return (var570*var121+1);
	}
}

class mthdcls554 {
	public static int method554 (int var785, int var969) {
		if (var785>var969)
			return (var785+var969);
		else
			return (var969+var785+1);
	}
}

class mthdcls555 {
	public static int method555 (int var331, int var966) {
		if (var331>var966)
			return (var331+var966);
		else
			return (var966+var331+1);
	}
}

class mthdcls556 {
	public static int method556 (int var819, int var459) {
		if (var819>var459)
			return (var819*var459);
		else
			return (var459*var819+1);
	}
}

class mthdcls557 {
	public static int method557 (int var743, int var330) {
		if (var743>var330)
			return (var743*var330);
		else
			return (var330*var743+1);
	}
}

class mthdcls558 {
	public static int method558 (int var983, int var884) {
		if (var983>var884)
			return (var983-var884);
		else
			return (var884-var983+1);
	}
}

class mthdcls559 {
	public static int method559 (int var234, int var582) {
		if (var234>var582)
			return (var234-var582);
		else
			return (var582-var234+1);
	}
}

class mthdcls560 {
	public static int method560 (int var576, int var182) {
		if (var576>var182)
			return (var576+var182);
		else
			return (var182+var576+1);
	}
}

class mthdcls561 {
	public static int method561 (int var471, int var529) {
		if (var471>var529)
			return (var471*var529);
		else
			return (var529*var471+1);
	}
}

class mthdcls562 {
	public static int method562 (int var463, int var351) {
		if (var463>var351)
			return (var463+var351);
		else
			return (var351+var463+1);
	}
}

class mthdcls563 {
	public static int method563 (int var346, int var957) {
		if (var346>var957)
			return (var346*var957);
		else
			return (var957*var346+1);
	}
}

class mthdcls564 {
	public static int method564 (int var885, int var698) {
		if (var885>var698)
			return (var885*var698);
		else
			return (var698*var885+1);
	}
}

class mthdcls565 {
	public static int method565 (int var995, int var820) {
		if (var995>var820)
			return (var995*var820);
		else
			return (var820*var995+1);
	}
}

class mthdcls566 {
	public static int method566 (int var236, int var793) {
		if (var236>var793)
			return (var236+var793);
		else
			return (var793+var236+1);
	}
}

class mthdcls567 {
	public static int method567 (int var98, int var715) {
		if (var98>var715)
			return (var98-var715);
		else
			return (var715-var98+1);
	}
}

class mthdcls568 {
	public static int method568 (int var506, int var477) {
		if (var506>var477)
			return (var506*var477);
		else
			return (var477*var506+1);
	}
}

class mthdcls569 {
	public static int method569 (int var619, int var991) {
		if (var619>var991)
			return (var619-var991);
		else
			return (var991-var619+1);
	}
}

class mthdcls570 {
	public static int method570 (int var574, int var345) {
		if (var574>var345)
			return (var574*var345);
		else
			return (var345*var574+1);
	}
}

class mthdcls571 {
	public static int method571 (int var334, int var248) {
		if (var334>var248)
			return (var334+var248);
		else
			return (var248+var334+1);
	}
}

class mthdcls572 {
	public static int method572 (int var647, int var778) {
		if (var647>var778)
			return (var647-var778);
		else
			return (var778-var647+1);
	}
}

class mthdcls573 {
	public static int method573 (int var484, int var827) {
		if (var484>var827)
			return (var484+var827);
		else
			return (var827+var484+1);
	}
}

class mthdcls574 {
	public static int method574 (int var676, int var461) {
		if (var676>var461)
			return (var676+var461);
		else
			return (var461+var676+1);
	}
}

class mthdcls575 {
	public static int method575 (int var910, int var173) {
		if (var910>var173)
			return (var910*var173);
		else
			return (var173*var910+1);
	}
}

class mthdcls576 {
	public static int method576 (int var777, int var599) {
		if (var777>var599)
			return (var777*var599);
		else
			return (var599*var777+1);
	}
}

class mthdcls577 {
	public static int method577 (int var623, int var28) {
		if (var623>var28)
			return (var623*var28);
		else
			return (var28*var623+1);
	}
}

class mthdcls578 {
	public static int method578 (int var51, int var554) {
		if (var51>var554)
			return (var51*var554);
		else
			return (var554*var51+1);
	}
}

class mthdcls579 {
	public static int method579 (int var336, int var465) {
		if (var336>var465)
			return (var336+var465);
		else
			return (var465+var336+1);
	}
}

class mthdcls580 {
	public static int method580 (int var117, int var280) {
		if (var117>var280)
			return (var117*var280);
		else
			return (var280*var117+1);
	}
}

class mthdcls581 {
	public static int method581 (int var805, int var216) {
		if (var805>var216)
			return (var805*var216);
		else
			return (var216*var805+1);
	}
}

class mthdcls582 {
	public static int method582 (int var699, int var485) {
		if (var699>var485)
			return (var699+var485);
		else
			return (var485+var699+1);
	}
}

class mthdcls583 {
	public static int method583 (int var368, int var364) {
		if (var368>var364)
			return (var368-var364);
		else
			return (var364-var368+1);
	}
}

class mthdcls584 {
	public static int method584 (int var294, int var735) {
		if (var294>var735)
			return (var294+var735);
		else
			return (var735+var294+1);
	}
}

class mthdcls585 {
	public static int method585 (int var912, int var300) {
		if (var912>var300)
			return (var912-var300);
		else
			return (var300-var912+1);
	}
}

class mthdcls586 {
	public static int method586 (int var661, int var130) {
		if (var661>var130)
			return (var661*var130);
		else
			return (var130*var661+1);
	}
}

class mthdcls587 {
	public static int method587 (int var21, int var506) {
		if (var21>var506)
			return (var21+var506);
		else
			return (var506+var21+1);
	}
}

class mthdcls588 {
	public static int method588 (int var761, int var462) {
		if (var761>var462)
			return (var761-var462);
		else
			return (var462-var761+1);
	}
}

class mthdcls589 {
	public static int method589 (int var805, int var668) {
		if (var805>var668)
			return (var805*var668);
		else
			return (var668*var805+1);
	}
}

class mthdcls590 {
	public static int method590 (int var305, int var294) {
		if (var305>var294)
			return (var305-var294);
		else
			return (var294-var305+1);
	}
}

class mthdcls591 {
	public static int method591 (int var115, int var957) {
		if (var115>var957)
			return (var115+var957);
		else
			return (var957+var115+1);
	}
}

class mthdcls592 {
	public static int method592 (int var892, int var135) {
		if (var892>var135)
			return (var892+var135);
		else
			return (var135+var892+1);
	}
}

class mthdcls593 {
	public static int method593 (int var444, int var357) {
		if (var444>var357)
			return (var444*var357);
		else
			return (var357*var444+1);
	}
}

class mthdcls594 {
	public static int method594 (int var519, int var677) {
		if (var519>var677)
			return (var519*var677);
		else
			return (var677*var519+1);
	}
}

class mthdcls595 {
	public static int method595 (int var411, int var659) {
		if (var411>var659)
			return (var411+var659);
		else
			return (var659+var411+1);
	}
}

class mthdcls596 {
	public static int method596 (int var179, int var687) {
		if (var179>var687)
			return (var179+var687);
		else
			return (var687+var179+1);
	}
}

class mthdcls597 {
	public static int method597 (int var248, int var579) {
		if (var248>var579)
			return (var248*var579);
		else
			return (var579*var248+1);
	}
}

class mthdcls598 {
	public static int method598 (int var842, int var665) {
		if (var842>var665)
			return (var842-var665);
		else
			return (var665-var842+1);
	}
}

class mthdcls599 {
	public static int method599 (int var856, int var173) {
		if (var856>var173)
			return (var856-var173);
		else
			return (var173-var856+1);
	}
}

class mthdcls600 {
	public static int method600 (int var611, int var329) {
		if (var611>var329)
			return (var611+var329);
		else
			return (var329+var611+1);
	}
}

class mthdcls601 {
	public static int method601 (int var95, int var227) {
		if (var95>var227)
			return (var95*var227);
		else
			return (var227*var95+1);
	}
}

class mthdcls602 {
	public static int method602 (int var283, int var790) {
		if (var283>var790)
			return (var283+var790);
		else
			return (var790+var283+1);
	}
}

class mthdcls603 {
	public static int method603 (int var420, int var552) {
		if (var420>var552)
			return (var420-var552);
		else
			return (var552-var420+1);
	}
}

class mthdcls604 {
	public static int method604 (int var451, int var815) {
		if (var451>var815)
			return (var451-var815);
		else
			return (var815-var451+1);
	}
}

class mthdcls605 {
	public static int method605 (int var101, int var784) {
		if (var101>var784)
			return (var101+var784);
		else
			return (var784+var101+1);
	}
}

class mthdcls606 {
	public static int method606 (int var524, int var514) {
		if (var524>var514)
			return (var524+var514);
		else
			return (var514+var524+1);
	}
}

class mthdcls607 {
	public static int method607 (int var262, int var905) {
		if (var262>var905)
			return (var262*var905);
		else
			return (var905*var262+1);
	}
}

class mthdcls608 {
	public static int method608 (int var393, int var368) {
		if (var393>var368)
			return (var393-var368);
		else
			return (var368-var393+1);
	}
}

class mthdcls609 {
	public static int method609 (int var239, int var153) {
		if (var239>var153)
			return (var239-var153);
		else
			return (var153-var239+1);
	}
}

class mthdcls610 {
	public static int method610 (int var235, int var42) {
		if (var235>var42)
			return (var235+var42);
		else
			return (var42+var235+1);
	}
}

class mthdcls611 {
	public static int method611 (int var478, int var341) {
		if (var478>var341)
			return (var478+var341);
		else
			return (var341+var478+1);
	}
}

class mthdcls612 {
	public static int method612 (int var141, int var126) {
		if (var141>var126)
			return (var141-var126);
		else
			return (var126-var141+1);
	}
}

class mthdcls613 {
	public static int method613 (int var911, int var586) {
		if (var911>var586)
			return (var911*var586);
		else
			return (var586*var911+1);
	}
}

class mthdcls614 {
	public static int method614 (int var935, int var317) {
		if (var935>var317)
			return (var935*var317);
		else
			return (var317*var935+1);
	}
}

class mthdcls615 {
	public static int method615 (int var988, int var979) {
		if (var988>var979)
			return (var988*var979);
		else
			return (var979*var988+1);
	}
}

class mthdcls616 {
	public static int method616 (int var555, int var670) {
		if (var555>var670)
			return (var555+var670);
		else
			return (var670+var555+1);
	}
}

class mthdcls617 {
	public static int method617 (int var74, int var225) {
		if (var74>var225)
			return (var74-var225);
		else
			return (var225-var74+1);
	}
}

class mthdcls618 {
	public static int method618 (int var673, int var370) {
		if (var673>var370)
			return (var673*var370);
		else
			return (var370*var673+1);
	}
}

class mthdcls619 {
	public static int method619 (int var873, int var346) {
		if (var873>var346)
			return (var873*var346);
		else
			return (var346*var873+1);
	}
}

class mthdcls620 {
	public static int method620 (int var579, int var486) {
		if (var579>var486)
			return (var579-var486);
		else
			return (var486-var579+1);
	}
}

class mthdcls621 {
	public static int method621 (int var917, int var165) {
		if (var917>var165)
			return (var917-var165);
		else
			return (var165-var917+1);
	}
}

class mthdcls622 {
	public static int method622 (int var813, int var849) {
		if (var813>var849)
			return (var813+var849);
		else
			return (var849+var813+1);
	}
}

class mthdcls623 {
	public static int method623 (int var79, int var215) {
		if (var79>var215)
			return (var79*var215);
		else
			return (var215*var79+1);
	}
}

class mthdcls624 {
	public static int method624 (int var368, int var362) {
		if (var368>var362)
			return (var368-var362);
		else
			return (var362-var368+1);
	}
}

class mthdcls625 {
	public static int method625 (int var467, int var588) {
		if (var467>var588)
			return (var467*var588);
		else
			return (var588*var467+1);
	}
}

class mthdcls626 {
	public static int method626 (int var547, int var130) {
		if (var547>var130)
			return (var547+var130);
		else
			return (var130+var547+1);
	}
}

class mthdcls627 {
	public static int method627 (int var669, int var71) {
		if (var669>var71)
			return (var669-var71);
		else
			return (var71-var669+1);
	}
}

class mthdcls628 {
	public static int method628 (int var503, int var418) {
		if (var503>var418)
			return (var503*var418);
		else
			return (var418*var503+1);
	}
}

class mthdcls629 {
	public static int method629 (int var900, int var97) {
		if (var900>var97)
			return (var900*var97);
		else
			return (var97*var900+1);
	}
}

class mthdcls630 {
	public static int method630 (int var301, int var478) {
		if (var301>var478)
			return (var301-var478);
		else
			return (var478-var301+1);
	}
}

class mthdcls631 {
	public static int method631 (int var757, int var914) {
		if (var757>var914)
			return (var757*var914);
		else
			return (var914*var757+1);
	}
}

class mthdcls632 {
	public static int method632 (int var822, int var944) {
		if (var822>var944)
			return (var822*var944);
		else
			return (var944*var822+1);
	}
}

class mthdcls633 {
	public static int method633 (int var677, int var201) {
		if (var677>var201)
			return (var677*var201);
		else
			return (var201*var677+1);
	}
}

class mthdcls634 {
	public static int method634 (int var983, int var970) {
		if (var983>var970)
			return (var983+var970);
		else
			return (var970+var983+1);
	}
}

class mthdcls635 {
	public static int method635 (int var265, int var354) {
		if (var265>var354)
			return (var265-var354);
		else
			return (var354-var265+1);
	}
}

class mthdcls636 {
	public static int method636 (int var547, int var515) {
		if (var547>var515)
			return (var547+var515);
		else
			return (var515+var547+1);
	}
}

class mthdcls637 {
	public static int method637 (int var457, int var325) {
		if (var457>var325)
			return (var457*var325);
		else
			return (var325*var457+1);
	}
}

class mthdcls638 {
	public static int method638 (int var540, int var533) {
		if (var540>var533)
			return (var540-var533);
		else
			return (var533-var540+1);
	}
}

class mthdcls639 {
	public static int method639 (int var910, int var857) {
		if (var910>var857)
			return (var910*var857);
		else
			return (var857*var910+1);
	}
}

class mthdcls640 {
	public static int method640 (int var147, int var840) {
		if (var147>var840)
			return (var147*var840);
		else
			return (var840*var147+1);
	}
}

class mthdcls641 {
	public static int method641 (int var805, int var37) {
		if (var805>var37)
			return (var805-var37);
		else
			return (var37-var805+1);
	}
}

class mthdcls642 {
	public static int method642 (int var870, int var748) {
		if (var870>var748)
			return (var870-var748);
		else
			return (var748-var870+1);
	}
}

class mthdcls643 {
	public static int method643 (int var640, int var975) {
		if (var640>var975)
			return (var640-var975);
		else
			return (var975-var640+1);
	}
}

class mthdcls644 {
	public static int method644 (int var938, int var853) {
		if (var938>var853)
			return (var938*var853);
		else
			return (var853*var938+1);
	}
}

class mthdcls645 {
	public static int method645 (int var3, int var201) {
		if (var3>var201)
			return (var3*var201);
		else
			return (var201*var3+1);
	}
}

class mthdcls646 {
	public static int method646 (int var797, int var249) {
		if (var797>var249)
			return (var797+var249);
		else
			return (var249+var797+1);
	}
}

class mthdcls647 {
	public static int method647 (int var395, int var247) {
		if (var395>var247)
			return (var395+var247);
		else
			return (var247+var395+1);
	}
}

class mthdcls648 {
	public static int method648 (int var100, int var991) {
		if (var100>var991)
			return (var100+var991);
		else
			return (var991+var100+1);
	}
}

class mthdcls649 {
	public static int method649 (int var714, int var575) {
		if (var714>var575)
			return (var714+var575);
		else
			return (var575+var714+1);
	}
}

class mthdcls650 {
	public static int method650 (int var577, int var132) {
		if (var577>var132)
			return (var577-var132);
		else
			return (var132-var577+1);
	}
}

class mthdcls651 {
	public static int method651 (int var321, int var855) {
		if (var321>var855)
			return (var321*var855);
		else
			return (var855*var321+1);
	}
}

class mthdcls652 {
	public static int method652 (int var735, int var238) {
		if (var735>var238)
			return (var735+var238);
		else
			return (var238+var735+1);
	}
}

class mthdcls653 {
	public static int method653 (int var461, int var929) {
		if (var461>var929)
			return (var461*var929);
		else
			return (var929*var461+1);
	}
}

class mthdcls654 {
	public static int method654 (int var740, int var177) {
		if (var740>var177)
			return (var740+var177);
		else
			return (var177+var740+1);
	}
}

class mthdcls655 {
	public static int method655 (int var323, int var955) {
		if (var323>var955)
			return (var323*var955);
		else
			return (var955*var323+1);
	}
}

class mthdcls656 {
	public static int method656 (int var170, int var495) {
		if (var170>var495)
			return (var170+var495);
		else
			return (var495+var170+1);
	}
}

class mthdcls657 {
	public static int method657 (int var653, int var525) {
		if (var653>var525)
			return (var653*var525);
		else
			return (var525*var653+1);
	}
}

class mthdcls658 {
	public static int method658 (int var117, int var770) {
		if (var117>var770)
			return (var117-var770);
		else
			return (var770-var117+1);
	}
}

class mthdcls659 {
	public static int method659 (int var739, int var105) {
		if (var739>var105)
			return (var739+var105);
		else
			return (var105+var739+1);
	}
}

class mthdcls660 {
	public static int method660 (int var508, int var620) {
		if (var508>var620)
			return (var508-var620);
		else
			return (var620-var508+1);
	}
}

class mthdcls661 {
	public static int method661 (int var924, int var10) {
		if (var924>var10)
			return (var924+var10);
		else
			return (var10+var924+1);
	}
}

class mthdcls662 {
	public static int method662 (int var820, int var178) {
		if (var820>var178)
			return (var820*var178);
		else
			return (var178*var820+1);
	}
}

class mthdcls663 {
	public static int method663 (int var922, int var469) {
		if (var922>var469)
			return (var922-var469);
		else
			return (var469-var922+1);
	}
}

class mthdcls664 {
	public static int method664 (int var14, int var802) {
		if (var14>var802)
			return (var14-var802);
		else
			return (var802-var14+1);
	}
}

class mthdcls665 {
	public static int method665 (int var116, int var17) {
		if (var116>var17)
			return (var116*var17);
		else
			return (var17*var116+1);
	}
}

class mthdcls666 {
	public static int method666 (int var729, int var82) {
		if (var729>var82)
			return (var729+var82);
		else
			return (var82+var729+1);
	}
}

class mthdcls667 {
	public static int method667 (int var4, int var74) {
		if (var4>var74)
			return (var4-var74);
		else
			return (var74-var4+1);
	}
}

class mthdcls668 {
	public static int method668 (int var356, int var513) {
		if (var356>var513)
			return (var356-var513);
		else
			return (var513-var356+1);
	}
}

class mthdcls669 {
	public static int method669 (int var33, int var664) {
		if (var33>var664)
			return (var33*var664);
		else
			return (var664*var33+1);
	}
}

class mthdcls670 {
	public static int method670 (int var481, int var941) {
		if (var481>var941)
			return (var481+var941);
		else
			return (var941+var481+1);
	}
}

class mthdcls671 {
	public static int method671 (int var409, int var3) {
		if (var409>var3)
			return (var409*var3);
		else
			return (var3*var409+1);
	}
}

class mthdcls672 {
	public static int method672 (int var40, int var470) {
		if (var40>var470)
			return (var40-var470);
		else
			return (var470-var40+1);
	}
}

class mthdcls673 {
	public static int method673 (int var100, int var611) {
		if (var100>var611)
			return (var100+var611);
		else
			return (var611+var100+1);
	}
}

class mthdcls674 {
	public static int method674 (int var264, int var105) {
		if (var264>var105)
			return (var264+var105);
		else
			return (var105+var264+1);
	}
}

class mthdcls675 {
	public static int method675 (int var977, int var58) {
		if (var977>var58)
			return (var977-var58);
		else
			return (var58-var977+1);
	}
}

class mthdcls676 {
	public static int method676 (int var697, int var261) {
		if (var697>var261)
			return (var697-var261);
		else
			return (var261-var697+1);
	}
}

class mthdcls677 {
	public static int method677 (int var60, int var377) {
		if (var60>var377)
			return (var60*var377);
		else
			return (var377*var60+1);
	}
}

class mthdcls678 {
	public static int method678 (int var762, int var177) {
		if (var762>var177)
			return (var762-var177);
		else
			return (var177-var762+1);
	}
}

class mthdcls679 {
	public static int method679 (int var916, int var471) {
		if (var916>var471)
			return (var916*var471);
		else
			return (var471*var916+1);
	}
}

class mthdcls680 {
	public static int method680 (int var818, int var874) {
		if (var818>var874)
			return (var818*var874);
		else
			return (var874*var818+1);
	}
}

class mthdcls681 {
	public static int method681 (int var97, int var510) {
		if (var97>var510)
			return (var97-var510);
		else
			return (var510-var97+1);
	}
}

class mthdcls682 {
	public static int method682 (int var815, int var392) {
		if (var815>var392)
			return (var815-var392);
		else
			return (var392-var815+1);
	}
}

class mthdcls683 {
	public static int method683 (int var604, int var707) {
		if (var604>var707)
			return (var604-var707);
		else
			return (var707-var604+1);
	}
}

class mthdcls684 {
	public static int method684 (int var179, int var613) {
		if (var179>var613)
			return (var179*var613);
		else
			return (var613*var179+1);
	}
}

class mthdcls685 {
	public static int method685 (int var86, int var613) {
		if (var86>var613)
			return (var86-var613);
		else
			return (var613-var86+1);
	}
}

class mthdcls686 {
	public static int method686 (int var651, int var549) {
		if (var651>var549)
			return (var651+var549);
		else
			return (var549+var651+1);
	}
}

class mthdcls687 {
	public static int method687 (int var514, int var426) {
		if (var514>var426)
			return (var514+var426);
		else
			return (var426+var514+1);
	}
}

class mthdcls688 {
	public static int method688 (int var289, int var490) {
		if (var289>var490)
			return (var289*var490);
		else
			return (var490*var289+1);
	}
}

class mthdcls689 {
	public static int method689 (int var454, int var838) {
		if (var454>var838)
			return (var454-var838);
		else
			return (var838-var454+1);
	}
}

class mthdcls690 {
	public static int method690 (int var82, int var870) {
		if (var82>var870)
			return (var82+var870);
		else
			return (var870+var82+1);
	}
}

class mthdcls691 {
	public static int method691 (int var291, int var708) {
		if (var291>var708)
			return (var291-var708);
		else
			return (var708-var291+1);
	}
}

class mthdcls692 {
	public static int method692 (int var525, int var943) {
		if (var525>var943)
			return (var525-var943);
		else
			return (var943-var525+1);
	}
}

class mthdcls693 {
	public static int method693 (int var957, int var866) {
		if (var957>var866)
			return (var957-var866);
		else
			return (var866-var957+1);
	}
}

class mthdcls694 {
	public static int method694 (int var584, int var550) {
		if (var584>var550)
			return (var584+var550);
		else
			return (var550+var584+1);
	}
}

class mthdcls695 {
	public static int method695 (int var56, int var723) {
		if (var56>var723)
			return (var56+var723);
		else
			return (var723+var56+1);
	}
}

class mthdcls696 {
	public static int method696 (int var310, int var692) {
		if (var310>var692)
			return (var310+var692);
		else
			return (var692+var310+1);
	}
}

class mthdcls697 {
	public static int method697 (int var416, int var731) {
		if (var416>var731)
			return (var416+var731);
		else
			return (var731+var416+1);
	}
}

class mthdcls698 {
	public static int method698 (int var88, int var299) {
		if (var88>var299)
			return (var88-var299);
		else
			return (var299-var88+1);
	}
}

class mthdcls699 {
	public static int method699 (int var655, int var729) {
		if (var655>var729)
			return (var655*var729);
		else
			return (var729*var655+1);
	}
}

class mthdcls700 {
	public static int method700 (int var964, int var225) {
		if (var964>var225)
			return (var964*var225);
		else
			return (var225*var964+1);
	}
}

class mthdcls701 {
	public static int method701 (int var370, int var115) {
		if (var370>var115)
			return (var370*var115);
		else
			return (var115*var370+1);
	}
}

class mthdcls702 {
	public static int method702 (int var180, int var622) {
		if (var180>var622)
			return (var180*var622);
		else
			return (var622*var180+1);
	}
}

class mthdcls703 {
	public static int method703 (int var421, int var885) {
		if (var421>var885)
			return (var421-var885);
		else
			return (var885-var421+1);
	}
}

class mthdcls704 {
	public static int method704 (int var782, int var643) {
		if (var782>var643)
			return (var782-var643);
		else
			return (var643-var782+1);
	}
}

class mthdcls705 {
	public static int method705 (int var52, int var802) {
		if (var52>var802)
			return (var52+var802);
		else
			return (var802+var52+1);
	}
}

class mthdcls706 {
	public static int method706 (int var978, int var299) {
		if (var978>var299)
			return (var978+var299);
		else
			return (var299+var978+1);
	}
}

class mthdcls707 {
	public static int method707 (int var496, int var374) {
		if (var496>var374)
			return (var496-var374);
		else
			return (var374-var496+1);
	}
}

class mthdcls708 {
	public static int method708 (int var369, int var190) {
		if (var369>var190)
			return (var369+var190);
		else
			return (var190+var369+1);
	}
}

class mthdcls709 {
	public static int method709 (int var301, int var795) {
		if (var301>var795)
			return (var301+var795);
		else
			return (var795+var301+1);
	}
}

class mthdcls710 {
	public static int method710 (int var909, int var260) {
		if (var909>var260)
			return (var909*var260);
		else
			return (var260*var909+1);
	}
}

class mthdcls711 {
	public static int method711 (int var678, int var21) {
		if (var678>var21)
			return (var678*var21);
		else
			return (var21*var678+1);
	}
}

class mthdcls712 {
	public static int method712 (int var320, int var284) {
		if (var320>var284)
			return (var320-var284);
		else
			return (var284-var320+1);
	}
}

class mthdcls713 {
	public static int method713 (int var232, int var845) {
		if (var232>var845)
			return (var232*var845);
		else
			return (var845*var232+1);
	}
}

class mthdcls714 {
	public static int method714 (int var855, int var969) {
		if (var855>var969)
			return (var855+var969);
		else
			return (var969+var855+1);
	}
}

class mthdcls715 {
	public static int method715 (int var814, int var487) {
		if (var814>var487)
			return (var814+var487);
		else
			return (var487+var814+1);
	}
}

class mthdcls716 {
	public static int method716 (int var712, int var965) {
		if (var712>var965)
			return (var712-var965);
		else
			return (var965-var712+1);
	}
}

class mthdcls717 {
	public static int method717 (int var207, int var497) {
		if (var207>var497)
			return (var207-var497);
		else
			return (var497-var207+1);
	}
}

class mthdcls718 {
	public static int method718 (int var323, int var82) {
		if (var323>var82)
			return (var323+var82);
		else
			return (var82+var323+1);
	}
}

class mthdcls719 {
	public static int method719 (int var569, int var983) {
		if (var569>var983)
			return (var569-var983);
		else
			return (var983-var569+1);
	}
}

class mthdcls720 {
	public static int method720 (int var444, int var813) {
		if (var444>var813)
			return (var444-var813);
		else
			return (var813-var444+1);
	}
}

class mthdcls721 {
	public static int method721 (int var560, int var83) {
		if (var560>var83)
			return (var560+var83);
		else
			return (var83+var560+1);
	}
}

class mthdcls722 {
	public static int method722 (int var478, int var775) {
		if (var478>var775)
			return (var478+var775);
		else
			return (var775+var478+1);
	}
}

class mthdcls723 {
	public static int method723 (int var588, int var456) {
		if (var588>var456)
			return (var588+var456);
		else
			return (var456+var588+1);
	}
}

class mthdcls724 {
	public static int method724 (int var937, int var203) {
		if (var937>var203)
			return (var937-var203);
		else
			return (var203-var937+1);
	}
}

class mthdcls725 {
	public static int method725 (int var452, int var315) {
		if (var452>var315)
			return (var452+var315);
		else
			return (var315+var452+1);
	}
}

class mthdcls726 {
	public static int method726 (int var839, int var546) {
		if (var839>var546)
			return (var839-var546);
		else
			return (var546-var839+1);
	}
}

class mthdcls727 {
	public static int method727 (int var285, int var143) {
		if (var285>var143)
			return (var285*var143);
		else
			return (var143*var285+1);
	}
}

class mthdcls728 {
	public static int method728 (int var490, int var30) {
		if (var490>var30)
			return (var490-var30);
		else
			return (var30-var490+1);
	}
}

class mthdcls729 {
	public static int method729 (int var898, int var405) {
		if (var898>var405)
			return (var898*var405);
		else
			return (var405*var898+1);
	}
}

class mthdcls730 {
	public static int method730 (int var28, int var311) {
		if (var28>var311)
			return (var28+var311);
		else
			return (var311+var28+1);
	}
}

class mthdcls731 {
	public static int method731 (int var45, int var339) {
		if (var45>var339)
			return (var45-var339);
		else
			return (var339-var45+1);
	}
}

class mthdcls732 {
	public static int method732 (int var242, int var241) {
		if (var242>var241)
			return (var242*var241);
		else
			return (var241*var242+1);
	}
}

class mthdcls733 {
	public static int method733 (int var698, int var355) {
		if (var698>var355)
			return (var698-var355);
		else
			return (var355-var698+1);
	}
}

class mthdcls734 {
	public static int method734 (int var322, int var573) {
		if (var322>var573)
			return (var322*var573);
		else
			return (var573*var322+1);
	}
}

class mthdcls735 {
	public static int method735 (int var185, int var500) {
		if (var185>var500)
			return (var185-var500);
		else
			return (var500-var185+1);
	}
}

class mthdcls736 {
	public static int method736 (int var747, int var428) {
		if (var747>var428)
			return (var747-var428);
		else
			return (var428-var747+1);
	}
}

class mthdcls737 {
	public static int method737 (int var411, int var362) {
		if (var411>var362)
			return (var411+var362);
		else
			return (var362+var411+1);
	}
}

class mthdcls738 {
	public static int method738 (int var961, int var239) {
		if (var961>var239)
			return (var961+var239);
		else
			return (var239+var961+1);
	}
}

class mthdcls739 {
	public static int method739 (int var616, int var505) {
		if (var616>var505)
			return (var616*var505);
		else
			return (var505*var616+1);
	}
}

class mthdcls740 {
	public static int method740 (int var224, int var68) {
		if (var224>var68)
			return (var224+var68);
		else
			return (var68+var224+1);
	}
}

class mthdcls741 {
	public static int method741 (int var958, int var790) {
		if (var958>var790)
			return (var958+var790);
		else
			return (var790+var958+1);
	}
}

class mthdcls742 {
	public static int method742 (int var283, int var945) {
		if (var283>var945)
			return (var283*var945);
		else
			return (var945*var283+1);
	}
}

class mthdcls743 {
	public static int method743 (int var993, int var758) {
		if (var993>var758)
			return (var993*var758);
		else
			return (var758*var993+1);
	}
}

class mthdcls744 {
	public static int method744 (int var350, int var433) {
		if (var350>var433)
			return (var350-var433);
		else
			return (var433-var350+1);
	}
}

class mthdcls745 {
	public static int method745 (int var722, int var53) {
		if (var722>var53)
			return (var722+var53);
		else
			return (var53+var722+1);
	}
}

class mthdcls746 {
	public static int method746 (int var16, int var97) {
		if (var16>var97)
			return (var16+var97);
		else
			return (var97+var16+1);
	}
}

class mthdcls747 {
	public static int method747 (int var397, int var700) {
		if (var397>var700)
			return (var397*var700);
		else
			return (var700*var397+1);
	}
}

class mthdcls748 {
	public static int method748 (int var289, int var60) {
		if (var289>var60)
			return (var289+var60);
		else
			return (var60+var289+1);
	}
}

class mthdcls749 {
	public static int method749 (int var506, int var666) {
		if (var506>var666)
			return (var506+var666);
		else
			return (var666+var506+1);
	}
}

class mthdcls750 {
	public static int method750 (int var78, int var41) {
		if (var78>var41)
			return (var78-var41);
		else
			return (var41-var78+1);
	}
}

class mthdcls751 {
	public static int method751 (int var640, int var875) {
		if (var640>var875)
			return (var640-var875);
		else
			return (var875-var640+1);
	}
}

class mthdcls752 {
	public static int method752 (int var222, int var608) {
		if (var222>var608)
			return (var222+var608);
		else
			return (var608+var222+1);
	}
}

class mthdcls753 {
	public static int method753 (int var108, int var347) {
		if (var108>var347)
			return (var108-var347);
		else
			return (var347-var108+1);
	}
}

class mthdcls754 {
	public static int method754 (int var779, int var784) {
		if (var779>var784)
			return (var779+var784);
		else
			return (var784+var779+1);
	}
}

class mthdcls755 {
	public static int method755 (int var284, int var217) {
		if (var284>var217)
			return (var284*var217);
		else
			return (var217*var284+1);
	}
}

class mthdcls756 {
	public static int method756 (int var866, int var149) {
		if (var866>var149)
			return (var866*var149);
		else
			return (var149*var866+1);
	}
}

class mthdcls757 {
	public static int method757 (int var118, int var418) {
		if (var118>var418)
			return (var118-var418);
		else
			return (var418-var118+1);
	}
}

class mthdcls758 {
	public static int method758 (int var210, int var21) {
		if (var210>var21)
			return (var210-var21);
		else
			return (var21-var210+1);
	}
}

class mthdcls759 {
	public static int method759 (int var945, int var390) {
		if (var945>var390)
			return (var945*var390);
		else
			return (var390*var945+1);
	}
}

class mthdcls760 {
	public static int method760 (int var318, int var89) {
		if (var318>var89)
			return (var318-var89);
		else
			return (var89-var318+1);
	}
}

class mthdcls761 {
	public static int method761 (int var415, int var32) {
		if (var415>var32)
			return (var415+var32);
		else
			return (var32+var415+1);
	}
}

class mthdcls762 {
	public static int method762 (int var193, int var552) {
		if (var193>var552)
			return (var193-var552);
		else
			return (var552-var193+1);
	}
}

class mthdcls763 {
	public static int method763 (int var548, int var543) {
		if (var548>var543)
			return (var548-var543);
		else
			return (var543-var548+1);
	}
}

class mthdcls764 {
	public static int method764 (int var831, int var602) {
		if (var831>var602)
			return (var831-var602);
		else
			return (var602-var831+1);
	}
}

class mthdcls765 {
	public static int method765 (int var929, int var23) {
		if (var929>var23)
			return (var929-var23);
		else
			return (var23-var929+1);
	}
}

class mthdcls766 {
	public static int method766 (int var289, int var772) {
		if (var289>var772)
			return (var289+var772);
		else
			return (var772+var289+1);
	}
}

class mthdcls767 {
	public static int method767 (int var931, int var421) {
		if (var931>var421)
			return (var931*var421);
		else
			return (var421*var931+1);
	}
}

class mthdcls768 {
	public static int method768 (int var590, int var137) {
		if (var590>var137)
			return (var590+var137);
		else
			return (var137+var590+1);
	}
}

class mthdcls769 {
	public static int method769 (int var669, int var730) {
		if (var669>var730)
			return (var669*var730);
		else
			return (var730*var669+1);
	}
}

class mthdcls770 {
	public static int method770 (int var318, int var419) {
		if (var318>var419)
			return (var318*var419);
		else
			return (var419*var318+1);
	}
}

class mthdcls771 {
	public static int method771 (int var488, int var438) {
		if (var488>var438)
			return (var488-var438);
		else
			return (var438-var488+1);
	}
}

class mthdcls772 {
	public static int method772 (int var731, int var677) {
		if (var731>var677)
			return (var731+var677);
		else
			return (var677+var731+1);
	}
}

class mthdcls773 {
	public static int method773 (int var458, int var662) {
		if (var458>var662)
			return (var458-var662);
		else
			return (var662-var458+1);
	}
}

class mthdcls774 {
	public static int method774 (int var498, int var480) {
		if (var498>var480)
			return (var498*var480);
		else
			return (var480*var498+1);
	}
}

class mthdcls775 {
	public static int method775 (int var363, int var84) {
		if (var363>var84)
			return (var363+var84);
		else
			return (var84+var363+1);
	}
}

class mthdcls776 {
	public static int method776 (int var233, int var99) {
		if (var233>var99)
			return (var233-var99);
		else
			return (var99-var233+1);
	}
}

class mthdcls777 {
	public static int method777 (int var54, int var969) {
		if (var54>var969)
			return (var54-var969);
		else
			return (var969-var54+1);
	}
}

class mthdcls778 {
	public static int method778 (int var870, int var972) {
		if (var870>var972)
			return (var870-var972);
		else
			return (var972-var870+1);
	}
}

class mthdcls779 {
	public static int method779 (int var565, int var677) {
		if (var565>var677)
			return (var565+var677);
		else
			return (var677+var565+1);
	}
}

class mthdcls780 {
	public static int method780 (int var273, int var211) {
		if (var273>var211)
			return (var273*var211);
		else
			return (var211*var273+1);
	}
}

class mthdcls781 {
	public static int method781 (int var57, int var236) {
		if (var57>var236)
			return (var57*var236);
		else
			return (var236*var57+1);
	}
}

class mthdcls782 {
	public static int method782 (int var903, int var800) {
		if (var903>var800)
			return (var903*var800);
		else
			return (var800*var903+1);
	}
}

class mthdcls783 {
	public static int method783 (int var675, int var34) {
		if (var675>var34)
			return (var675*var34);
		else
			return (var34*var675+1);
	}
}

class mthdcls784 {
	public static int method784 (int var548, int var269) {
		if (var548>var269)
			return (var548-var269);
		else
			return (var269-var548+1);
	}
}

class mthdcls785 {
	public static int method785 (int var873, int var909) {
		if (var873>var909)
			return (var873*var909);
		else
			return (var909*var873+1);
	}
}

class mthdcls786 {
	public static int method786 (int var404, int var509) {
		if (var404>var509)
			return (var404+var509);
		else
			return (var509+var404+1);
	}
}

class mthdcls787 {
	public static int method787 (int var604, int var322) {
		if (var604>var322)
			return (var604-var322);
		else
			return (var322-var604+1);
	}
}

class mthdcls788 {
	public static int method788 (int var577, int var277) {
		if (var577>var277)
			return (var577-var277);
		else
			return (var277-var577+1);
	}
}

class mthdcls789 {
	public static int method789 (int var408, int var837) {
		if (var408>var837)
			return (var408*var837);
		else
			return (var837*var408+1);
	}
}

class mthdcls790 {
	public static int method790 (int var550, int var100) {
		if (var550>var100)
			return (var550*var100);
		else
			return (var100*var550+1);
	}
}

class mthdcls791 {
	public static int method791 (int var96, int var655) {
		if (var96>var655)
			return (var96+var655);
		else
			return (var655+var96+1);
	}
}

class mthdcls792 {
	public static int method792 (int var568, int var997) {
		if (var568>var997)
			return (var568-var997);
		else
			return (var997-var568+1);
	}
}

class mthdcls793 {
	public static int method793 (int var72, int var461) {
		if (var72>var461)
			return (var72-var461);
		else
			return (var461-var72+1);
	}
}

class mthdcls794 {
	public static int method794 (int var634, int var773) {
		if (var634>var773)
			return (var634+var773);
		else
			return (var773+var634+1);
	}
}

class mthdcls795 {
	public static int method795 (int var828, int var193) {
		if (var828>var193)
			return (var828+var193);
		else
			return (var193+var828+1);
	}
}

class mthdcls796 {
	public static int method796 (int var250, int var199) {
		if (var250>var199)
			return (var250+var199);
		else
			return (var199+var250+1);
	}
}

class mthdcls797 {
	public static int method797 (int var938, int var494) {
		if (var938>var494)
			return (var938-var494);
		else
			return (var494-var938+1);
	}
}

class mthdcls798 {
	public static int method798 (int var701, int var386) {
		if (var701>var386)
			return (var701*var386);
		else
			return (var386*var701+1);
	}
}

class mthdcls799 {
	public static int method799 (int var836, int var624) {
		if (var836>var624)
			return (var836-var624);
		else
			return (var624-var836+1);
	}
}

class mthdcls800 {
	public static int method800 (int var874, int var991) {
		if (var874>var991)
			return (var874-var991);
		else
			return (var991-var874+1);
	}
}

class mthdcls801 {
	public static int method801 (int var494, int var191) {
		if (var494>var191)
			return (var494-var191);
		else
			return (var191-var494+1);
	}
}

class mthdcls802 {
	public static int method802 (int var27, int var244) {
		if (var27>var244)
			return (var27-var244);
		else
			return (var244-var27+1);
	}
}

class mthdcls803 {
	public static int method803 (int var898, int var835) {
		if (var898>var835)
			return (var898-var835);
		else
			return (var835-var898+1);
	}
}

class mthdcls804 {
	public static int method804 (int var399, int var887) {
		if (var399>var887)
			return (var399-var887);
		else
			return (var887-var399+1);
	}
}

class mthdcls805 {
	public static int method805 (int var818, int var254) {
		if (var818>var254)
			return (var818-var254);
		else
			return (var254-var818+1);
	}
}

class mthdcls806 {
	public static int method806 (int var123, int var358) {
		if (var123>var358)
			return (var123+var358);
		else
			return (var358+var123+1);
	}
}

class mthdcls807 {
	public static int method807 (int var967, int var297) {
		if (var967>var297)
			return (var967*var297);
		else
			return (var297*var967+1);
	}
}

class mthdcls808 {
	public static int method808 (int var746, int var497) {
		if (var746>var497)
			return (var746*var497);
		else
			return (var497*var746+1);
	}
}

class mthdcls809 {
	public static int method809 (int var294, int var945) {
		if (var294>var945)
			return (var294-var945);
		else
			return (var945-var294+1);
	}
}

class mthdcls810 {
	public static int method810 (int var800, int var850) {
		if (var800>var850)
			return (var800-var850);
		else
			return (var850-var800+1);
	}
}

class mthdcls811 {
	public static int method811 (int var675, int var222) {
		if (var675>var222)
			return (var675+var222);
		else
			return (var222+var675+1);
	}
}

class mthdcls812 {
	public static int method812 (int var263, int var666) {
		if (var263>var666)
			return (var263+var666);
		else
			return (var666+var263+1);
	}
}

class mthdcls813 {
	public static int method813 (int var657, int var71) {
		if (var657>var71)
			return (var657*var71);
		else
			return (var71*var657+1);
	}
}

class mthdcls814 {
	public static int method814 (int var244, int var32) {
		if (var244>var32)
			return (var244+var32);
		else
			return (var32+var244+1);
	}
}

class mthdcls815 {
	public static int method815 (int var896, int var660) {
		if (var896>var660)
			return (var896+var660);
		else
			return (var660+var896+1);
	}
}

class mthdcls816 {
	public static int method816 (int var444, int var763) {
		if (var444>var763)
			return (var444*var763);
		else
			return (var763*var444+1);
	}
}

class mthdcls817 {
	public static int method817 (int var160, int var179) {
		if (var160>var179)
			return (var160*var179);
		else
			return (var179*var160+1);
	}
}

class mthdcls818 {
	public static int method818 (int var623, int var987) {
		if (var623>var987)
			return (var623+var987);
		else
			return (var987+var623+1);
	}
}

class mthdcls819 {
	public static int method819 (int var597, int var573) {
		if (var597>var573)
			return (var597+var573);
		else
			return (var573+var597+1);
	}
}

class mthdcls820 {
	public static int method820 (int var113, int var730) {
		if (var113>var730)
			return (var113-var730);
		else
			return (var730-var113+1);
	}
}

class mthdcls821 {
	public static int method821 (int var400, int var617) {
		if (var400>var617)
			return (var400*var617);
		else
			return (var617*var400+1);
	}
}

class mthdcls822 {
	public static int method822 (int var52, int var326) {
		if (var52>var326)
			return (var52-var326);
		else
			return (var326-var52+1);
	}
}

class mthdcls823 {
	public static int method823 (int var785, int var443) {
		if (var785>var443)
			return (var785*var443);
		else
			return (var443*var785+1);
	}
}

class mthdcls824 {
	public static int method824 (int var227, int var355) {
		if (var227>var355)
			return (var227+var355);
		else
			return (var355+var227+1);
	}
}

class mthdcls825 {
	public static int method825 (int var625, int var316) {
		if (var625>var316)
			return (var625+var316);
		else
			return (var316+var625+1);
	}
}

class mthdcls826 {
	public static int method826 (int var553, int var86) {
		if (var553>var86)
			return (var553*var86);
		else
			return (var86*var553+1);
	}
}

class mthdcls827 {
	public static int method827 (int var670, int var623) {
		if (var670>var623)
			return (var670-var623);
		else
			return (var623-var670+1);
	}
}

class mthdcls828 {
	public static int method828 (int var980, int var481) {
		if (var980>var481)
			return (var980+var481);
		else
			return (var481+var980+1);
	}
}

class mthdcls829 {
	public static int method829 (int var875, int var487) {
		if (var875>var487)
			return (var875+var487);
		else
			return (var487+var875+1);
	}
}

class mthdcls830 {
	public static int method830 (int var536, int var714) {
		if (var536>var714)
			return (var536+var714);
		else
			return (var714+var536+1);
	}
}

class mthdcls831 {
	public static int method831 (int var553, int var807) {
		if (var553>var807)
			return (var553+var807);
		else
			return (var807+var553+1);
	}
}

class mthdcls832 {
	public static int method832 (int var434, int var612) {
		if (var434>var612)
			return (var434+var612);
		else
			return (var612+var434+1);
	}
}

class mthdcls833 {
	public static int method833 (int var903, int var184) {
		if (var903>var184)
			return (var903+var184);
		else
			return (var184+var903+1);
	}
}

class mthdcls834 {
	public static int method834 (int var750, int var882) {
		if (var750>var882)
			return (var750*var882);
		else
			return (var882*var750+1);
	}
}

class mthdcls835 {
	public static int method835 (int var742, int var664) {
		if (var742>var664)
			return (var742*var664);
		else
			return (var664*var742+1);
	}
}

class mthdcls836 {
	public static int method836 (int var790, int var631) {
		if (var790>var631)
			return (var790*var631);
		else
			return (var631*var790+1);
	}
}

class mthdcls837 {
	public static int method837 (int var889, int var303) {
		if (var889>var303)
			return (var889*var303);
		else
			return (var303*var889+1);
	}
}

class mthdcls838 {
	public static int method838 (int var920, int var110) {
		if (var920>var110)
			return (var920-var110);
		else
			return (var110-var920+1);
	}
}

class mthdcls839 {
	public static int method839 (int var413, int var660) {
		if (var413>var660)
			return (var413*var660);
		else
			return (var660*var413+1);
	}
}

class mthdcls840 {
	public static int method840 (int var336, int var417) {
		if (var336>var417)
			return (var336-var417);
		else
			return (var417-var336+1);
	}
}

class mthdcls841 {
	public static int method841 (int var881, int var976) {
		if (var881>var976)
			return (var881*var976);
		else
			return (var976*var881+1);
	}
}

class mthdcls842 {
	public static int method842 (int var691, int var100) {
		if (var691>var100)
			return (var691*var100);
		else
			return (var100*var691+1);
	}
}

class mthdcls843 {
	public static int method843 (int var597, int var376) {
		if (var597>var376)
			return (var597*var376);
		else
			return (var376*var597+1);
	}
}

class mthdcls844 {
	public static int method844 (int var793, int var642) {
		if (var793>var642)
			return (var793+var642);
		else
			return (var642+var793+1);
	}
}

class mthdcls845 {
	public static int method845 (int var394, int var241) {
		if (var394>var241)
			return (var394-var241);
		else
			return (var241-var394+1);
	}
}

class mthdcls846 {
	public static int method846 (int var275, int var438) {
		if (var275>var438)
			return (var275*var438);
		else
			return (var438*var275+1);
	}
}

class mthdcls847 {
	public static int method847 (int var302, int var714) {
		if (var302>var714)
			return (var302+var714);
		else
			return (var714+var302+1);
	}
}

class mthdcls848 {
	public static int method848 (int var190, int var56) {
		if (var190>var56)
			return (var190*var56);
		else
			return (var56*var190+1);
	}
}

class mthdcls849 {
	public static int method849 (int var66, int var897) {
		if (var66>var897)
			return (var66-var897);
		else
			return (var897-var66+1);
	}
}

class mthdcls850 {
	public static int method850 (int var837, int var421) {
		if (var837>var421)
			return (var837*var421);
		else
			return (var421*var837+1);
	}
}

class mthdcls851 {
	public static int method851 (int var909, int var86) {
		if (var909>var86)
			return (var909*var86);
		else
			return (var86*var909+1);
	}
}

class mthdcls852 {
	public static int method852 (int var225, int var578) {
		if (var225>var578)
			return (var225+var578);
		else
			return (var578+var225+1);
	}
}

class mthdcls853 {
	public static int method853 (int var527, int var923) {
		if (var527>var923)
			return (var527+var923);
		else
			return (var923+var527+1);
	}
}

class mthdcls854 {
	public static int method854 (int var554, int var40) {
		if (var554>var40)
			return (var554+var40);
		else
			return (var40+var554+1);
	}
}

class mthdcls855 {
	public static int method855 (int var118, int var895) {
		if (var118>var895)
			return (var118+var895);
		else
			return (var895+var118+1);
	}
}

class mthdcls856 {
	public static int method856 (int var592, int var560) {
		if (var592>var560)
			return (var592-var560);
		else
			return (var560-var592+1);
	}
}

class mthdcls857 {
	public static int method857 (int var329, int var914) {
		if (var329>var914)
			return (var329-var914);
		else
			return (var914-var329+1);
	}
}

class mthdcls858 {
	public static int method858 (int var749, int var179) {
		if (var749>var179)
			return (var749-var179);
		else
			return (var179-var749+1);
	}
}

class mthdcls859 {
	public static int method859 (int var921, int var780) {
		if (var921>var780)
			return (var921-var780);
		else
			return (var780-var921+1);
	}
}

class mthdcls860 {
	public static int method860 (int var664, int var513) {
		if (var664>var513)
			return (var664-var513);
		else
			return (var513-var664+1);
	}
}

class mthdcls861 {
	public static int method861 (int var687, int var536) {
		if (var687>var536)
			return (var687*var536);
		else
			return (var536*var687+1);
	}
}

class mthdcls862 {
	public static int method862 (int var715, int var452) {
		if (var715>var452)
			return (var715-var452);
		else
			return (var452-var715+1);
	}
}

class mthdcls863 {
	public static int method863 (int var93, int var875) {
		if (var93>var875)
			return (var93*var875);
		else
			return (var875*var93+1);
	}
}

class mthdcls864 {
	public static int method864 (int var419, int var952) {
		if (var419>var952)
			return (var419-var952);
		else
			return (var952-var419+1);
	}
}

class mthdcls865 {
	public static int method865 (int var165, int var435) {
		if (var165>var435)
			return (var165-var435);
		else
			return (var435-var165+1);
	}
}

class mthdcls866 {
	public static int method866 (int var913, int var344) {
		if (var913>var344)
			return (var913+var344);
		else
			return (var344+var913+1);
	}
}

class mthdcls867 {
	public static int method867 (int var235, int var367) {
		if (var235>var367)
			return (var235+var367);
		else
			return (var367+var235+1);
	}
}

class mthdcls868 {
	public static int method868 (int var325, int var416) {
		if (var325>var416)
			return (var325*var416);
		else
			return (var416*var325+1);
	}
}

class mthdcls869 {
	public static int method869 (int var802, int var26) {
		if (var802>var26)
			return (var802*var26);
		else
			return (var26*var802+1);
	}
}

class mthdcls870 {
	public static int method870 (int var140, int var513) {
		if (var140>var513)
			return (var140*var513);
		else
			return (var513*var140+1);
	}
}

class mthdcls871 {
	public static int method871 (int var748, int var142) {
		if (var748>var142)
			return (var748-var142);
		else
			return (var142-var748+1);
	}
}

class mthdcls872 {
	public static int method872 (int var265, int var190) {
		if (var265>var190)
			return (var265+var190);
		else
			return (var190+var265+1);
	}
}

class mthdcls873 {
	public static int method873 (int var545, int var4) {
		if (var545>var4)
			return (var545*var4);
		else
			return (var4*var545+1);
	}
}

class mthdcls874 {
	public static int method874 (int var188, int var17) {
		if (var188>var17)
			return (var188-var17);
		else
			return (var17-var188+1);
	}
}

class mthdcls875 {
	public static int method875 (int var734, int var261) {
		if (var734>var261)
			return (var734+var261);
		else
			return (var261+var734+1);
	}
}

class mthdcls876 {
	public static int method876 (int var311, int var86) {
		if (var311>var86)
			return (var311-var86);
		else
			return (var86-var311+1);
	}
}

class mthdcls877 {
	public static int method877 (int var126, int var118) {
		if (var126>var118)
			return (var126*var118);
		else
			return (var118*var126+1);
	}
}

class mthdcls878 {
	public static int method878 (int var687, int var183) {
		if (var687>var183)
			return (var687*var183);
		else
			return (var183*var687+1);
	}
}

class mthdcls879 {
	public static int method879 (int var890, int var459) {
		if (var890>var459)
			return (var890+var459);
		else
			return (var459+var890+1);
	}
}

class mthdcls880 {
	public static int method880 (int var156, int var906) {
		if (var156>var906)
			return (var156*var906);
		else
			return (var906*var156+1);
	}
}

class mthdcls881 {
	public static int method881 (int var632, int var676) {
		if (var632>var676)
			return (var632+var676);
		else
			return (var676+var632+1);
	}
}

class mthdcls882 {
	public static int method882 (int var105, int var222) {
		if (var105>var222)
			return (var105-var222);
		else
			return (var222-var105+1);
	}
}

class mthdcls883 {
	public static int method883 (int var900, int var884) {
		if (var900>var884)
			return (var900+var884);
		else
			return (var884+var900+1);
	}
}

class mthdcls884 {
	public static int method884 (int var826, int var909) {
		if (var826>var909)
			return (var826*var909);
		else
			return (var909*var826+1);
	}
}

class mthdcls885 {
	public static int method885 (int var251, int var885) {
		if (var251>var885)
			return (var251+var885);
		else
			return (var885+var251+1);
	}
}

class mthdcls886 {
	public static int method886 (int var33, int var53) {
		if (var33>var53)
			return (var33+var53);
		else
			return (var53+var33+1);
	}
}

class mthdcls887 {
	public static int method887 (int var371, int var596) {
		if (var371>var596)
			return (var371+var596);
		else
			return (var596+var371+1);
	}
}

class mthdcls888 {
	public static int method888 (int var912, int var273) {
		if (var912>var273)
			return (var912-var273);
		else
			return (var273-var912+1);
	}
}

class mthdcls889 {
	public static int method889 (int var361, int var906) {
		if (var361>var906)
			return (var361*var906);
		else
			return (var906*var361+1);
	}
}

class mthdcls890 {
	public static int method890 (int var260, int var427) {
		if (var260>var427)
			return (var260-var427);
		else
			return (var427-var260+1);
	}
}

class mthdcls891 {
	public static int method891 (int var866, int var383) {
		if (var866>var383)
			return (var866+var383);
		else
			return (var383+var866+1);
	}
}

class mthdcls892 {
	public static int method892 (int var149, int var11) {
		if (var149>var11)
			return (var149+var11);
		else
			return (var11+var149+1);
	}
}

class mthdcls893 {
	public static int method893 (int var88, int var143) {
		if (var88>var143)
			return (var88*var143);
		else
			return (var143*var88+1);
	}
}

class mthdcls894 {
	public static int method894 (int var994, int var952) {
		if (var994>var952)
			return (var994-var952);
		else
			return (var952-var994+1);
	}
}

class mthdcls895 {
	public static int method895 (int var315, int var61) {
		if (var315>var61)
			return (var315-var61);
		else
			return (var61-var315+1);
	}
}

class mthdcls896 {
	public static int method896 (int var962, int var923) {
		if (var962>var923)
			return (var962-var923);
		else
			return (var923-var962+1);
	}
}

class mthdcls897 {
	public static int method897 (int var18, int var327) {
		if (var18>var327)
			return (var18+var327);
		else
			return (var327+var18+1);
	}
}

class mthdcls898 {
	public static int method898 (int var759, int var225) {
		if (var759>var225)
			return (var759+var225);
		else
			return (var225+var759+1);
	}
}

class mthdcls899 {
	public static int method899 (int var797, int var851) {
		if (var797>var851)
			return (var797*var851);
		else
			return (var851*var797+1);
	}
}

class mthdcls900 {
	public static int method900 (int var59, int var19) {
		if (var59>var19)
			return (var59+var19);
		else
			return (var19+var59+1);
	}
}

class mthdcls901 {
	public static int method901 (int var280, int var874) {
		if (var280>var874)
			return (var280*var874);
		else
			return (var874*var280+1);
	}
}

class mthdcls902 {
	public static int method902 (int var311, int var724) {
		if (var311>var724)
			return (var311*var724);
		else
			return (var724*var311+1);
	}
}

class mthdcls903 {
	public static int method903 (int var888, int var697) {
		if (var888>var697)
			return (var888*var697);
		else
			return (var697*var888+1);
	}
}

class mthdcls904 {
	public static int method904 (int var668, int var326) {
		if (var668>var326)
			return (var668-var326);
		else
			return (var326-var668+1);
	}
}

class mthdcls905 {
	public static int method905 (int var585, int var836) {
		if (var585>var836)
			return (var585*var836);
		else
			return (var836*var585+1);
	}
}

class mthdcls906 {
	public static int method906 (int var952, int var975) {
		if (var952>var975)
			return (var952+var975);
		else
			return (var975+var952+1);
	}
}

class mthdcls907 {
	public static int method907 (int var862, int var198) {
		if (var862>var198)
			return (var862-var198);
		else
			return (var198-var862+1);
	}
}

class mthdcls908 {
	public static int method908 (int var641, int var864) {
		if (var641>var864)
			return (var641-var864);
		else
			return (var864-var641+1);
	}
}

class mthdcls909 {
	public static int method909 (int var172, int var867) {
		if (var172>var867)
			return (var172*var867);
		else
			return (var867*var172+1);
	}
}

class mthdcls910 {
	public static int method910 (int var255, int var134) {
		if (var255>var134)
			return (var255+var134);
		else
			return (var134+var255+1);
	}
}

class mthdcls911 {
	public static int method911 (int var741, int var932) {
		if (var741>var932)
			return (var741*var932);
		else
			return (var932*var741+1);
	}
}

class mthdcls912 {
	public static int method912 (int var273, int var812) {
		if (var273>var812)
			return (var273+var812);
		else
			return (var812+var273+1);
	}
}

class mthdcls913 {
	public static int method913 (int var989, int var983) {
		if (var989>var983)
			return (var989+var983);
		else
			return (var983+var989+1);
	}
}

class mthdcls914 {
	public static int method914 (int var378, int var856) {
		if (var378>var856)
			return (var378+var856);
		else
			return (var856+var378+1);
	}
}

class mthdcls915 {
	public static int method915 (int var701, int var166) {
		if (var701>var166)
			return (var701-var166);
		else
			return (var166-var701+1);
	}
}

class mthdcls916 {
	public static int method916 (int var469, int var127) {
		if (var469>var127)
			return (var469+var127);
		else
			return (var127+var469+1);
	}
}

class mthdcls917 {
	public static int method917 (int var955, int var154) {
		if (var955>var154)
			return (var955+var154);
		else
			return (var154+var955+1);
	}
}

class mthdcls918 {
	public static int method918 (int var52, int var745) {
		if (var52>var745)
			return (var52-var745);
		else
			return (var745-var52+1);
	}
}

class mthdcls919 {
	public static int method919 (int var847, int var265) {
		if (var847>var265)
			return (var847+var265);
		else
			return (var265+var847+1);
	}
}

class mthdcls920 {
	public static int method920 (int var597, int var3) {
		if (var597>var3)
			return (var597+var3);
		else
			return (var3+var597+1);
	}
}

class mthdcls921 {
	public static int method921 (int var496, int var358) {
		if (var496>var358)
			return (var496+var358);
		else
			return (var358+var496+1);
	}
}

class mthdcls922 {
	public static int method922 (int var395, int var520) {
		if (var395>var520)
			return (var395+var520);
		else
			return (var520+var395+1);
	}
}

class mthdcls923 {
	public static int method923 (int var911, int var8) {
		if (var911>var8)
			return (var911-var8);
		else
			return (var8-var911+1);
	}
}

class mthdcls924 {
	public static int method924 (int var81, int var252) {
		if (var81>var252)
			return (var81-var252);
		else
			return (var252-var81+1);
	}
}

class mthdcls925 {
	public static int method925 (int var84, int var880) {
		if (var84>var880)
			return (var84-var880);
		else
			return (var880-var84+1);
	}
}

class mthdcls926 {
	public static int method926 (int var214, int var807) {
		if (var214>var807)
			return (var214-var807);
		else
			return (var807-var214+1);
	}
}

class mthdcls927 {
	public static int method927 (int var486, int var591) {
		if (var486>var591)
			return (var486+var591);
		else
			return (var591+var486+1);
	}
}

class mthdcls928 {
	public static int method928 (int var461, int var291) {
		if (var461>var291)
			return (var461*var291);
		else
			return (var291*var461+1);
	}
}

class mthdcls929 {
	public static int method929 (int var254, int var489) {
		if (var254>var489)
			return (var254+var489);
		else
			return (var489+var254+1);
	}
}

class mthdcls930 {
	public static int method930 (int var46, int var554) {
		if (var46>var554)
			return (var46-var554);
		else
			return (var554-var46+1);
	}
}

class mthdcls931 {
	public static int method931 (int var69, int var837) {
		if (var69>var837)
			return (var69*var837);
		else
			return (var837*var69+1);
	}
}

class mthdcls932 {
	public static int method932 (int var530, int var577) {
		if (var530>var577)
			return (var530*var577);
		else
			return (var577*var530+1);
	}
}

class mthdcls933 {
	public static int method933 (int var779, int var132) {
		if (var779>var132)
			return (var779-var132);
		else
			return (var132-var779+1);
	}
}

class mthdcls934 {
	public static int method934 (int var542, int var523) {
		if (var542>var523)
			return (var542*var523);
		else
			return (var523*var542+1);
	}
}

class mthdcls935 {
	public static int method935 (int var103, int var344) {
		if (var103>var344)
			return (var103-var344);
		else
			return (var344-var103+1);
	}
}

class mthdcls936 {
	public static int method936 (int var473, int var147) {
		if (var473>var147)
			return (var473-var147);
		else
			return (var147-var473+1);
	}
}

class mthdcls937 {
	public static int method937 (int var938, int var541) {
		if (var938>var541)
			return (var938-var541);
		else
			return (var541-var938+1);
	}
}

class mthdcls938 {
	public static int method938 (int var722, int var861) {
		if (var722>var861)
			return (var722*var861);
		else
			return (var861*var722+1);
	}
}

class mthdcls939 {
	public static int method939 (int var101, int var229) {
		if (var101>var229)
			return (var101+var229);
		else
			return (var229+var101+1);
	}
}

class mthdcls940 {
	public static int method940 (int var264, int var830) {
		if (var264>var830)
			return (var264+var830);
		else
			return (var830+var264+1);
	}
}

class mthdcls941 {
	public static int method941 (int var487, int var548) {
		if (var487>var548)
			return (var487-var548);
		else
			return (var548-var487+1);
	}
}

class mthdcls942 {
	public static int method942 (int var817, int var920) {
		if (var817>var920)
			return (var817*var920);
		else
			return (var920*var817+1);
	}
}

class mthdcls943 {
	public static int method943 (int var550, int var293) {
		if (var550>var293)
			return (var550-var293);
		else
			return (var293-var550+1);
	}
}

class mthdcls944 {
	public static int method944 (int var832, int var763) {
		if (var832>var763)
			return (var832-var763);
		else
			return (var763-var832+1);
	}
}

class mthdcls945 {
	public static int method945 (int var736, int var991) {
		if (var736>var991)
			return (var736*var991);
		else
			return (var991*var736+1);
	}
}

class mthdcls946 {
	public static int method946 (int var312, int var231) {
		if (var312>var231)
			return (var312*var231);
		else
			return (var231*var312+1);
	}
}

class mthdcls947 {
	public static int method947 (int var644, int var785) {
		if (var644>var785)
			return (var644*var785);
		else
			return (var785*var644+1);
	}
}

class mthdcls948 {
	public static int method948 (int var706, int var102) {
		if (var706>var102)
			return (var706+var102);
		else
			return (var102+var706+1);
	}
}

class mthdcls949 {
	public static int method949 (int var793, int var298) {
		if (var793>var298)
			return (var793*var298);
		else
			return (var298*var793+1);
	}
}

class mthdcls950 {
	public static int method950 (int var315, int var718) {
		if (var315>var718)
			return (var315+var718);
		else
			return (var718+var315+1);
	}
}

class mthdcls951 {
	public static int method951 (int var804, int var460) {
		if (var804>var460)
			return (var804+var460);
		else
			return (var460+var804+1);
	}
}

class mthdcls952 {
	public static int method952 (int var549, int var33) {
		if (var549>var33)
			return (var549*var33);
		else
			return (var33*var549+1);
	}
}

class mthdcls953 {
	public static int method953 (int var585, int var927) {
		if (var585>var927)
			return (var585*var927);
		else
			return (var927*var585+1);
	}
}

class mthdcls954 {
	public static int method954 (int var124, int var770) {
		if (var124>var770)
			return (var124+var770);
		else
			return (var770+var124+1);
	}
}

class mthdcls955 {
	public static int method955 (int var754, int var60) {
		if (var754>var60)
			return (var754-var60);
		else
			return (var60-var754+1);
	}
}

class mthdcls956 {
	public static int method956 (int var605, int var393) {
		if (var605>var393)
			return (var605*var393);
		else
			return (var393*var605+1);
	}
}

class mthdcls957 {
	public static int method957 (int var488, int var884) {
		if (var488>var884)
			return (var488*var884);
		else
			return (var884*var488+1);
	}
}

class mthdcls958 {
	public static int method958 (int var214, int var331) {
		if (var214>var331)
			return (var214+var331);
		else
			return (var331+var214+1);
	}
}

class mthdcls959 {
	public static int method959 (int var405, int var365) {
		if (var405>var365)
			return (var405+var365);
		else
			return (var365+var405+1);
	}
}

class mthdcls960 {
	public static int method960 (int var106, int var305) {
		if (var106>var305)
			return (var106-var305);
		else
			return (var305-var106+1);
	}
}

class mthdcls961 {
	public static int method961 (int var30, int var227) {
		if (var30>var227)
			return (var30*var227);
		else
			return (var227*var30+1);
	}
}

class mthdcls962 {
	public static int method962 (int var497, int var398) {
		if (var497>var398)
			return (var497*var398);
		else
			return (var398*var497+1);
	}
}

class mthdcls963 {
	public static int method963 (int var809, int var368) {
		if (var809>var368)
			return (var809*var368);
		else
			return (var368*var809+1);
	}
}

class mthdcls964 {
	public static int method964 (int var503, int var584) {
		if (var503>var584)
			return (var503*var584);
		else
			return (var584*var503+1);
	}
}

class mthdcls965 {
	public static int method965 (int var54, int var416) {
		if (var54>var416)
			return (var54*var416);
		else
			return (var416*var54+1);
	}
}

class mthdcls966 {
	public static int method966 (int var926, int var753) {
		if (var926>var753)
			return (var926-var753);
		else
			return (var753-var926+1);
	}
}

class mthdcls967 {
	public static int method967 (int var897, int var53) {
		if (var897>var53)
			return (var897-var53);
		else
			return (var53-var897+1);
	}
}

class mthdcls968 {
	public static int method968 (int var272, int var393) {
		if (var272>var393)
			return (var272*var393);
		else
			return (var393*var272+1);
	}
}

class mthdcls969 {
	public static int method969 (int var250, int var241) {
		if (var250>var241)
			return (var250*var241);
		else
			return (var241*var250+1);
	}
}

class mthdcls970 {
	public static int method970 (int var889, int var471) {
		if (var889>var471)
			return (var889-var471);
		else
			return (var471-var889+1);
	}
}

class mthdcls971 {
	public static int method971 (int var830, int var772) {
		if (var830>var772)
			return (var830+var772);
		else
			return (var772+var830+1);
	}
}

class mthdcls972 {
	public static int method972 (int var405, int var123) {
		if (var405>var123)
			return (var405*var123);
		else
			return (var123*var405+1);
	}
}

class mthdcls973 {
	public static int method973 (int var467, int var626) {
		if (var467>var626)
			return (var467*var626);
		else
			return (var626*var467+1);
	}
}

class mthdcls974 {
	public static int method974 (int var32, int var9) {
		if (var32>var9)
			return (var32*var9);
		else
			return (var9*var32+1);
	}
}

class mthdcls975 {
	public static int method975 (int var976, int var827) {
		if (var976>var827)
			return (var976-var827);
		else
			return (var827-var976+1);
	}
}

class mthdcls976 {
	public static int method976 (int var508, int var603) {
		if (var508>var603)
			return (var508+var603);
		else
			return (var603+var508+1);
	}
}

class mthdcls977 {
	public static int method977 (int var538, int var539) {
		if (var538>var539)
			return (var538*var539);
		else
			return (var539*var538+1);
	}
}

class mthdcls978 {
	public static int method978 (int var179, int var4) {
		if (var179>var4)
			return (var179-var4);
		else
			return (var4-var179+1);
	}
}

class mthdcls979 {
	public static int method979 (int var948, int var453) {
		if (var948>var453)
			return (var948-var453);
		else
			return (var453-var948+1);
	}
}

class mthdcls980 {
	public static int method980 (int var720, int var492) {
		if (var720>var492)
			return (var720*var492);
		else
			return (var492*var720+1);
	}
}

class mthdcls981 {
	public static int method981 (int var77, int var688) {
		if (var77>var688)
			return (var77-var688);
		else
			return (var688-var77+1);
	}
}

class mthdcls982 {
	public static int method982 (int var884, int var678) {
		if (var884>var678)
			return (var884+var678);
		else
			return (var678+var884+1);
	}
}

class mthdcls983 {
	public static int method983 (int var702, int var463) {
		if (var702>var463)
			return (var702*var463);
		else
			return (var463*var702+1);
	}
}

class mthdcls984 {
	public static int method984 (int var291, int var415) {
		if (var291>var415)
			return (var291+var415);
		else
			return (var415+var291+1);
	}
}

class mthdcls985 {
	public static int method985 (int var824, int var537) {
		if (var824>var537)
			return (var824+var537);
		else
			return (var537+var824+1);
	}
}

class mthdcls986 {
	public static int method986 (int var402, int var836) {
		if (var402>var836)
			return (var402+var836);
		else
			return (var836+var402+1);
	}
}

class mthdcls987 {
	public static int method987 (int var549, int var521) {
		if (var549>var521)
			return (var549*var521);
		else
			return (var521*var549+1);
	}
}

class mthdcls988 {
	public static int method988 (int var125, int var326) {
		if (var125>var326)
			return (var125+var326);
		else
			return (var326+var125+1);
	}
}

class mthdcls989 {
	public static int method989 (int var124, int var922) {
		if (var124>var922)
			return (var124*var922);
		else
			return (var922*var124+1);
	}
}

class mthdcls990 {
	public static int method990 (int var311, int var412) {
		if (var311>var412)
			return (var311-var412);
		else
			return (var412-var311+1);
	}
}

class mthdcls991 {
	public static int method991 (int var715, int var716) {
		if (var715>var716)
			return (var715*var716);
		else
			return (var716*var715+1);
	}
}

class mthdcls992 {
	public static int method992 (int var601, int var808) {
		if (var601>var808)
			return (var601*var808);
		else
			return (var808*var601+1);
	}
}

class mthdcls993 {
	public static int method993 (int var903, int var91) {
		if (var903>var91)
			return (var903-var91);
		else
			return (var91-var903+1);
	}
}

class mthdcls994 {
	public static int method994 (int var366, int var965) {
		if (var366>var965)
			return (var366-var965);
		else
			return (var965-var366+1);
	}
}

class mthdcls995 {
	public static int method995 (int var564, int var424) {
		if (var564>var424)
			return (var564*var424);
		else
			return (var424*var564+1);
	}
}

class mthdcls996 {
	public static int method996 (int var367, int var268) {
		if (var367>var268)
			return (var367-var268);
		else
			return (var268-var367+1);
	}
}

class mthdcls997 {
	public static int method997 (int var748, int var390) {
		if (var748>var390)
			return (var748-var390);
		else
			return (var390-var748+1);
	}
}

class mthdcls998 {
	public static int method998 (int var813, int var61) {
		if (var813>var61)
			return (var813+var61);
		else
			return (var61+var813+1);
	}
}

class mthdcls999 {
	public static int method999 (int var633, int var688) {
		if (var633>var688)
			return (var633*var688);
		else
			return (var688*var633+1);
	}
}

	}
}
