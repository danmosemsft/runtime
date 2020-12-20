//------------------------------------------------------------------
// <copyright file="PhonemeConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using Microsoft.Win32;

namespace System.Speech.Internal
{
    /// <summary>
    /// Summary description for PhonemeConverter.
    /// </summary>
    internal sealed class PhonemeConverter
    {
        //*******************************************************************
        //
        // Constructors
        //
        //*******************************************************************

        #region Constructors

        static PhonemeConverter ()
        {
#if BUILD_PHONEMAP
            BuildPhoneIds (_phoneMapsCompressed);
#endif
            _phoneMaps = DecompressPhoneMaps (_phoneMapsCompressed);
            _upsConverter = new PhonemeConverter (_phoneMaps [0]);
        }

        private PhonemeConverter (PhoneMap phoneMap)
        {
            _phoneMap = phoneMap;
        }

        #endregion

        //*******************************************************************
        //
        // Internal methods
        //
        //*******************************************************************

        #region Internal methods

        /// <summary>
        /// Returns the cached version of the universal phone converter.
        /// </summary>
        /// <returns></returns>
        static internal PhonemeConverter UpsConverter
        {
            get
            {
                return _upsConverter;
            }
        }

        /// <summary>
        /// Convert a pronunciation string to code points
        /// </summary>
        /// <param name="pronunciation">pronunciation</param>
        /// <param name="lcid"></param>
        /// <returns></returns>
        static internal string ConvertPronToId (string pronunciation, int lcid)
        {
            //CfgGrammar.TraceInformation ("BackEnd::ConvertPronToId");
            PhonemeConverter phoneConv = UpsConverter;
            foreach (PhoneMap phoneMap in _phoneMaps)
            {
                if (phoneMap._lcid == lcid)
                {
                    phoneConv = new PhonemeConverter (phoneMap);
                }
            }

            string phonemes = phoneConv.ConvertPronToId (pronunciation);
            if (string.IsNullOrEmpty (phonemes))
            {
                throw new FormatException (SR.Get (SRID.EmptyPronunciationString));
            }
            return phonemes;
        }

        /// <summary>
        /// Convert an internal phone string to Id code string
        /// The internal phones are space separated and may have a space
        /// at the end.
        /// </summary>
        /// <param name="sPhone"></param>
        /// <returns></returns>
        internal string ConvertPronToId (string sPhone)    // Internal phone string
        {
            //CfgGrammar.TraceInformation ("CSpPhoneConverter::PhoneToId");
            // remove the white spaces
            sPhone = sPhone.Trim (Helpers._achTrimChars);

            // Empty Phoneme string
            if (string.IsNullOrEmpty (sPhone))
            {
                return string.Empty;
            }

            int iPos = 0, iPosNext;
            int cLen = sPhone.Length;
            StringBuilder pidArray = new StringBuilder (cLen);
            PhoneId phoneIdRef = new PhoneId ();

            while (iPos < cLen)
            {
                iPosNext = sPhone.IndexOf (' ', iPos + 1);
                if (iPosNext < 0)
                {
                    iPosNext = cLen;
                }

                string sCur = sPhone.Substring (iPos, iPosNext - iPos);
                string sCurUpper = sCur.ToUpperInvariant ();

                // Search for this phone
                phoneIdRef._phone = sCurUpper;
                int index = Array.BinarySearch<PhoneId> (_phoneMap._phoneIds, phoneIdRef, (IComparer<PhoneId>) phoneIdRef);
                if (index >= 0)
                {
                    foreach (char ch in _phoneMap._phoneIds [index]._cp)
                    {
                        pidArray.Append (ch);
                    }
                }
                else
                {
                    // phoneme not found error out
                    throw new FormatException (SR.Get (SRID.InvalidPhoneme, sCur));
                }

                iPos = iPosNext;

                // skip over the spaces
                while (iPos < cLen && sPhone [iPos] == ' ')
                {
                    iPos++;
                }
            }

            return pidArray.ToString ();
        } /* CSpPhoneConverter::PhoneToId */

        static internal void ValidateUpsIds (string ids)
        {
            ValidateUpsIds (ids.ToCharArray ());
        }

        static internal void ValidateUpsIds (char [] ids)
        {
            foreach (char id in ids)
            {
                if (Array.BinarySearch (_updIds, id) < 0)
                {
                    throw new FormatException (SR.Get (SRID.InvalidPhoneme, id));
                }
            }
        }

        #endregion

        //*******************************************************************
        //
        // Private Methods
        //
        //*******************************************************************

        #region Private Methods

        /// <summary>
        /// Builds the Phoneme maps from the compressed form.
        /// </summary>
        /// <param name="pmComps"></param>
        /// <returns></returns>
        static private PhoneMap [] DecompressPhoneMaps (PhoneMapCompressed [] pmComps)
        {
            PhoneMap [] phoneMaps = new PhoneMap [pmComps.Length];

            // Build the phoneme maps
            for (int i = 0; i < pmComps.Length; i++)
            {
                PhoneMapCompressed pmCompressed = pmComps [i];
                PhoneMap pm = phoneMaps [i] = new PhoneMap ();
                pm._lcid = pmCompressed._lcid;
                pm._phoneIds = new PhoneId [pmCompressed._count];

                int posPhone = 0;
                int posCp = 0;
                for (int j = 0; j < pm._phoneIds.Length; j++)
                {
                    pm._phoneIds [j] = new PhoneId ();
                    // Count the number of chars in the phoneme string
                    int lastPhone;
                    int multi_phones = 0;
                    for (lastPhone = posPhone; pmCompressed._phones [lastPhone] != 0; lastPhone++)
                    {
                        // All phoneme code points are assumed to be of length == 1
                        // if the lenght is greater, then a marker of -1 is set for each additional code points
                        if (pmCompressed._phones [lastPhone] == unchecked ((byte) -1))
                        {
                            multi_phones++;
                        }
                    }

                    // Build the phoneme string
                    int strLen = lastPhone - posPhone - multi_phones;
                    char [] phone = new char [strLen];
                    for (int l = 0; l < strLen; l++)
                    {
                        phone [l] = (char) pmCompressed._phones [posPhone++];
                    }

                    // Update the index for the next phoneme string
                    posPhone += multi_phones + 1;

                    // Copy the code points for this phoneme
                    pm._phoneIds [j]._phone = new string (phone);
                    pm._phoneIds [j]._cp = new char [multi_phones + 1];
                    for (int l = 0; l < pm._phoneIds [j]._cp.Length; l++)
                    {
                        pm._phoneIds [j]._cp [l] = pmCompressed._cps [posCp++];
                    }
                }

                // Ensure that the table is built properly
                System.Diagnostics.Debug.Assert (posPhone == pmCompressed._phones.Length);
                System.Diagnostics.Debug.Assert (posCp == pmCompressed._cps.Length);
            }
            return phoneMaps;
        }

        // Do not delete generation of the phone conversion table from the registry entries

#if BUILD_PHONEMAP

        private static string BuildPhoneIds (PhoneMapCompressed [] _compressed)
        {
            string uni = "I 0069 Y 0079 IX 0268 YX 0289 UU 026F U 0075 IH 026A YH 028F UH 028A E 0065 EU 00F8 EX 0258 OX 0275 OU 0264 O 006F AX 0259 EH 025B OE 0153 ER 025C UR 025E AH 028C AO 0254 AE 00E6 AEX 0250 A 0061 AOE 0276 AA 0251 Q 0252 EI 006503610069 AU 00610361028A OI 025403610069 AI 006103610069 IYX 006903610259 UYX 007903610259 EHX 025B03610259 UWX 007503610259 OWX 006F03610259 AOX 025403610259 EN 00650303 AN 00610303 ON 006F0303 OEN 01530303 P 0070 B 0062 M 006D BB 0299 PH 0278 BH 03B2 MF 0271 F 0066 V 0076 VA 028B TH 03B8 DH 00F0 T 0074 D 0064 N 006E RR 0072 DX 027E S 0073 Z 007A LSH 026C LH 026E RA 0279 L 006C SH 0283 ZH 0292 TR 0288 DR 0256 NR 0273 DXR 027D SR 0282 ZR 0290 R 027B LR 026D CT 0063 JD 025F NJ 0272 C 00E7 CJ 029D J 006A LJ 028E W 0077 K 006B G 0067 NG 014B X 0078 GH 0263 GA 0270 GL 029F QT 0071 QD 0262 QN 0274 QQ 0280 QH 03C7 RH 0281 HH 0127 HG 0295 GT 0294 H 0068 WJ 0265 PF 007003610066 TS 007403610073 CH 007403610283 JH 006403610292 JJ 006A0361006A DZ 00640361007A CC 074003610255 JC 006403610291 TSR 007403610282 WH 028D ESH 029C EZH 02A2 ET 02A1 SC 0255 ZC 0291 LT 027A SHX 0267 HZ 0266 PCK 0298 TCK 01C0 NCK 0021 CCK 01C2 LCK 01C1 BIM 0253 DIM 0257 QIM 029B GIM 0260 JIM 0284 S1 02C8 S2 02CC . 002E _| 007C _|| 2016 lng 02D0 hlg 02D1 xsh 02D8 _^ 203F _! 0001 _& 0002 _, 0003 _s 0004 _. 2198 _? 2197 T5 030B T4 0301 T3 0304 T2 0300 T1 030F T- 2193 T+ 2191 vls 030A vcd 032C bvd 0324 cvd 0330 asp 02B0 mrd 0339 lrd 031C adv 031F ret 0331 cen 0308 mcn 033D syl 0329 nsy 032F rho 02DE lla 033C lab 02B7 pal 02B2 vel 02E0 phr 02E4 vph 0334 rai 031D low 031E atr 0318 rtr 0319 den 032A api 033A lam 033B nas 0303 nsr 207F lar 02E1 nar 031A ejc 02BC + 0361 bva 02B1 G2 0261 rte 0320 vsl 0325 NCK3 0297 NCK2 01C3 LCK2 0296 TCK2 0287 JC2 02A5 CC2 02A8 LG 026B DZ2 02A3 TS2 02A6 JH2 02A4 CH2 02A7 SHC 0286 rhz 02B4 QOM 02A0 xst 0306 T= 2192 ERR 025D AXR 025A ZHJ 0293";
            string chinese = "- 0001 ! 0002 & 0003 , 0004 . 0005 ? 0006 _ 0007 + 0008 * 0009 1 000A 2 000B 3 000C 4 000D 5 000E a 000F ai 0010 an 0011 ang 0012 ao 0013 ba 0014 bai 0015 ban 0016 bang 0017 bao 0018 bei 0019 ben 001A beng 001B bi 001C bian 001D biao 001E bie 001F bin 0020 bing 0021 bo 0022 bu 0023 ca 0024 cai 0025 can 0026 cang 0027 cao 0028 ce 0029 cen 002A ceng 002B cha 002C chai 002D chan 002E chang 002F chao 0030 che 0031 chen 0032 cheng 0033 chi 0034 chong 0035 chou 0036 chu 0037 chuai 0038 chuan 0039 chuang 003A chui 003B chun 003C chuo 003D ci 003E cong 003F cou 0040 cu 0041 cuan 0042 cui 0043 cun 0044 cuo 0045 da 0046 dai 0047 dan 0048 dang 0049 dao 004A de 004B dei 004C den 004D deng 004E di 004F dia 0050 dian 0051 diao 0052 die 0053 ding 0054 diu 0055 dong 0056 dou 0057 du 0058 duan 0059 dui 005A dun 005B duo 005C e 005D ei 005E en 005F er 0060 fa 0061 fan 0062 fang 0063 fei 0064 fen 0065 feng 0066 fo 0067 fou 0068 fu 0069 ga 006A gai 006B gan 006C gang 006D gao 006E ge 006F gei 0070 gen 0071 geng 0072 gong 0073 gou 0074 gu 0075 gua 0076 guai 0077 guan 0078 guang 0079 gui 007A gun 007B guo 007C ha 007D hai 007E han 007F hang 0080 hao 0081 he 0082 hei 0083 hen 0084 heng 0085 hong 0086 hou 0087 hu 0088 hua 0089 huai 008A huan 008B huang 008C hui 008D hun 008E huo 008F ji 0090 jia 0091 jian 0092 jiang 0093 jiao 0094 jie 0095 jin 0096 jing 0097 jiong 0098 jiu 0099 ju 009A juan 009B jue 009C jun 009D ka 009E kai 009F kan 00A0 kang 00A1 kao 00A2 ke 00A3 kei 00A4 ken 00A5 keng 00A6 kong 00A7 kou 00A8 ku 00A9 kua 00AA kuai 00AB kuan 00AC kuang 00AD kui 00AE kun 00AF kuo 00B0 la 00B1 lai 00B2 lan 00B3 lang 00B4 lao 00B5 le 00B6 lei 00B7 leng 00B8 li 00B9 lia 00BA lian 00BB liang 00BC liao 00BD lie 00BE lin 00BF ling 00C0 liu 00C1 lo 00C2 long 00C3 lou 00C4 lu 00C5 luan 00C6 lue 00C7 lun 00C8 luo 00C9 lv 00CA ma 00CB mai 00CC man 00CD mang 00CE mao 00CF me 00D0 mei 00D1 men 00D2 meng 00D3 mi 00D4 mian 00D5 miao 00D6 mie 00D7 min 00D8 ming 00D9 miu 00DA mo 00DB mou 00DC mu 00DD na 00DE nai 00DF nan 00E0 nang 00E1 nao 00E2 ne 00E3 nei 00E4 nen 00E5 neng 00E6 ni 00E7 nian 00E8 niang 00E9 niao 00EA nie 00EB nin 00EC ning 00ED niu 00EE nong 00EF nou 00F0 nu 00F1 nuan 00F2 nue 00F3 nuo 00F4 nv 00F5 o 00F6 ou 00F7 pa 00F8 pai 00F9 pan 00FA pang 00FB pao 00FC pei 00FD pen 00FE peng 00FF pi 0100 pian 0101 piao 0102 pie 0103 pin 0104 ping 0105 po 0106 pou 0107 pu 0108 qi 0109 qia 010A qian 010B qiang 010C qiao 010D qie 010E qin 010F qing 0110 qiong 0111 qiu 0112 qu 0113 quan 0114 que 0115 qun 0116 ran 0117 rang 0118 rao 0119 re 011A ren 011B reng 011C ri 011D rong 011E rou 011F ru 0120 ruan 0121 rui 0122 run 0123 ruo 0124 sa 0125 sai 0126 san 0127 sang 0128 sao 0129 se 012A sen 012B seng 012C sha 012D shai 012E shan 012F shang 0130 shao 0131 she 0132 shei 0133 shen 0134 sheng 0135 shi 0136 shou 0137 shu 0138 shua 0139 shuai 013A shuan 013B shuang 013C shui 013D shun 013E shuo 013F si 0140 song 0141 sou 0142 su 0143 suan 0144 sui 0145 sun 0146 suo 0147 ta 0148 tai 0149 tan 014A tang 014B tao 014C te 014D tei 014E teng 014F ti 0150 tian 0151 tiao 0152 tie 0153 ting 0154 tong 0155 tou 0156 tu 0157 tuan 0158 tui 0159 tun 015A tuo 015B wa 015C wai 015D wan 015E wang 015F wei 0160 wen 0161 weng 0162 wo 0163 wu 0164 xi 0165 xia 0166 xian 0167 xiang 0168 xiao 0169 xie 016A xin 016B xing 016C xiong 016D xiu 016E xu 016F xuan 0170 xue 0171 xun 0172 ya 0173 yan 0174 yang 0175 yao 0176 ye 0177 yi 0178 yin 0179 ying 017A yo 017B yong 017C you 017D yu 017E yuan 017F yue 0180 yun 0181 za 0182 zai 0183 zan 0184 zang 0185 zao 0186 ze 0187 zei 0188 zen 0189 zeng 018A zha 018B zhai 018C zhan 018D zhang 018E zhao 018F zhe 0190 zhei 0191 zhen 0192 zheng 0193 zhi 0194 zhong 0195 zhou 0196 zhu 0197 zhua 0198 zhuai 0199 zhuan 019A zhuang 019B zhui 019C zhun 019D zhuo 019E zi 019F zong 01A0 zou 01A1 zu 01A2 zuan 01A3 zui 01A4 zun 01A5 zuo 01A6";
            string english = "- 0001 ! 0002 & 0003 , 0004 . 0005 ? 0006 _ 0007 1 0008 2 0009 aa 000a ae 000b ah 000c ao 000d aw 000e ax 000f ay 0010 b 0011 ch 0012 d 0013 dh 0014 eh 0015 er 0016 ey 0017 f 0018 g 0019 h 001a ih 001b iy 001c jh 001d k 001e l 001f m 0020 n 0021 ng 0022 ow 0023 oy 0024 p 0025 r 0026 s 0027 sh 0028 t 0029 th 002a uh 002b uw 002c v 002d w 002e y 002f z 0030 zh 0031";
            string french = "- 0001 ! 0002 & 0003 , 0004 . 0005 ? 0006 _ 0007 1 0008 ~ 0009 aa 000a a 000b oh 000c ax 000d b 000e d 000f eh 0010 ey 0011 f 0012 g 0013 hy 0014 uy 0015 iy 0016 k 0017 l 0018 m 0019 n 001a ng 001b nj 001c oe 001d eu 001e ow 001f p 0020 r 0021 s 0022 sh 0023 t 0024 uw 0025 v 0026 w 0027 y 0028 z 0029 zh 002a";
            string german = "- 0001 ! 0002 & 0003 , 0004 . 0005 ? 0006 _ 0007 ^ 0008 1 0009 2 000a ~ 000b : 000c a 000d aw 000e ax 000f ay 0010 b 0011 d 0012 ch 0013 eh 0014 eu 0015 ey 0016 f 0017 g 0018 h 0019 ih 001a iy 001b jh 001c k 001d l 001e m 001f n 0020 ng 0021 oe 0022 oh 0023 ow 0024 oy 0025 p 0026 pf 0027 r 0028 s 0029 sh 002a t 002b ts 002c ue 002d uh 002e uw 002f uy 0030 v 0031 x 0032 y 0033 z 0034 zh 0035";
            string japanese = "309C 309C 30A1 30A1 30A2 30A2 30A3 30A3 30A4 30A4 30A5 30A5 30A6 30A6 30A7 30A7 30A8 30A8 30A9 30A9 30AA 30AA 30AB 30AB 30AC 30AC 30AD 30AD 30AE 30AE 30AF 30AF 30B0 30B0 30B1 30B1 30B2 30B2 30B3 30B3 30B4 30B4 30B5 30B5 30B6 30B6 30B7 30B7 30B8 30B8 30B9 30B9 30BA 30BA 30BB 30BB 30BC 30BC 30BD 30BD 30BE 30BE 30BF 30BF 30C0 30C0 30C1 30C1 30C2 30C2 30C3 30C3 30C4 30C4 30C5 30C5 30C6 30C6 30C7 30C7 30C8 30C8 30C9 30C9 30CA 30CA 30CB 30CB 30CC 30CC 30CD 30CD 30CE 30CE 30CF 30CF 30D0 30D0 30D1 30D1 30D2 30D2 30D3 30D3 30D4 30D4 30D5 30D5 30D6 30D6 30D7 30D7 30D8 30D8 30D9 30D9 30DA 30DA 30DB 30DB 30DC 30DC 30DD 30DD 30DE 30DE 30DF 30DF 30E0 30E0 30E1 30E1 30E2 30E2 30E3 30E3 30E4 30E4 30E5 30E5 30E6 30E6 30E7 30E7 30E8 30E8 30E9 30E9 30EA 30EA 30EB 30EB 30EC 30EC 30ED 30ED 30EE 30EE 30EF 30EF 30F0 30F0 30F1 30F1 30F2 30F2 30F3 30F3 30F4 30F4 30F5 30F5 30F6 30F6 30F7 30F7 30F8 30F8 30F9 30F9 30FA 30FA 30FB 30FB 30FC 30FC 30FD 30FD 30FE 30FE 0021 0021 0027 0027 002B 002B 002E 002E 003F 003F 005F 005F 007C 007C";
            string spanish = "- 0001 ! 0002 & 0003 , 0004 . 0005 ? 0006 _ 0007 1 0008 2 0009 a 000a e 000b i 000c o 000d u 000e t 000f d 0010 p 0011 b 0012 k 0013 g 0014 ch 0015 jj 0016 f 0017 s 0018 x 0019 m 001a n 001b nj 001c l 001d ll 001e r 001f rr 0020 j 0021 w 0022 th 0023";
            string mandarin = "002D 002D 0021 0021 0026 0026 002C 002C 002E 002E 003F 003F 005F 005F 002B 002B 002A 002A 02C9 02C9 02CA 02CA 02C7 02C7 02CB 02CB 02D9 02D9 3000 3000 3105 3105 3106 3106 3107 3107 3108 3108 3109 3109 310A 310A 310B 310B 310C 310C 310D 310D 310E 310E 310F 310F 3110 3110 3111 3111 3112 3112 3113 3113 3114 3114 3115 3115 3116 3116 3117 3117 3118 3118 3119 3119 3127 3127 3128 3128 3129 3129 311A 311A 311B 311B 311C 311C 311D 311D 311E 311E 311F 311F 3120 3120 3121 3121 3122 3122 3123 3123 3124 3124 3125 3125 3126 3126";

            PhoneMap [] phoneMaps = new PhoneMap [8];
            phoneMaps [0] = new PhoneMap (0, null);
            string s0 = BuildPhoneTable (uni, out phoneMaps [0]._phoneIds);
            phoneMaps [1] = new PhoneMap (0x404, null);
            string s404 = BuildPhoneTable (mandarin, out phoneMaps [1]._phoneIds);
            phoneMaps [2] = new PhoneMap (0x407, null);
            string s407 = BuildPhoneTable (german, out phoneMaps [2]._phoneIds);
            phoneMaps [3] = new PhoneMap (0x409, null);
            string s409 = BuildPhoneTable (english, out phoneMaps [3]._phoneIds);
            phoneMaps [4] = new PhoneMap (0x40A, null);
            string s40A = BuildPhoneTable (spanish, out phoneMaps [4]._phoneIds);
            phoneMaps [5] = new PhoneMap (0x40C, null);
            string s40C = BuildPhoneTable (french, out phoneMaps [5]._phoneIds);
            phoneMaps [6] = new PhoneMap (0x411, null);
            string s411 = BuildPhoneTable (japanese, out phoneMaps [6]._phoneIds);
            phoneMaps [7] = new PhoneMap (0x804, null);
            string s804 = BuildPhoneTable (chinese, out phoneMaps [7]._phoneIds);

            CheckPhoneMaps (_phoneMapsRef, phoneMaps, false);

            StringBuilder sb = new StringBuilder ();
            sb.Append ("PhoneMapCompressed [] _phoneMaps = new PhoneMapCompressed [] {\n");

            BuildPhoneIdsEntry ("0", s0, sb, ",");
            BuildPhoneIdsEntry ("404", s404, sb, ",");
            BuildPhoneIdsEntry ("407", s407, sb, ",");
            BuildPhoneIdsEntry ("409", s409, sb, ",");
            BuildPhoneIdsEntry ("40A", s40A, sb, ",");
            BuildPhoneIdsEntry ("40C", s40C, sb, ",");
            BuildPhoneIdsEntry ("411", s411, sb, ",");
            BuildPhoneIdsEntry ("804", s804, sb, ",");

            sb.Append (" };\n");

            sb.Append ("static char [] _updIds = new char [] { ");
            sb.Append (BuildUpsIds (uni));
            sb.Append (" };\n");

            PhoneMap [] decompressPhoneMaps = DecompressPhoneMaps (_compressed);
            CheckPhoneMaps (phoneMaps, decompressPhoneMaps, true);

            return sb.ToString ();
        }

        private static void CheckPhoneMaps (PhoneMap [] phoneMapsRef, PhoneMap [] phoneMaps, bool checkGerman)
        {
            // Compare the phoneme maps
            for (int i = 0; i < phoneMaps.Length; i++)
            {
                PhoneMap pm1 = phoneMapsRef [i];
                PhoneMap pm2 = phoneMaps [i];

                if (checkGerman || pm1._lcid != 0x407)
                {
                    System.Diagnostics.Debug.Assert (pm1._lcid == pm2._lcid);
                    System.Diagnostics.Debug.Assert (pm1._phoneIds.Length == pm2._phoneIds.Length);
                    for (int j = 0; j < pm1._phoneIds.Length; j++)
                    {
                        System.Diagnostics.Debug.Assert (pm1._phoneIds [j]._phone == pm2._phoneIds [j]._phone);
                        System.Diagnostics.Debug.Assert (pm1._phoneIds [j]._cp.Length == pm2._phoneIds [j]._cp.Length);
                        for (int k = 0; k < pm1._phoneIds [j]._cp.Length; k++)
                        {
                            System.Diagnostics.Debug.Assert (pm1._phoneIds [j]._cp [k] == pm2._phoneIds [j]._cp [k]);
                        }
                    }
                }
            }
        }

        private static void BuildPhoneIdsEntry (string lcid, string phoneIds, StringBuilder sb, string comma)
        {
            sb.Append ("new PhoneMapCompressed ( 0x");
            sb.Append (lcid);
            sb.Append (", ");
            sb.Append (phoneIds);
            sb.Append (")");
            sb.Append (comma);
            sb.Append ("\n");
        }

        private static string BuildPhoneTable (string uni, out PhoneId [] ph)
        {
            string [] halves = uni.Split (new char[] { }, StringSplitOptions.RemoveEmptyEntries);
            ph = new PhoneId [halves.Length / 2];
            for (int i = 0; i < halves.Length / 2; i++)
            {
                ph [i] = new PhoneId ();
                ph [i]._phone = halves [i * 2].ToUpper ();

                int cTokens = halves [i * 2 + 1].Length / 4;
                ph [i]._cp = new char [cTokens];

                for (int j = 0; j < cTokens; j++)
                {
                    ph [i]._cp [j] = (char) short.Parse (halves [i * 2 + 1].Substring (j * 4, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
            }
            // Build the array of codepoints for ups
            Array.Sort<PhoneId> (ph, (IComparer<PhoneId>) ph [0]);

            StringBuilder sbPhones = new StringBuilder ();
            StringBuilder sbCodePoints = new StringBuilder ();
            sbPhones.Append ("new byte [] {");
            sbCodePoints.Append ("new char [] {");
            for (int i = 0; i < ph.Length; i++)
            {
                if (i > 0)
                {
                    sbPhones.Append (", ");
                    sbCodePoints.Append (", ");
                }

                foreach (char ch in ph [i]._phone)
                {
                    System.Diagnostics.Debug.Assert ((short) ch > 0 && (short) ch < 128);
                    sbPhones.Append ((byte) ch);
                    sbPhones.Append (", ");
                }
                for (int j = 1; j < ph [i]._cp.Length; j++)
                {
                    sbPhones.Append (unchecked ((byte) -1));
                    sbPhones.Append (", ");
                }
                sbPhones.Append ((byte) 0);

                for (int j = 0; j < ph [i]._cp.Length; j++)
                {
                    if (j > 0)
                    {
                        sbCodePoints.Append (", ");
                    }
                    sbCodePoints.Append ("(char) ");
                    sbCodePoints.Append ((short) ph [i]._cp [j]);
                }
            }

            sbPhones.Append ("}");
            sbCodePoints.Append ("}");
            return ph.Length.ToString () + ", " + sbPhones.ToString () + ", " + sbCodePoints.ToString ();
        }

        static string BuildUpsIds (string uni)
        {
            string [] halves = uni.Split (new char[] { }, StringSplitOptions.RemoveEmptyEntries);
            PhoneId [] ph = new PhoneId [halves.Length / 2];
            for (int i = 0; i < halves.Length / 2; i++)
            {
                ph [i] = new PhoneId ();
                ph [i]._phone = halves [i * 2].ToUpper ();

                int cTokens = halves [i * 2 + 1].Length / 4;
                ph [i]._cp = new char [cTokens];

                for (int j = 0; j < cTokens; j++)
                {
                    ph [i]._cp [j] = (char) short.Parse (halves [i * 2 + 1].Substring (j * 4, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
                }
            }

            List<char> _upsValue = new List<char> ();
            foreach (PhoneId phoneId in ph)
            {
                foreach (char id in phoneId._cp)
                {
                    if (!_upsValue.Contains (id))
                    {
                        _upsValue.Add (id);
                    }
                }
            }
            _upsValue.Sort ();

            StringBuilder sb = new StringBuilder ();
            foreach (char id in _upsValue)
            {
                if (sb.Length > 0)
                {
                    sb.Append (", ");
                }
                sb.Append ("(char) ");
                sb.Append ((int) id);
            }
            return sb.ToString ();
        }

        private static readonly PhoneMap [] _phoneMapsRef = new PhoneMap [] 
        {
            new PhoneMap ( 0x0, new PhoneId [] {new PhoneId (".", new char [] { (char) 46}), new PhoneId ("_!", new char [] { (char) 1}), new PhoneId ("_&", new char [] { (char) 2}), new PhoneId ("_,", new char [] { (char) 3}), new PhoneId ("_.", new char [] { (char) 8600}), new PhoneId ("_?", new char [] { (char) 8599}), new PhoneId ("_^", new char [] { (char) 8255}), new PhoneId ("_|", new char [] { (char) 124}), new PhoneId ("_||", new char [] { (char) 8214}), new PhoneId ("_S", new char [] { (char) 4}), new PhoneId ("+", new char [] { (char) 865}), new PhoneId ("A", new char [] { (char) 97}), new PhoneId ("AA", new char [] { (char) 593}), new PhoneId ("ADV", new char [] { (char) 799}), new PhoneId ("AE", new char [] { (char) 230}), new PhoneId ("AEX", new char [] { (char) 592}), new PhoneId ("AH", new char [] { (char) 652}), new PhoneId ("AI", new char [] { (char) 97, (char) 865, (char) 105}), new PhoneId ("AN", new char [] { (char) 97, (char) 771}), new PhoneId ("AO", new char [] { (char) 596}), new PhoneId ("AOE", new char [] { (char) 630}), new PhoneId ("AOX", new char [] { (char) 596, (char) 865, (char) 601}), new PhoneId ("API", new char [] { (char) 826}), new PhoneId ("ASP", new char [] { (char) 688}), new PhoneId ("ATR", new char [] { (char) 792}), new PhoneId ("AU", new char [] { (char) 97, (char) 865, (char) 650}), new PhoneId ("AX", new char [] { (char) 601}), new PhoneId ("AXR", new char [] { (char) 602}), new PhoneId ("B", new char [] { (char) 98}), new PhoneId ("BB", new char [] { (char) 665}), new PhoneId ("BH", new char [] { (char) 946}), new PhoneId ("BIM", new char [] { (char) 595}), new PhoneId ("BVA", new char [] { (char) 689}), new PhoneId ("BVD", new char [] { (char) 804}), new PhoneId ("C", new char [] { (char) 231}), new PhoneId ("CC", new char [] { (char) 1856, (char) 865, (char) 597}), new PhoneId ("CC2", new char [] { (char) 680}), new PhoneId ("CCK", new char [] { (char) 450}), new PhoneId ("CEN", new char [] { (char) 776}), new PhoneId ("CH", new char [] { (char) 116, (char) 865, (char) 643}), new PhoneId ("CH2", new char [] { (char) 679}), new PhoneId ("CJ", new char [] { (char) 669}), new PhoneId ("CT", new char [] { (char) 99}), new PhoneId ("CVD", new char [] { (char) 816}), new PhoneId ("D", new char [] { (char) 100}), new PhoneId ("DEN", new char [] { (char) 810}), new PhoneId ("DH", new char [] { (char) 240}), new PhoneId ("DIM", new char [] { (char) 599}), new PhoneId ("DR", new char [] { (char) 598}), new PhoneId ("DX", new char [] { (char) 638}), new PhoneId ("DXR", new char [] { (char) 637}), new PhoneId ("DZ", new char [] { (char) 100, (char) 865, (char) 122}), new PhoneId ("DZ2", new char [] { (char) 675}), new PhoneId ("E", new char [] { (char) 101}), new PhoneId ("EH", new char [] { (char) 603}), new PhoneId ("EHX", new char [] { (char) 603, (char) 865, (char) 601}), new PhoneId ("EI", new char [] { (char) 101, (char) 865, (char) 105}), new PhoneId ("EJC", new char [] { (char) 700}), new PhoneId ("EN", new char [] { (char) 101, (char) 771}), new PhoneId ("ER", new char [] { (char) 604}), new PhoneId ("ERR", new char [] { (char) 605}), new PhoneId ("ESH", new char [] { (char) 668}), new PhoneId ("ET", new char [] { (char) 673}), new PhoneId ("EU", new char [] { (char) 248}), new PhoneId ("EX", new char [] { (char) 600}), new PhoneId ("EZH", new char [] { (char) 674}), new PhoneId ("F", new char [] { (char) 102}), new PhoneId ("G", new char [] { (char) 103}), new PhoneId ("G2", new char [] { (char) 609}), new PhoneId ("GA", new char [] { (char) 624}), new PhoneId ("GH", new char [] { (char) 611}), new PhoneId ("GIM", new char [] { (char) 608}), new PhoneId ("GL", new char [] { (char) 671}), new PhoneId ("GT", new char [] { (char) 660}), new PhoneId ("H", new char [] { (char) 104}), new PhoneId ("HG", new char [] { (char) 661}), new PhoneId ("HH", new char [] { (char) 295}), new PhoneId ("HLG", new char [] { (char) 721}), new PhoneId ("HZ", new char [] { (char) 614}), new PhoneId ("I", new char [] { (char) 105}), new PhoneId ("IH", new char [] { (char) 618}), new PhoneId ("IX", new char [] { (char) 616}), new PhoneId ("IYX", new char [] { (char) 105, (char) 865, (char) 601}), new PhoneId ("J", new char [] { (char) 106}), new PhoneId ("JC", new char [] { (char) 100, (char) 865, (char) 657}), new PhoneId ("JC2", new char [] { (char) 677}), new PhoneId ("JD", new char [] { (char) 607}), new PhoneId ("JH", new char [] { (char) 100, (char) 865, (char) 658}), new PhoneId ("JH2", new char [] { (char) 676}), new PhoneId ("JIM", new char [] { (char) 644}), new PhoneId ("JJ", new char [] { (char) 106, (char) 865, (char) 106}), new PhoneId ("K", new char [] { (char) 107}), new PhoneId ("L", new char [] { (char) 108}), new PhoneId ("LAB", new char [] { (char) 695}), new PhoneId ("LAM", new char [] { (char) 827}), new PhoneId ("LAR", new char [] { (char) 737}), new PhoneId ("LCK", new char [] { (char) 449}), new PhoneId ("LCK2", new char [] { (char) 662}), new PhoneId ("LG", new char [] { (char) 619}), new PhoneId ("LH", new char [] { (char) 622}), new PhoneId ("LJ", new char [] { (char) 654}), new PhoneId ("LLA", new char [] { (char) 828}), new PhoneId ("LNG", new char [] { (char) 720}), new PhoneId ("LOW", new char [] { (char) 798}), new PhoneId ("LR", new char [] { (char) 621}), new PhoneId ("LRD", new char [] { (char) 796}), new PhoneId ("LSH", new char [] { (char) 620}), new PhoneId ("LT", new char [] { (char) 634}), new PhoneId ("M", new char [] { (char) 109}), new PhoneId ("MCN", new char [] { (char) 829}), new PhoneId ("MF", new char [] { (char) 625}), new PhoneId ("MRD", new char [] { (char) 825}), new PhoneId ("N", new char [] { (char) 110}), new PhoneId ("NAR", new char [] { (char) 794}), new PhoneId ("NAS", new char [] { (char) 771}), new PhoneId ("NCK", new char [] { (char) 33}), new PhoneId ("NCK2", new char [] { (char) 451}), new PhoneId ("NCK3", new char [] { (char) 663}), new PhoneId ("NG", new char [] { (char) 331}), new PhoneId ("NJ", new char [] { (char) 626}), new PhoneId ("NR", new char [] { (char) 627}), new PhoneId ("NSR", new char [] { (char) 8319}), new PhoneId ("NSY", new char [] { (char) 815}), new PhoneId ("O", new char [] { (char) 111}), new PhoneId ("OE", new char [] { (char) 339}), new PhoneId ("OEN", new char [] { (char) 339, (char) 771}), new PhoneId ("OI", new char [] { (char) 596, (char) 865, (char) 105}), new PhoneId ("ON", new char [] { (char) 111, (char) 771}), new PhoneId ("OU", new char [] { (char) 612}), new PhoneId ("OWX", new char [] { (char) 111, (char) 865, (char) 601}), new PhoneId ("OX", new char [] { (char) 629}), new PhoneId ("P", new char [] { (char) 112}), new PhoneId ("PAL", new char [] { (char) 690}), new PhoneId ("PCK", new char [] { (char) 664}), new PhoneId ("PF", new char [] { (char) 112, (char) 865, (char) 102}), new PhoneId ("PH", new char [] { (char) 632}), new PhoneId ("PHR", new char [] { (char) 740}), new PhoneId ("Q", new char [] { (char) 594}), new PhoneId ("QD", new char [] { (char) 610}), new PhoneId ("QH", new char [] { (char) 967}), new PhoneId ("QIM", new char [] { (char) 667}), new PhoneId ("QN", new char [] { (char) 628}), new PhoneId ("QOM", new char [] { (char) 672}), new PhoneId ("QQ", new char [] { (char) 640}), new PhoneId ("QT", new char [] { (char) 113}), new PhoneId ("R", new char [] { (char) 635}), new PhoneId ("RA", new char [] { (char) 633}), new PhoneId ("RAI", new char [] { (char) 797}), new PhoneId ("RET", new char [] { (char) 817}), new PhoneId ("RH", new char [] { (char) 641}), new PhoneId ("RHO", new char [] { (char) 734}), new PhoneId ("RHZ", new char [] { (char) 692}), new PhoneId ("RR", new char [] { (char) 114}), new PhoneId ("RTE", new char [] { (char) 800}), new PhoneId ("RTR", new char [] { (char) 793}), new PhoneId ("S", new char [] { (char) 115}), new PhoneId ("S1", new char [] { (char) 712}), new PhoneId ("S2", new char [] { (char) 716}), new PhoneId ("SC", new char [] { (char) 597}), new PhoneId ("SH", new char [] { (char) 643}), new PhoneId ("SHC", new char [] { (char) 646}), new PhoneId ("SHX", new char [] { (char) 615}), new PhoneId ("SR", new char [] { (char) 642}), new PhoneId ("SYL", new char [] { (char) 809}), new PhoneId ("T", new char [] { (char) 116}), new PhoneId ("T-", new char [] { (char) 8595}), new PhoneId ("T+", new char [] { (char) 8593}), new PhoneId ("T=", new char [] { (char) 8594}), new PhoneId ("T1", new char [] { (char) 783}), new PhoneId ("T2", new char [] { (char) 768}), new PhoneId ("T3", new char [] { (char) 772}), new PhoneId ("T4", new char [] { (char) 769}), new PhoneId ("T5", new char [] { (char) 779}), new PhoneId ("TCK", new char [] { (char) 448}), new PhoneId ("TCK2", new char [] { (char) 647}), new PhoneId ("TH", new char [] { (char) 952}), new PhoneId ("TR", new char [] { (char) 648}), new PhoneId ("TS", new char [] { (char) 116, (char) 865, (char) 115}), new PhoneId ("TS2", new char [] { (char) 678}), new PhoneId ("TSR", new char [] { (char) 116, (char) 865, (char) 642}), new PhoneId ("U", new char [] { (char) 117}), new PhoneId ("UH", new char [] { (char) 650}), new PhoneId ("UR", new char [] { (char) 606}), new PhoneId ("UU", new char [] { (char) 623}), new PhoneId ("UWX", new char [] { (char) 117, (char) 865, (char) 601}), new PhoneId ("UYX", new char [] { (char) 121, (char) 865, (char) 601}), new PhoneId ("V", new char [] { (char) 118}), new PhoneId ("VA", new char [] { (char) 651}), new PhoneId ("VCD", new char [] { (char) 812}), new PhoneId ("VEL", new char [] { (char) 736}), new PhoneId ("VLS", new char [] { (char) 778}), new PhoneId ("VPH", new char [] { (char) 820}), new PhoneId ("VSL", new char [] { (char) 805}), new PhoneId ("W", new char [] { (char) 119}), new PhoneId ("WH", new char [] { (char) 653}), new PhoneId ("WJ", new char [] { (char) 613}), new PhoneId ("X", new char [] { (char) 120}), new PhoneId ("XSH", new char [] { (char) 728}), new PhoneId ("XST", new char [] { (char) 774}), new PhoneId ("Y", new char [] { (char) 121}), new PhoneId ("YH", new char [] { (char) 655}), new PhoneId ("YX", new char [] { (char) 649}), new PhoneId ("Z", new char [] { (char) 122}), new PhoneId ("ZC", new char [] { (char) 657}), new PhoneId ("ZH", new char [] { (char) 658}), new PhoneId ("ZHJ", new char [] { (char) 659}), new PhoneId ("ZR", new char [] { (char) 656})}),
            new PhoneMap ( 0x404, new PhoneId [] {new PhoneId ("0021", new char [] { (char) 33}), new PhoneId ("0026", new char [] { (char) 38}), new PhoneId ("002A", new char [] { (char) 42}), new PhoneId ("002B", new char [] { (char) 43}), new PhoneId ("002C", new char [] { (char) 44}), new PhoneId ("002D", new char [] { (char) 45}), new PhoneId ("002E", new char [] { (char) 46}), new PhoneId ("003F", new char [] { (char) 63}), new PhoneId ("005F", new char [] { (char) 95}), new PhoneId ("02C7", new char [] { (char) 711}), new PhoneId ("02C9", new char [] { (char) 713}), new PhoneId ("02CA", new char [] { (char) 714}), new PhoneId ("02CB", new char [] { (char) 715}), new PhoneId ("02D9", new char [] { (char) 729}), new PhoneId ("3000", new char [] { (char) 12288}), new PhoneId ("3105", new char [] { (char) 12549}), new PhoneId ("3106", new char [] { (char) 12550}), new PhoneId ("3107", new char [] { (char) 12551}), new PhoneId ("3108", new char [] { (char) 12552}), new PhoneId ("3109", new char [] { (char) 12553}), new PhoneId ("310A", new char [] { (char) 12554}), new PhoneId ("310B", new char [] { (char) 12555}), new PhoneId ("310C", new char [] { (char) 12556}), new PhoneId ("310D", new char [] { (char) 12557}), new PhoneId ("310E", new char [] { (char) 12558}), new PhoneId ("310F", new char [] { (char) 12559}), new PhoneId ("3110", new char [] { (char) 12560}), new PhoneId ("3111", new char [] { (char) 12561}), new PhoneId ("3112", new char [] { (char) 12562}), new PhoneId ("3113", new char [] { (char) 12563}), new PhoneId ("3114", new char [] { (char) 12564}), new PhoneId ("3115", new char [] { (char) 12565}), new PhoneId ("3116", new char [] { (char) 12566}), new PhoneId ("3117", new char [] { (char) 12567}), new PhoneId ("3118", new char [] { (char) 12568}), new PhoneId ("3119", new char [] { (char) 12569}), new PhoneId ("311A", new char [] { (char) 12570}), new PhoneId ("311B", new char [] { (char) 12571}), new PhoneId ("311C", new char [] { (char) 12572}), new PhoneId ("311D", new char [] { (char) 12573}), new PhoneId ("311E", new char [] { (char) 12574}), new PhoneId ("311F", new char [] { (char) 12575}), new PhoneId ("3120", new char [] { (char) 12576}), new PhoneId ("3121", new char [] { (char) 12577}), new PhoneId ("3122", new char [] { (char) 12578}), new PhoneId ("3123", new char [] { (char) 12579}), new PhoneId ("3124", new char [] { (char) 12580}), new PhoneId ("3125", new char [] { (char) 12581}), new PhoneId ("3126", new char [] { (char) 12582}), new PhoneId ("3127", new char [] { (char) 12583}), new PhoneId ("3128", new char [] { (char) 12584}), new PhoneId ("3129", new char [] { (char) 12585})}),
            new PhoneMap ( 0x407, new PhoneId [] {new PhoneId ("-", new char [] { (char) 1}), new PhoneId ("!", new char [] { (char) 2}), new PhoneId ("&", new char [] { (char) 3}), new PhoneId (",", new char [] { (char) 4}), new PhoneId (".", new char [] { (char) 5}), new PhoneId (":", new char [] { (char) 12}), new PhoneId ("?", new char [] { (char) 6}), new PhoneId ("^", new char [] { (char) 8}), new PhoneId ("_", new char [] { (char) 7}), new PhoneId ("~", new char [] { (char) 11}), new PhoneId (":", new char [] {(char) 12 }), new PhoneId ("a", new char [] { (char) 14 }), new PhoneId ("0013", new char [] { (char) 14 }), new PhoneId ("0014", new char [] { (char) 14 }), new PhoneId ("0015", new char [] { (char) 14 }), new PhoneId ("0016", new char [] { (char) 14 }), new PhoneId ("0017", new char [] { (char) 14 }), new PhoneId ("0018", new char [] { (char) 14 }), new PhoneId ("0019", new char [] { (char) 14 }), new PhoneId ("001A", new char [] { (char) 14 }), new PhoneId ("001B", new char [] { (char) 14 }), new PhoneId ("001C", new char [] { (char) 14 }), new PhoneId ("001D", new char [] { (char) 14 }), new PhoneId ("001E", new char [] { (char) 14 }), new PhoneId ("001F", new char [] { (char) 14 }), new PhoneId ("0020", new char [] { (char) 14 }), new PhoneId ("0021", new char [] { (char) 14 }), new PhoneId ("0022", new char [] { (char) 14 }), new PhoneId ("0023", new char [] { (char) 14 }), new PhoneId ("0024", new char [] { (char) 14 }), new PhoneId ("0025", new char [] { (char) 14 }), new PhoneId ("0026", new char [] { (char) 14 }), new PhoneId ("0027", new char [] { (char) 14 }), new PhoneId ("0028", new char [] { (char) 14 }), new PhoneId ("0029", new char [] { (char) 14 }), new PhoneId ("002A", new char [] { (char) 14 }), new PhoneId ("002B", new char [] { (char) 14 }), new PhoneId ("002C", new char [] { (char) 14 }), new PhoneId ("002D", new char [] { (char) 14 }), new PhoneId ("002E", new char [] { (char) 14 }), new PhoneId ("002F", new char [] { (char) 14 }), new PhoneId ("0030", new char [] { (char) 14 }), new PhoneId ("1", new char [] { (char) 9}), new PhoneId ("2", new char [] { (char) 10}), new PhoneId ("A", new char [] { (char) 13}), new PhoneId ("AW", new char [] { (char) 14}), new PhoneId ("AX", new char [] { (char) 15}), new PhoneId ("AY", new char [] { (char) 16}), new PhoneId ("V", new char [] { (char) 49}), new PhoneId ("X", new char [] { (char) 50}), new PhoneId ("Y", new char [] { (char) 51}), new PhoneId ("Z", new char [] { (char) 52}), new PhoneId ("ZH", new char [] { (char) 53})}),
            new PhoneMap ( 0x409, new PhoneId [] {new PhoneId ("-", new char [] { (char) 1}), new PhoneId ("!", new char [] { (char) 2}), new PhoneId ("&", new char [] { (char) 3}), new PhoneId (",", new char [] { (char) 4}), new PhoneId (".", new char [] { (char) 5}), new PhoneId ("?", new char [] { (char) 6}), new PhoneId ("_", new char [] { (char) 7}), new PhoneId ("1", new char [] { (char) 8}), new PhoneId ("2", new char [] { (char) 9}), new PhoneId ("AA", new char [] { (char) 10}), new PhoneId ("AE", new char [] { (char) 11}), new PhoneId ("AH", new char [] { (char) 12}), new PhoneId ("AO", new char [] { (char) 13}), new PhoneId ("AW", new char [] { (char) 14}), new PhoneId ("AX", new char [] { (char) 15}), new PhoneId ("AY", new char [] { (char) 16}), new PhoneId ("B", new char [] { (char) 17}), new PhoneId ("CH", new char [] { (char) 18}), new PhoneId ("D", new char [] { (char) 19}), new PhoneId ("DH", new char [] { (char) 20}), new PhoneId ("EH", new char [] { (char) 21}), new PhoneId ("ER", new char [] { (char) 22}), new PhoneId ("EY", new char [] { (char) 23}), new PhoneId ("F", new char [] { (char) 24}), new PhoneId ("G", new char [] { (char) 25}), new PhoneId ("H", new char [] { (char) 26}), new PhoneId ("IH", new char [] { (char) 27}), new PhoneId ("IY", new char [] { (char) 28}), new PhoneId ("JH", new char [] { (char) 29}), new PhoneId ("K", new char [] { (char) 30}), new PhoneId ("L", new char [] { (char) 31}), new PhoneId ("M", new char [] { (char) 32}), new PhoneId ("N", new char [] { (char) 33}), new PhoneId ("NG", new char [] { (char) 34}), new PhoneId ("OW", new char [] { (char) 35}), new PhoneId ("OY", new char [] { (char) 36}), new PhoneId ("P", new char [] { (char) 37}), new PhoneId ("R", new char [] { (char) 38}), new PhoneId ("S", new char [] { (char) 39}), new PhoneId ("SH", new char [] { (char) 40}), new PhoneId ("T", new char [] { (char) 41}), new PhoneId ("TH", new char [] { (char) 42}), new PhoneId ("UH", new char [] { (char) 43}), new PhoneId ("UW", new char [] { (char) 44}), new PhoneId ("V", new char [] { (char) 45}), new PhoneId ("W", new char [] { (char) 46}), new PhoneId ("Y", new char [] { (char) 47}), new PhoneId ("Z", new char [] { (char) 48}), new PhoneId ("ZH", new char [] { (char) 49})}),
            new PhoneMap ( 0x40A, new PhoneId [] {new PhoneId ("-", new char [] { (char) 1}), new PhoneId ("!", new char [] { (char) 2}), new PhoneId ("&", new char [] { (char) 3}), new PhoneId (",", new char [] { (char) 4}), new PhoneId (".", new char [] { (char) 5}), new PhoneId ("?", new char [] { (char) 6}), new PhoneId ("_", new char [] { (char) 7}), new PhoneId ("1", new char [] { (char) 8}), new PhoneId ("2", new char [] { (char) 9}), new PhoneId ("A", new char [] { (char) 10}), new PhoneId ("B", new char [] { (char) 18}), new PhoneId ("CH", new char [] { (char) 21}), new PhoneId ("D", new char [] { (char) 16}), new PhoneId ("E", new char [] { (char) 11}), new PhoneId ("F", new char [] { (char) 23}), new PhoneId ("G", new char [] { (char) 20}), new PhoneId ("I", new char [] { (char) 12}), new PhoneId ("J", new char [] { (char) 33}), new PhoneId ("JJ", new char [] { (char) 22}), new PhoneId ("K", new char [] { (char) 19}), new PhoneId ("L", new char [] { (char) 29}), new PhoneId ("LL", new char [] { (char) 30}), new PhoneId ("M", new char [] { (char) 26}), new PhoneId ("N", new char [] { (char) 27}), new PhoneId ("NJ", new char [] { (char) 28}), new PhoneId ("O", new char [] { (char) 13}), new PhoneId ("P", new char [] { (char) 17}), new PhoneId ("R", new char [] { (char) 31}), new PhoneId ("RR", new char [] { (char) 32}), new PhoneId ("S", new char [] { (char) 24}), new PhoneId ("T", new char [] { (char) 15}), new PhoneId ("TH", new char [] { (char) 35}), new PhoneId ("U", new char [] { (char) 14}), new PhoneId ("W", new char [] { (char) 34}), new PhoneId ("X", new char [] { (char) 25})}),
            new PhoneMap ( 0x40C, new PhoneId [] {new PhoneId ("-", new char [] { (char) 1}), new PhoneId ("!", new char [] { (char) 2}), new PhoneId ("&", new char [] { (char) 3}), new PhoneId (",", new char [] { (char) 4}), new PhoneId (".", new char [] { (char) 5}), new PhoneId ("?", new char [] { (char) 6}), new PhoneId ("_", new char [] { (char) 7}), new PhoneId ("~", new char [] { (char) 9}), new PhoneId ("1", new char [] { (char) 8}), new PhoneId ("A", new char [] { (char) 11}), new PhoneId ("AA", new char [] { (char) 10}), new PhoneId ("AX", new char [] { (char) 13}), new PhoneId ("B", new char [] { (char) 14}), new PhoneId ("D", new char [] { (char) 15}), new PhoneId ("EH", new char [] { (char) 16}), new PhoneId ("EU", new char [] { (char) 30}), new PhoneId ("EY", new char [] { (char) 17}), new PhoneId ("F", new char [] { (char) 18}), new PhoneId ("G", new char [] { (char) 19}), new PhoneId ("HY", new char [] { (char) 20}), new PhoneId ("IY", new char [] { (char) 22}), new PhoneId ("K", new char [] { (char) 23}), new PhoneId ("L", new char [] { (char) 24}), new PhoneId ("M", new char [] { (char) 25}), new PhoneId ("N", new char [] { (char) 26}), new PhoneId ("NG", new char [] { (char) 27}), new PhoneId ("NJ", new char [] { (char) 28}), new PhoneId ("OE", new char [] { (char) 29}), new PhoneId ("OH", new char [] { (char) 12}), new PhoneId ("OW", new char [] { (char) 31}), new PhoneId ("P", new char [] { (char) 32}), new PhoneId ("R", new char [] { (char) 33}), new PhoneId ("S", new char [] { (char) 34}), new PhoneId ("SH", new char [] { (char) 35}), new PhoneId ("T", new char [] { (char) 36}), new PhoneId ("UW", new char [] { (char) 37}), new PhoneId ("UY", new char [] { (char) 21}), new PhoneId ("V", new char [] { (char) 38}), new PhoneId ("W", new char [] { (char) 39}), new PhoneId ("Y", new char [] { (char) 40}), new PhoneId ("Z", new char [] { (char) 41}), new PhoneId ("ZH", new char [] { (char) 42})}),
            new PhoneMap ( 0x411, new PhoneId [] {new PhoneId ("0021", new char [] { (char) 33}), new PhoneId ("0027", new char [] { (char) 39}), new PhoneId ("002B", new char [] { (char) 43}), new PhoneId ("002E", new char [] { (char) 46}), new PhoneId ("003F", new char [] { (char) 63}), new PhoneId ("005F", new char [] { (char) 95}), new PhoneId ("007C", new char [] { (char) 124}), new PhoneId ("309C", new char [] { (char) 12444}), new PhoneId ("30A1", new char [] { (char) 12449}), new PhoneId ("30A2", new char [] { (char) 12450}), new PhoneId ("30A3", new char [] { (char) 12451}), new PhoneId ("30A4", new char [] { (char) 12452}), new PhoneId ("30A5", new char [] { (char) 12453}), new PhoneId ("30A6", new char [] { (char) 12454}), new PhoneId ("30A7", new char [] { (char) 12455}), new PhoneId ("30A8", new char [] { (char) 12456}), new PhoneId ("30A9", new char [] { (char) 12457}), new PhoneId ("30AA", new char [] { (char) 12458}), new PhoneId ("30AB", new char [] { (char) 12459}), new PhoneId ("30AC", new char [] { (char) 12460}), new PhoneId ("30AD", new char [] { (char) 12461}), new PhoneId ("30AE", new char [] { (char) 12462}), new PhoneId ("30AF", new char [] { (char) 12463}), new PhoneId ("30B0", new char [] { (char) 12464}), new PhoneId ("30B1", new char [] { (char) 12465}), new PhoneId ("30B2", new char [] { (char) 12466}), new PhoneId ("30B3", new char [] { (char) 12467}), new PhoneId ("30B4", new char [] { (char) 12468}), new PhoneId ("30B5", new char [] { (char) 12469}), new PhoneId ("30B6", new char [] { (char) 12470}), new PhoneId ("30B7", new char [] { (char) 12471}), new PhoneId ("30B8", new char [] { (char) 12472}), new PhoneId ("30B9", new char [] { (char) 12473}), new PhoneId ("30BA", new char [] { (char) 12474}), new PhoneId ("30BB", new char [] { (char) 12475}), new PhoneId ("30BC", new char [] { (char) 12476}), new PhoneId ("30BD", new char [] { (char) 12477}), new PhoneId ("30BE", new char [] { (char) 12478}), new PhoneId ("30BF", new char [] { (char) 12479}), new PhoneId ("30C0", new char [] { (char) 12480}), new PhoneId ("30C1", new char [] { (char) 12481}), new PhoneId ("30C2", new char [] { (char) 12482}), new PhoneId ("30C3", new char [] { (char) 12483}), new PhoneId ("30C4", new char [] { (char) 12484}), new PhoneId ("30C5", new char [] { (char) 12485}), new PhoneId ("30C6", new char [] { (char) 12486}), new PhoneId ("30C7", new char [] { (char) 12487}), new PhoneId ("30C8", new char [] { (char) 12488}), new PhoneId ("30C9", new char [] { (char) 12489}), new PhoneId ("30CA", new char [] { (char) 12490}), new PhoneId ("30CB", new char [] { (char) 12491}), new PhoneId ("30CC", new char [] { (char) 12492}), new PhoneId ("30CD", new char [] { (char) 12493}), new PhoneId ("30CE", new char [] { (char) 12494}), new PhoneId ("30CF", new char [] { (char) 12495}), new PhoneId ("30D0", new char [] { (char) 12496}), new PhoneId ("30D1", new char [] { (char) 12497}), new PhoneId ("30D2", new char [] { (char) 12498}), new PhoneId ("30D3", new char [] { (char) 12499}), new PhoneId ("30D4", new char [] { (char) 12500}), new PhoneId ("30D5", new char [] { (char) 12501}), new PhoneId ("30D6", new char [] { (char) 12502}), new PhoneId ("30D7", new char [] { (char) 12503}), new PhoneId ("30D8", new char [] { (char) 12504}), new PhoneId ("30D9", new char [] { (char) 12505}), new PhoneId ("30DA", new char [] { (char) 12506}), new PhoneId ("30DB", new char [] { (char) 12507}), new PhoneId ("30DC", new char [] { (char) 12508}), new PhoneId ("30DD", new char [] { (char) 12509}), new PhoneId ("30DE", new char [] { (char) 12510}), new PhoneId ("30DF", new char [] { (char) 12511}), new PhoneId ("30E0", new char [] { (char) 12512}), new PhoneId ("30E1", new char [] { (char) 12513}), new PhoneId ("30E2", new char [] { (char) 12514}), new PhoneId ("30E3", new char [] { (char) 12515}), new PhoneId ("30E4", new char [] { (char) 12516}), new PhoneId ("30E5", new char [] { (char) 12517}), new PhoneId ("30E6", new char [] { (char) 12518}), new PhoneId ("30E7", new char [] { (char) 12519}), new PhoneId ("30E8", new char [] { (char) 12520}), new PhoneId ("30E9", new char [] { (char) 12521}), new PhoneId ("30EA", new char [] { (char) 12522}), new PhoneId ("30EB", new char [] { (char) 12523}), new PhoneId ("30EC", new char [] { (char) 12524}), new PhoneId ("30ED", new char [] { (char) 12525}), new PhoneId ("30EE", new char [] { (char) 12526}), new PhoneId ("30EF", new char [] { (char) 12527}), new PhoneId ("30F0", new char [] { (char) 12528}), new PhoneId ("30F1", new char [] { (char) 12529}), new PhoneId ("30F2", new char [] { (char) 12530}), new PhoneId ("30F3", new char [] { (char) 12531}), new PhoneId ("30F4", new char [] { (char) 12532}), new PhoneId ("30F5", new char [] { (char) 12533}), new PhoneId ("30F6", new char [] { (char) 12534}), new PhoneId ("30F7", new char [] { (char) 12535}), new PhoneId ("30F8", new char [] { (char) 12536}), new PhoneId ("30F9", new char [] { (char) 12537}), new PhoneId ("30FA", new char [] { (char) 12538}), new PhoneId ("30FB", new char [] { (char) 12539}), new PhoneId ("30FC", new char [] { (char) 12540}), new PhoneId ("30FD", new char [] { (char) 12541}), new PhoneId ("30FE", new char [] { (char) 12542})}),
            new PhoneMap ( 0x804, new PhoneId [] {new PhoneId ("-", new char [] { (char) 1}), new PhoneId ("!", new char [] { (char) 2}), new PhoneId ("&", new char [] { (char) 3}), new PhoneId ("*", new char [] { (char) 9}), new PhoneId (",", new char [] { (char) 4}), new PhoneId (".", new char [] { (char) 5}), new PhoneId ("?", new char [] { (char) 6}), new PhoneId ("_", new char [] { (char) 7}), new PhoneId ("+", new char [] { (char) 8}), new PhoneId ("1", new char [] { (char) 10}), new PhoneId ("2", new char [] { (char) 11}), new PhoneId ("3", new char [] { (char) 12}), new PhoneId ("4", new char [] { (char) 13}), new PhoneId ("5", new char [] { (char) 14}), new PhoneId ("A", new char [] { (char) 15}), new PhoneId ("AI", new char [] { (char) 16}), new PhoneId ("AN", new char [] { (char) 17}), new PhoneId ("ANG", new char [] { (char) 18}), new PhoneId ("AO", new char [] { (char) 19}), new PhoneId ("BA", new char [] { (char) 20}), new PhoneId ("BAI", new char [] { (char) 21}), new PhoneId ("BAN", new char [] { (char) 22}), new PhoneId ("BANG", new char [] { (char) 23}), new PhoneId ("BAO", new char [] { (char) 24}), new PhoneId ("BEI", new char [] { (char) 25}), new PhoneId ("BEN", new char [] { (char) 26}), new PhoneId ("BENG", new char [] { (char) 27}), new PhoneId ("BI", new char [] { (char) 28}), new PhoneId ("BIAN", new char [] { (char) 29}), new PhoneId ("BIAO", new char [] { (char) 30}), new PhoneId ("BIE", new char [] { (char) 31}), new PhoneId ("BIN", new char [] { (char) 32}), new PhoneId ("BING", new char [] { (char) 33}), new PhoneId ("BO", new char [] { (char) 34}), new PhoneId ("BU", new char [] { (char) 35}), new PhoneId ("CA", new char [] { (char) 36}), new PhoneId ("CAI", new char [] { (char) 37}), new PhoneId ("CAN", new char [] { (char) 38}), new PhoneId ("CANG", new char [] { (char) 39}), new PhoneId ("CAO", new char [] { (char) 40}), new PhoneId ("CE", new char [] { (char) 41}), new PhoneId ("CEN", new char [] { (char) 42}), new PhoneId ("CENG", new char [] { (char) 43}), new PhoneId ("CHA", new char [] { (char) 44}), new PhoneId ("CHAI", new char [] { (char) 45}), new PhoneId ("CHAN", new char [] { (char) 46}), new PhoneId ("CHANG", new char [] { (char) 47}), new PhoneId ("CHAO", new char [] { (char) 48}), new PhoneId ("CHE", new char [] { (char) 49}), new PhoneId ("CHEN", new char [] { (char) 50}), new PhoneId ("CHENG", new char [] { (char) 51}), new PhoneId ("CHI", new char [] { (char) 52}), new PhoneId ("CHONG", new char [] { (char) 53}), new PhoneId ("CHOU", new char [] { (char) 54}), new PhoneId ("CHU", new char [] { (char) 55}), new PhoneId ("CHUAI", new char [] { (char) 56}), new PhoneId ("CHUAN", new char [] { (char) 57}), new PhoneId ("CHUANG", new char [] { (char) 58}), new PhoneId ("CHUI", new char [] { (char) 59}), new PhoneId ("CHUN", new char [] { (char) 60}), new PhoneId ("CHUO", new char [] { (char) 61}), new PhoneId ("CI", new char [] { (char) 62}), new PhoneId ("CONG", new char [] { (char) 63}), new PhoneId ("COU", new char [] { (char) 64}), new PhoneId ("CU", new char [] { (char) 65}), new PhoneId ("CUAN", new char [] { (char) 66}), new PhoneId ("CUI", new char [] { (char) 67}), new PhoneId ("CUN", new char [] { (char) 68}), new PhoneId ("CUO", new char [] { (char) 69}), new PhoneId ("DA", new char [] { (char) 70}), new PhoneId ("DAI", new char [] { (char) 71}), new PhoneId ("DAN", new char [] { (char) 72}), new PhoneId ("DANG", new char [] { (char) 73}), new PhoneId ("DAO", new char [] { (char) 74}), new PhoneId ("DE", new char [] { (char) 75}), new PhoneId ("DEI", new char [] { (char) 76}), new PhoneId ("DEN", new char [] { (char) 77}), new PhoneId ("DENG", new char [] { (char) 78}), new PhoneId ("DI", new char [] { (char) 79}), new PhoneId ("DIA", new char [] { (char) 80}), new PhoneId ("DIAN", new char [] { (char) 81}), new PhoneId ("DIAO", new char [] { (char) 82}), new PhoneId ("DIE", new char [] { (char) 83}), new PhoneId ("DING", new char [] { (char) 84}), new PhoneId ("DIU", new char [] { (char) 85}), new PhoneId ("DONG", new char [] { (char) 86}), new PhoneId ("DOU", new char [] { (char) 87}), new PhoneId ("DU", new char [] { (char) 88}), new PhoneId ("DUAN", new char [] { (char) 89}), new PhoneId ("DUI", new char [] { (char) 90}), new PhoneId ("DUN", new char [] { (char) 91}), new PhoneId ("DUO", new char [] { (char) 92}), new PhoneId ("E", new char [] { (char) 93}), new PhoneId ("EI", new char [] { (char) 94}), new PhoneId ("EN", new char [] { (char) 95}), new PhoneId ("ER", new char [] { (char) 96}), new PhoneId ("FA", new char [] { (char) 97}), new PhoneId ("FAN", new char [] { (char) 98}), new PhoneId ("FANG", new char [] { (char) 99}), new PhoneId ("FEI", new char [] { (char) 100}), new PhoneId ("FEN", new char [] { (char) 101}), new PhoneId ("FENG", new char [] { (char) 102}), new PhoneId ("FO", new char [] { (char) 103}), new PhoneId ("FOU", new char [] { (char) 104}), new PhoneId ("FU", new char [] { (char) 105}), new PhoneId ("GA", new char [] { (char) 106}), new PhoneId ("GAI", new char [] { (char) 107}), new PhoneId ("GAN", new char [] { (char) 108}), new PhoneId ("GANG", new char [] { (char) 109}), new PhoneId ("GAO", new char [] { (char) 110}), new PhoneId ("GE", new char [] { (char) 111}), new PhoneId ("GEI", new char [] { (char) 112}), new PhoneId ("GEN", new char [] { (char) 113}), new PhoneId ("GENG", new char [] { (char) 114}), new PhoneId ("GONG", new char [] { (char) 115}), new PhoneId ("GOU", new char [] { (char) 116}), new PhoneId ("GU", new char [] { (char) 117}), new PhoneId ("GUA", new char [] { (char) 118}), new PhoneId ("GUAI", new char [] { (char) 119}), new PhoneId ("GUAN", new char [] { (char) 120}), new PhoneId ("GUANG", new char [] { (char) 121}), new PhoneId ("GUI", new char [] { (char) 122}), new PhoneId ("GUN", new char [] { (char) 123}), new PhoneId ("GUO", new char [] { (char) 124}), new PhoneId ("HA", new char [] { (char) 125}), new PhoneId ("HAI", new char [] { (char) 126}), new PhoneId ("HAN", new char [] { (char) 127}), new PhoneId ("HANG", new char [] { (char) 128}), new PhoneId ("HAO", new char [] { (char) 129}), new PhoneId ("HE", new char [] { (char) 130}), new PhoneId ("HEI", new char [] { (char) 131}), new PhoneId ("HEN", new char [] { (char) 132}), new PhoneId ("HENG", new char [] { (char) 133}), new PhoneId ("HONG", new char [] { (char) 134}), new PhoneId ("HOU", new char [] { (char) 135}), new PhoneId ("HU", new char [] { (char) 136}), new PhoneId ("HUA", new char [] { (char) 137}), new PhoneId ("HUAI", new char [] { (char) 138}), new PhoneId ("HUAN", new char [] { (char) 139}), new PhoneId ("HUANG", new char [] { (char) 140}), new PhoneId ("HUI", new char [] { (char) 141}), new PhoneId ("HUN", new char [] { (char) 142}), new PhoneId ("HUO", new char [] { (char) 143}), new PhoneId ("JI", new char [] { (char) 144}), new PhoneId ("JIA", new char [] { (char) 145}), new PhoneId ("JIAN", new char [] { (char) 146}), new PhoneId ("JIANG", new char [] { (char) 147}), new PhoneId ("JIAO", new char [] { (char) 148}), new PhoneId ("JIE", new char [] { (char) 149}), new PhoneId ("JIN", new char [] { (char) 150}), new PhoneId ("JING", new char [] { (char) 151}), new PhoneId ("JIONG", new char [] { (char) 152}), new PhoneId ("JIU", new char [] { (char) 153}), new PhoneId ("JU", new char [] { (char) 154}), new PhoneId ("JUAN", new char [] { (char) 155}), new PhoneId ("JUE", new char [] { (char) 156}), new PhoneId ("JUN", new char [] { (char) 157}), new PhoneId ("KA", new char [] { (char) 158}), new PhoneId ("KAI", new char [] { (char) 159}), new PhoneId ("KAN", new char [] { (char) 160}), new PhoneId ("KANG", new char [] { (char) 161}), new PhoneId ("KAO", new char [] { (char) 162}), new PhoneId ("KE", new char [] { (char) 163}), new PhoneId ("KEI", new char [] { (char) 164}), new PhoneId ("KEN", new char [] { (char) 165}), new PhoneId ("KENG", new char [] { (char) 166}), new PhoneId ("KONG", new char [] { (char) 167}), new PhoneId ("KOU", new char [] { (char) 168}), new PhoneId ("KU", new char [] { (char) 169}), new PhoneId ("KUA", new char [] { (char) 170}), new PhoneId ("KUAI", new char [] { (char) 171}), new PhoneId ("KUAN", new char [] { (char) 172}), new PhoneId ("KUANG", new char [] { (char) 173}), new PhoneId ("KUI", new char [] { (char) 174}), new PhoneId ("KUN", new char [] { (char) 175}), new PhoneId ("KUO", new char [] { (char) 176}), new PhoneId ("LA", new char [] { (char) 177}), new PhoneId ("LAI", new char [] { (char) 178}), new PhoneId ("LAN", new char [] { (char) 179}), new PhoneId ("LANG", new char [] { (char) 180}), new PhoneId ("LAO", new char [] { (char) 181}), new PhoneId ("LE", new char [] { (char) 182}), new PhoneId ("LEI", new char [] { (char) 183}), new PhoneId ("LENG", new char [] { (char) 184}), new PhoneId ("LI", new char [] { (char) 185}), new PhoneId ("LIA", new char [] { (char) 186}), new PhoneId ("LIAN", new char [] { (char) 187}), new PhoneId ("LIANG", new char [] { (char) 188}), new PhoneId ("LIAO", new char [] { (char) 189}), new PhoneId ("LIE", new char [] { (char) 190}), new PhoneId ("LIN", new char [] { (char) 191}), new PhoneId ("LING", new char [] { (char) 192}), new PhoneId ("LIU", new char [] { (char) 193}), new PhoneId ("LO", new char [] { (char) 194}), new PhoneId ("LONG", new char [] { (char) 195}), new PhoneId ("LOU", new char [] { (char) 196}), new PhoneId ("LU", new char [] { (char) 197}), new PhoneId ("LUAN", new char [] { (char) 198}), new PhoneId ("LUE", new char [] { (char) 199}), new PhoneId ("LUN", new char [] { (char) 200}), new PhoneId ("LUO", new char [] { (char) 201}), new PhoneId ("LV", new char [] { (char) 202}), new PhoneId ("MA", new char [] { (char) 203}), new PhoneId ("MAI", new char [] { (char) 204}), new PhoneId ("MAN", new char [] { (char) 205}), new PhoneId ("MANG", new char [] { (char) 206}), new PhoneId ("MAO", new char [] { (char) 207}), new PhoneId ("ME", new char [] { (char) 208}), new PhoneId ("MEI", new char [] { (char) 209}), new PhoneId ("MEN", new char [] { (char) 210}), new PhoneId ("MENG", new char [] { (char) 211}), new PhoneId ("MI", new char [] { (char) 212}), new PhoneId ("MIAN", new char [] { (char) 213}), new PhoneId ("MIAO", new char [] { (char) 214}), new PhoneId ("MIE", new char [] { (char) 215}), new PhoneId ("MIN", new char [] { (char) 216}), new PhoneId ("MING", new char [] { (char) 217}), new PhoneId ("MIU", new char [] { (char) 218}), new PhoneId ("MO", new char [] { (char) 219}), new PhoneId ("MOU", new char [] { (char) 220}), new PhoneId ("MU", new char [] { (char) 221}), new PhoneId ("NA", new char [] { (char) 222}), new PhoneId ("NAI", new char [] { (char) 223}), new PhoneId ("NAN", new char [] { (char) 224}), new PhoneId ("NANG", new char [] { (char) 225}), new PhoneId ("NAO", new char [] { (char) 226}), new PhoneId ("NE", new char [] { (char) 227}), new PhoneId ("NEI", new char [] { (char) 228}), new PhoneId ("NEN", new char [] { (char) 229}), new PhoneId ("NENG", new char [] { (char) 230}), new PhoneId ("NI", new char [] { (char) 231}), new PhoneId ("NIAN", new char [] { (char) 232}), new PhoneId ("NIANG", new char [] { (char) 233}), new PhoneId ("NIAO", new char [] { (char) 234}), new PhoneId ("NIE", new char [] { (char) 235}), new PhoneId ("NIN", new char [] { (char) 236}), new PhoneId ("NING", new char [] { (char) 237}), new PhoneId ("NIU", new char [] { (char) 238}), new PhoneId ("NONG", new char [] { (char) 239}), new PhoneId ("NOU", new char [] { (char) 240}), new PhoneId ("NU", new char [] { (char) 241}), new PhoneId ("NUAN", new char [] { (char) 242}), new PhoneId ("NUE", new char [] { (char) 243}), new PhoneId ("NUO", new char [] { (char) 244}), new PhoneId ("NV", new char [] { (char) 245}), new PhoneId ("O", new char [] { (char) 246}), new PhoneId ("OU", new char [] { (char) 247}), new PhoneId ("PA", new char [] { (char) 248}), new PhoneId ("PAI", new char [] { (char) 249}), new PhoneId ("PAN", new char [] { (char) 250}), new PhoneId ("PANG", new char [] { (char) 251}), new PhoneId ("PAO", new char [] { (char) 252}), new PhoneId ("PEI", new char [] { (char) 253}), new PhoneId ("PEN", new char [] { (char) 254}), new PhoneId ("PENG", new char [] { (char) 255}), new PhoneId ("PI", new char [] { (char) 256}), new PhoneId ("PIAN", new char [] { (char) 257}), new PhoneId ("PIAO", new char [] { (char) 258}), new PhoneId ("PIE", new char [] { (char) 259}), new PhoneId ("PIN", new char [] { (char) 260}), new PhoneId ("PING", new char [] { (char) 261}), new PhoneId ("PO", new char [] { (char) 262}), new PhoneId ("POU", new char [] { (char) 263}), new PhoneId ("PU", new char [] { (char) 264}), new PhoneId ("QI", new char [] { (char) 265}), new PhoneId ("QIA", new char [] { (char) 266}), new PhoneId ("QIAN", new char [] { (char) 267}), new PhoneId ("QIANG", new char [] { (char) 268}), new PhoneId ("QIAO", new char [] { (char) 269}), new PhoneId ("QIE", new char [] { (char) 270}), new PhoneId ("QIN", new char [] { (char) 271}), new PhoneId ("QING", new char [] { (char) 272}), new PhoneId ("QIONG", new char [] { (char) 273}), new PhoneId ("QIU", new char [] { (char) 274}), new PhoneId ("QU", new char [] { (char) 275}), new PhoneId ("QUAN", new char [] { (char) 276}), new PhoneId ("QUE", new char [] { (char) 277}), new PhoneId ("QUN", new char [] { (char) 278}), new PhoneId ("RAN", new char [] { (char) 279}), new PhoneId ("RANG", new char [] { (char) 280}), new PhoneId ("RAO", new char [] { (char) 281}), new PhoneId ("RE", new char [] { (char) 282}), new PhoneId ("REN", new char [] { (char) 283}), new PhoneId ("RENG", new char [] { (char) 284}), new PhoneId ("RI", new char [] { (char) 285}), new PhoneId ("RONG", new char [] { (char) 286}), new PhoneId ("ROU", new char [] { (char) 287}), new PhoneId ("RU", new char [] { (char) 288}), new PhoneId ("RUAN", new char [] { (char) 289}), new PhoneId ("RUI", new char [] { (char) 290}), new PhoneId ("RUN", new char [] { (char) 291}), new PhoneId ("RUO", new char [] { (char) 292}), new PhoneId ("SA", new char [] { (char) 293}), new PhoneId ("SAI", new char [] { (char) 294}), new PhoneId ("SAN", new char [] { (char) 295}), new PhoneId ("SANG", new char [] { (char) 296}), new PhoneId ("SAO", new char [] { (char) 297}), new PhoneId ("SE", new char [] { (char) 298}), new PhoneId ("SEN", new char [] { (char) 299}), new PhoneId ("SENG", new char [] { (char) 300}), new PhoneId ("SHA", new char [] { (char) 301}), new PhoneId ("SHAI", new char [] { (char) 302}), new PhoneId ("SHAN", new char [] { (char) 303}), new PhoneId ("SHANG", new char [] { (char) 304}), new PhoneId ("SHAO", new char [] { (char) 305}), new PhoneId ("SHE", new char [] { (char) 306}), new PhoneId ("SHEI", new char [] { (char) 307}), new PhoneId ("SHEN", new char [] { (char) 308}), new PhoneId ("SHENG", new char [] { (char) 309}), new PhoneId ("SHI", new char [] { (char) 310}), new PhoneId ("SHOU", new char [] { (char) 311}), new PhoneId ("SHU", new char [] { (char) 312}), new PhoneId ("SHUA", new char [] { (char) 313}), new PhoneId ("SHUAI", new char [] { (char) 314}), new PhoneId ("SHUAN", new char [] { (char) 315}), new PhoneId ("SHUANG", new char [] { (char) 316}), new PhoneId ("SHUI", new char [] { (char) 317}), new PhoneId ("SHUN", new char [] { (char) 318}), new PhoneId ("SHUO", new char [] { (char) 319}), new PhoneId ("SI", new char [] { (char) 320}), new PhoneId ("SONG", new char [] { (char) 321}), new PhoneId ("SOU", new char [] { (char) 322}), new PhoneId ("SU", new char [] { (char) 323}), new PhoneId ("SUAN", new char [] { (char) 324}), new PhoneId ("SUI", new char [] { (char) 325}), new PhoneId ("SUN", new char [] { (char) 326}), new PhoneId ("SUO", new char [] { (char) 327}), new PhoneId ("TA", new char [] { (char) 328}), new PhoneId ("TAI", new char [] { (char) 329}), new PhoneId ("TAN", new char [] { (char) 330}), new PhoneId ("TANG", new char [] { (char) 331}), new PhoneId ("TAO", new char [] { (char) 332}), new PhoneId ("TE", new char [] { (char) 333}), new PhoneId ("TEI", new char [] { (char) 334}), new PhoneId ("TENG", new char [] { (char) 335}), new PhoneId ("TI", new char [] { (char) 336}), new PhoneId ("TIAN", new char [] { (char) 337}), new PhoneId ("TIAO", new char [] { (char) 338}), new PhoneId ("TIE", new char [] { (char) 339}), new PhoneId ("TING", new char [] { (char) 340}), new PhoneId ("TONG", new char [] { (char) 341}), new PhoneId ("TOU", new char [] { (char) 342}), new PhoneId ("TU", new char [] { (char) 343}), new PhoneId ("TUAN", new char [] { (char) 344}), new PhoneId ("TUI", new char [] { (char) 345}), new PhoneId ("TUN", new char [] { (char) 346}), new PhoneId ("TUO", new char [] { (char) 347}), new PhoneId ("WA", new char [] { (char) 348}), new PhoneId ("WAI", new char [] { (char) 349}), new PhoneId ("WAN", new char [] { (char) 350}), new PhoneId ("WANG", new char [] { (char) 351}), new PhoneId ("WEI", new char [] { (char) 352}), new PhoneId ("WEN", new char [] { (char) 353}), new PhoneId ("WENG", new char [] { (char) 354}), new PhoneId ("WO", new char [] { (char) 355}), new PhoneId ("WU", new char [] { (char) 356}), new PhoneId ("XI", new char [] { (char) 357}), new PhoneId ("XIA", new char [] { (char) 358}), new PhoneId ("XIAN", new char [] { (char) 359}), new PhoneId ("XIANG", new char [] { (char) 360}), new PhoneId ("XIAO", new char [] { (char) 361}), new PhoneId ("XIE", new char [] { (char) 362}), new PhoneId ("XIN", new char [] { (char) 363}), new PhoneId ("XING", new char [] { (char) 364}), new PhoneId ("XIONG", new char [] { (char) 365}), new PhoneId ("XIU", new char [] { (char) 366}), new PhoneId ("XU", new char [] { (char) 367}), new PhoneId ("XUAN", new char [] { (char) 368}), new PhoneId ("XUE", new char [] { (char) 369}), new PhoneId ("XUN", new char [] { (char) 370}), new PhoneId ("YA", new char [] { (char) 371}), new PhoneId ("YAN", new char [] { (char) 372}), new PhoneId ("YANG", new char [] { (char) 373}), new PhoneId ("YAO", new char [] { (char) 374}), new PhoneId ("YE", new char [] { (char) 375}), new PhoneId ("YI", new char [] { (char) 376}), new PhoneId ("YIN", new char [] { (char) 377}), new PhoneId ("YING", new char [] { (char) 378}), new PhoneId ("YO", new char [] { (char) 379}), new PhoneId ("YONG", new char [] { (char) 380}), new PhoneId ("YOU", new char [] { (char) 381}), new PhoneId ("YU", new char [] { (char) 382}), new PhoneId ("YUAN", new char [] { (char) 383}), new PhoneId ("YUE", new char [] { (char) 384}), new PhoneId ("YUN", new char [] { (char) 385}), new PhoneId ("ZA", new char [] { (char) 386}), new PhoneId ("ZAI", new char [] { (char) 387}), new PhoneId ("ZAN", new char [] { (char) 388}), new PhoneId ("ZANG", new char [] { (char) 389}), new PhoneId ("ZAO", new char [] { (char) 390}), new PhoneId ("ZE", new char [] { (char) 391}), new PhoneId ("ZEI", new char [] { (char) 392}), new PhoneId ("ZEN", new char [] { (char) 393}), new PhoneId ("ZENG", new char [] { (char) 394}), new PhoneId ("ZHA", new char [] { (char) 395}), new PhoneId ("ZHAI", new char [] { (char) 396}), new PhoneId ("ZHAN", new char [] { (char) 397}), new PhoneId ("ZHANG", new char [] { (char) 398}), new PhoneId ("ZHAO", new char [] { (char) 399}), new PhoneId ("ZHE", new char [] { (char) 400}), new PhoneId ("ZHEI", new char [] { (char) 401}), new PhoneId ("ZHEN", new char [] { (char) 402}), new PhoneId ("ZHENG", new char [] { (char) 403}), new PhoneId ("ZHI", new char [] { (char) 404}), new PhoneId ("ZHONG", new char [] { (char) 405}), new PhoneId ("ZHOU", new char [] { (char) 406}), new PhoneId ("ZHU", new char [] { (char) 407}), new PhoneId ("ZHUA", new char [] { (char) 408}), new PhoneId ("ZHUAI", new char [] { (char) 409}), new PhoneId ("ZHUAN", new char [] { (char) 410}), new PhoneId ("ZHUANG", new char [] { (char) 411}), new PhoneId ("ZHUI", new char [] { (char) 412}), new PhoneId ("ZHUN", new char [] { (char) 413}), new PhoneId ("ZHUO", new char [] { (char) 414}), new PhoneId ("ZI", new char [] { (char) 415}), new PhoneId ("ZONG", new char [] { (char) 416}), new PhoneId ("ZOU", new char [] { (char) 417}), new PhoneId ("ZU", new char [] { (char) 418}), new PhoneId ("ZUAN", new char [] { (char) 419}), new PhoneId ("ZUI", new char [] { (char) 420}), new PhoneId ("ZUN", new char [] { (char) 421}), new PhoneId ("ZUO", new char [] { (char) 422})}),
         };

#endif

        #endregion

        //*******************************************************************
        //
        // Private Fields
        //
        //*******************************************************************

        #region Private Fields

        private PhoneMap _phoneMap;

        private static readonly PhoneMap [] _phoneMaps;

        private static PhoneMapCompressed [] _phoneMapsCompressed = new PhoneMapCompressed [] 
        {
            new PhoneMapCompressed ( 0x0, 207, new byte [] {46, 0, 95, 33, 0, 95, 38, 0, 95, 44, 0, 95, 46, 0, 95, 63, 0, 95, 94, 0, 95, 124, 0, 95, 124, 124, 0, 95, 83, 0, 43, 0, 65, 0, 65, 65, 0, 65, 68, 86, 0, 65, 69, 0, 65, 69, 88, 0, 65, 72, 0, 65, 73, 255, 255, 0, 65, 78, 255, 0, 65, 79, 0, 65, 79, 69, 0, 65, 79, 88, 255, 255, 0, 65, 80, 73, 0, 65, 83, 80, 0, 65, 84, 82, 0, 65, 85, 255, 255, 0, 65, 88, 0, 65, 88, 82, 0, 66, 0, 66, 66, 0, 66, 72, 0, 66, 73, 77, 0, 66, 86, 65, 0, 66, 86, 68, 0, 67, 0, 67, 67, 255, 255, 0, 67, 67, 50, 0, 67, 67, 75, 0, 67, 69, 78, 0, 67, 72, 255, 255, 0, 67, 72, 50, 0, 67, 74, 0, 67, 84, 0, 67, 86, 68, 0, 68, 0, 68, 69, 78, 0, 68, 72, 0, 68, 73, 77, 0, 68, 82, 0, 68, 88, 0, 68, 88, 82, 0, 68, 90, 255, 255, 0, 68, 90, 50, 0, 69, 0, 69, 72, 0, 69, 72, 88, 255, 255, 0, 69, 73, 255, 255, 0, 69, 74, 67, 0, 69, 78, 255, 0, 69, 82, 0, 69, 82, 82, 0, 69, 83, 72, 0, 69, 84, 0, 69, 85, 0, 69, 88, 0, 69, 90, 72, 0, 70, 0, 71, 0, 71, 50, 0, 71, 65, 0, 71, 72, 0, 71, 73, 77, 0, 71, 76, 0, 71, 84, 0, 72, 0, 72, 71, 0, 72, 72, 0, 72, 76, 71, 0, 72, 90, 0, 73, 0, 73, 72, 0, 73, 88, 0, 73, 89, 88, 255, 255, 0, 74, 0, 74, 67, 255, 255, 0, 74, 67, 50, 0, 74, 68, 0, 74, 72, 255, 255, 0, 74, 72, 50, 0, 74, 73, 77, 0, 74, 74, 255, 255, 0, 75, 0, 76, 0, 76, 65, 66, 0, 76, 65, 77, 0, 76, 65, 82, 0, 76, 67, 75, 0, 76, 67, 75, 50, 0, 76, 71, 0, 76, 72, 0, 76, 74, 0, 76, 76, 65, 0, 76, 78, 71, 0, 76, 79, 87, 0, 76, 82, 0, 76, 82, 68, 0, 76, 83, 72, 0, 76, 84, 0, 77, 0, 77, 67, 78, 0, 77, 70, 0, 77, 82, 68, 0, 78, 0, 78, 65, 82, 0, 78, 65, 83, 0, 78, 67, 75, 0, 78, 67, 75, 50, 0, 78, 67, 75, 51, 0, 78, 71, 0, 78, 74, 0, 78, 82, 0, 78, 83, 82, 0, 78, 83, 89, 0, 79, 0, 79, 69, 0, 79, 69, 78, 255, 0, 79, 73, 255, 255, 0, 79, 78, 255, 0, 79, 85, 0, 79, 87, 88, 255, 255, 0, 79, 88, 0, 80, 0, 80, 65, 76, 0, 80, 67, 75, 0, 80, 70, 255, 255, 0, 80, 72, 0, 80, 72, 82, 0, 81, 0, 81, 68, 0, 81, 72, 0, 81, 73, 77, 0, 81, 78, 0, 81, 79, 77, 0, 81, 81, 0, 81, 84, 0, 82, 0, 82, 65, 0, 82, 65, 73, 0, 82, 69, 84, 0, 82, 72, 0, 82, 72, 79, 0, 82, 72, 90, 0, 82, 82, 0, 82, 84, 69, 0, 82, 84, 82, 0, 83, 0, 83, 49, 0, 83, 50, 0, 83, 67, 0, 83, 72, 0, 83, 72, 67, 0, 83, 72, 88, 0, 83, 82, 0, 83, 89, 76, 0, 84, 0, 84, 45, 0, 84, 43, 0, 84, 61, 0, 84, 49, 0, 84, 50, 0, 84, 51, 0, 84, 52, 0, 84, 53, 0, 84, 67, 75, 0, 84, 67, 75, 50, 0, 84, 72, 0, 84, 82, 0, 84, 83, 255, 255, 0, 84, 83, 50, 0, 84, 83, 82, 255, 255, 0, 85, 0, 85, 72, 0, 85, 82, 0, 85, 85, 0, 85, 87, 88, 255, 255, 0, 85, 89, 88, 255, 255, 0, 86, 0, 86, 65, 0, 86, 67, 68, 0, 86, 69, 76, 0, 86, 76, 83, 0, 86, 80, 72, 0, 86, 83, 76, 0, 87, 0, 87, 72, 0, 87, 74, 0, 88, 0, 88, 83, 72, 0, 88, 83, 84, 0, 89, 0, 89, 72, 0, 89, 88, 0, 90, 0, 90, 67, 0, 90, 72, 0, 90, 72, 74, 0, 90, 82, 0}, new char [] {(char) 46, (char) 1, (char) 2, (char) 3, (char) 8600, (char) 8599, (char) 8255, (char) 124, (char) 8214, (char) 4, (char) 865, (char) 97, (char) 593, (char) 799, (char) 230, (char) 592, (char) 652, (char) 97, (char) 865, (char) 105, (char) 97, (char) 771, (char) 596, (char) 630, (char) 596, (char) 865, (char) 601, (char) 826, (char) 688, (char) 792, (char) 97, (char) 865, (char) 650, (char) 601, (char) 602, (char) 98, (char) 665, (char) 946, (char) 595, (char) 689, (char) 804, (char) 231, (char) 1856, (char) 865, (char) 597, (char) 680, (char) 450, (char) 776, (char) 116, (char) 865, (char) 643, (char) 679, (char) 669, (char) 99, (char) 816, (char) 100, (char) 810, (char) 240, (char) 599, (char) 598, (char) 638, (char) 637, (char) 100, (char) 865, (char) 122, (char) 675, (char) 101, (char) 603, (char) 603, (char) 865, (char) 601, (char) 101, (char) 865, (char) 105, (char) 700, (char) 101, (char) 771, (char) 604, (char) 605, (char) 668, (char) 673, (char) 248, (char) 600, (char) 674, (char) 102, (char) 103, (char) 609, (char) 624, (char) 611, (char) 608, (char) 671, (char) 660, (char) 104, (char) 661, (char) 295, (char) 721, (char) 614, (char) 105, (char) 618, (char) 616, (char) 105, (char) 865, (char) 601, (char) 106, (char) 100, (char) 865, (char) 657, (char) 677, (char) 607, (char) 100, (char) 865, (char) 658, (char) 676, (char) 644, (char) 106, (char) 865, (char) 106, (char) 107, (char) 108, (char) 695, (char) 827, (char) 737, (char) 449, (char) 662, (char) 619, (char) 622, (char) 654, (char) 828, (char) 720, (char) 798, (char) 621, (char) 796, (char) 620, (char) 634, (char) 109, (char) 829, (char) 625, (char) 825, (char) 110, (char) 794, (char) 771, (char) 33, (char) 451, (char) 663, (char) 331, (char) 626, (char) 627, (char) 8319, (char) 815, (char) 111, (char) 339, (char) 339, (char) 771, (char) 596, (char) 865, (char) 105, (char) 111, (char) 771, (char) 612, (char) 111, (char) 865, (char) 601, (char) 629, (char) 112, (char) 690, (char) 664, (char) 112, (char) 865, (char) 102, (char) 632, (char) 740, (char) 594, (char) 610, (char) 967, (char) 667, (char) 628, (char) 672, (char) 640, (char) 113, (char) 635, (char) 633, (char) 797, (char) 817, (char) 641, (char) 734, (char) 692, (char) 114, (char) 800, (char) 793, (char) 115, (char) 712, (char) 716, (char) 597, (char) 643, (char) 646, (char) 615, (char) 642, (char) 809, (char) 116, (char) 8595, (char) 8593, (char) 8594, (char) 783, (char) 768, (char) 772, (char) 769, (char) 779, (char) 448, (char) 647, (char) 952, (char) 648, (char) 116, (char) 865, (char) 115, (char) 678, (char) 116, (char) 865, (char) 642, (char) 117, (char) 650, (char) 606, (char) 623, (char) 117, (char) 865, (char) 601, (char) 121, (char) 865, (char) 601, (char) 118, (char) 651, (char) 812, (char) 736, (char) 778, (char) 820, (char) 805, (char) 119, (char) 653, (char) 613, (char) 120, (char) 728, (char) 774, (char) 121, (char) 655, (char) 649, (char) 122, (char) 657, (char) 658, (char) 659, (char) 656}),
            new PhoneMapCompressed ( 0x404, 52, new byte [] {48, 48, 50, 49, 0, 48, 48, 50, 54, 0, 48, 48, 50, 65, 0, 48, 48, 50, 66, 0, 48, 48, 50, 67, 0, 48, 48, 50, 68, 0, 48, 48, 50, 69, 0, 48, 48, 51, 70, 0, 48, 48, 53, 70, 0, 48, 50, 67, 55, 0, 48, 50, 67, 57, 0, 48, 50, 67, 65, 0, 48, 50, 67, 66, 0, 48, 50, 68, 57, 0, 51, 48, 48, 48, 0, 51, 49, 48, 53, 0, 51, 49, 48, 54, 0, 51, 49, 48, 55, 0, 51, 49, 48, 56, 0, 51, 49, 48, 57, 0, 51, 49, 48, 65, 0, 51, 49, 48, 66, 0, 51, 49, 48, 67, 0, 51, 49, 48, 68, 0, 51, 49, 48, 69, 0, 51, 49, 48, 70, 0, 51, 49, 49, 48, 0, 51, 49, 49, 49, 0, 51, 49, 49, 50, 0, 51, 49, 49, 51, 0, 51, 49, 49, 52, 0, 51, 49, 49, 53, 0, 51, 49, 49, 54, 0, 51, 49, 49, 55, 0, 51, 49, 49, 56, 0, 51, 49, 49, 57, 0, 51, 49, 49, 65, 0, 51, 49, 49, 66, 0, 51, 49, 49, 67, 0, 51, 49, 49, 68, 0, 51, 49, 49, 69, 0, 51, 49, 49, 70, 0, 51, 49, 50, 48, 0, 51, 49, 50, 49, 0, 51, 49, 50, 50, 0, 51, 49, 50, 51, 0, 51, 49, 50, 52, 0, 51, 49, 50, 53, 0, 51, 49, 50, 54, 0, 51, 49, 50, 55, 0, 51, 49, 50, 56, 0, 51, 49, 50, 57, 0}, new char [] {(char) 33, (char) 38, (char) 42, (char) 43, (char) 44, (char) 45, (char) 46, (char) 63, (char) 95, (char) 711, (char) 713, (char) 714, (char) 715, (char) 729, (char) 12288, (char) 12549, (char) 12550, (char) 12551, (char) 12552, (char) 12553, (char) 12554, (char) 12555, (char) 12556, (char) 12557, (char) 12558, (char) 12559, (char) 12560, (char) 12561, (char) 12562, (char) 12563, (char) 12564, (char) 12565, (char) 12566, (char) 12567, (char) 12568, (char) 12569, (char) 12570, (char) 12571, (char) 12572, (char) 12573, (char) 12574, (char) 12575, (char) 12576, (char) 12577, (char) 12578, (char) 12579, (char) 12580, (char) 12581, (char) 12582, (char) 12583, (char) 12584, (char) 12585}),
            new PhoneMapCompressed ( 0x407, 53, new byte [] {45, 0, 33, 0, 38, 0, 44, 0, 46, 0, 58, 0, 63, 0, 94, 0, 95, 0, 126, 0, 49, 0, 50, 0, 65, 0, 65, 87, 0, 65, 88, 0, 65, 89, 0, 66, 0, 67, 72, 0, 68, 0, 69, 72, 0, 69, 85, 0, 69, 89, 0, 70, 0, 71, 0, 72, 0, 73, 72, 0, 73, 89, 0, 74, 72, 0, 75, 0, 76, 0, 77, 0, 78, 0, 78, 71, 0, 79, 69, 0, 79, 72, 0, 79, 87, 0, 79, 89, 0, 80, 0, 80, 70, 0, 82, 0, 83, 0, 83, 72, 0, 84, 0, 84, 83, 0, 85, 69, 0, 85, 72, 0, 85, 87, 0, 85, 89, 0, 86, 0, 88, 0, 89, 0, 90, 0, 90, 72, 0}, new char [] {(char) 1, (char) 2, (char) 3, (char) 4, (char) 5, (char) 12, (char) 6, (char) 8, (char) 7, (char) 11, (char) 9, (char) 10, (char) 13, (char) 14, (char) 15, (char) 16, (char) 17, (char) 19, (char) 18, (char) 20, (char) 21, (char) 22, (char) 23, (char) 24, (char) 25, (char) 26, (char) 27, (char) 28, (char) 29, (char) 30, (char) 31, (char) 32, (char) 33, (char) 34, (char) 35, (char) 36, (char) 37, (char) 38, (char) 39, (char) 40, (char) 41, (char) 42, (char) 43, (char) 44, (char) 45, (char) 46, (char) 47, (char) 48, (char) 49, (char) 50, (char) 51, (char) 52, (char) 53}),
            new PhoneMapCompressed ( 0x409, 49, new byte [] {45, 0, 33, 0, 38, 0, 44, 0, 46, 0, 63, 0, 95, 0, 49, 0, 50, 0, 65, 65, 0, 65, 69, 0, 65, 72, 0, 65, 79, 0, 65, 87, 0, 65, 88, 0, 65, 89, 0, 66, 0, 67, 72, 0, 68, 0, 68, 72, 0, 69, 72, 0, 69, 82, 0, 69, 89, 0, 70, 0, 71, 0, 72, 0, 73, 72, 0, 73, 89, 0, 74, 72, 0, 75, 0, 76, 0, 77, 0, 78, 0, 78, 71, 0, 79, 87, 0, 79, 89, 0, 80, 0, 82, 0, 83, 0, 83, 72, 0, 84, 0, 84, 72, 0, 85, 72, 0, 85, 87, 0, 86, 0, 87, 0, 89, 0, 90, 0, 90, 72, 0}, new char [] {(char) 1, (char) 2, (char) 3, (char) 4, (char) 5, (char) 6, (char) 7, (char) 8, (char) 9, (char) 10, (char) 11, (char) 12, (char) 13, (char) 14, (char) 15, (char) 16, (char) 17, (char) 18, (char) 19, (char) 20, (char) 21, (char) 22, (char) 23, (char) 24, (char) 25, (char) 26, (char) 27, (char) 28, (char) 29, (char) 30, (char) 31, (char) 32, (char) 33, (char) 34, (char) 35, (char) 36, (char) 37, (char) 38, (char) 39, (char) 40, (char) 41, (char) 42, (char) 43, (char) 44, (char) 45, (char) 46, (char) 47, (char) 48, (char) 49}),
            new PhoneMapCompressed ( 0x40A, 35, new byte [] {45, 0, 33, 0, 38, 0, 44, 0, 46, 0, 63, 0, 95, 0, 49, 0, 50, 0, 65, 0, 66, 0, 67, 72, 0, 68, 0, 69, 0, 70, 0, 71, 0, 73, 0, 74, 0, 74, 74, 0, 75, 0, 76, 0, 76, 76, 0, 77, 0, 78, 0, 78, 74, 0, 79, 0, 80, 0, 82, 0, 82, 82, 0, 83, 0, 84, 0, 84, 72, 0, 85, 0, 87, 0, 88, 0}, new char [] {(char) 1, (char) 2, (char) 3, (char) 4, (char) 5, (char) 6, (char) 7, (char) 8, (char) 9, (char) 10, (char) 18, (char) 21, (char) 16, (char) 11, (char) 23, (char) 20, (char) 12, (char) 33, (char) 22, (char) 19, (char) 29, (char) 30, (char) 26, (char) 27, (char) 28, (char) 13, (char) 17, (char) 31, (char) 32, (char) 24, (char) 15, (char) 35, (char) 14, (char) 34, (char) 25}),
            new PhoneMapCompressed ( 0x40C, 42, new byte [] {45, 0, 33, 0, 38, 0, 44, 0, 46, 0, 63, 0, 95, 0, 126, 0, 49, 0, 65, 0, 65, 65, 0, 65, 88, 0, 66, 0, 68, 0, 69, 72, 0, 69, 85, 0, 69, 89, 0, 70, 0, 71, 0, 72, 89, 0, 73, 89, 0, 75, 0, 76, 0, 77, 0, 78, 0, 78, 71, 0, 78, 74, 0, 79, 69, 0, 79, 72, 0, 79, 87, 0, 80, 0, 82, 0, 83, 0, 83, 72, 0, 84, 0, 85, 87, 0, 85, 89, 0, 86, 0, 87, 0, 89, 0, 90, 0, 90, 72, 0}, new char [] {(char) 1, (char) 2, (char) 3, (char) 4, (char) 5, (char) 6, (char) 7, (char) 9, (char) 8, (char) 11, (char) 10, (char) 13, (char) 14, (char) 15, (char) 16, (char) 30, (char) 17, (char) 18, (char) 19, (char) 20, (char) 22, (char) 23, (char) 24, (char) 25, (char) 26, (char) 27, (char) 28, (char) 29, (char) 12, (char) 31, (char) 32, (char) 33, (char) 34, (char) 35, (char) 36, (char) 37, (char) 21, (char) 38, (char) 39, (char) 40, (char) 41, (char) 42}),
            new PhoneMapCompressed ( 0x411, 102, new byte [] {48, 48, 50, 49, 0, 48, 48, 50, 55, 0, 48, 48, 50, 66, 0, 48, 48, 50, 69, 0, 48, 48, 51, 70, 0, 48, 48, 53, 70, 0, 48, 48, 55, 67, 0, 51, 48, 57, 67, 0, 51, 48, 65, 49, 0, 51, 48, 65, 50, 0, 51, 48, 65, 51, 0, 51, 48, 65, 52, 0, 51, 48, 65, 53, 0, 51, 48, 65, 54, 0, 51, 48, 65, 55, 0, 51, 48, 65, 56, 0, 51, 48, 65, 57, 0, 51, 48, 65, 65, 0, 51, 48, 65, 66, 0, 51, 48, 65, 67, 0, 51, 48, 65, 68, 0, 51, 48, 65, 69, 0, 51, 48, 65, 70, 0, 51, 48, 66, 48, 0, 51, 48, 66, 49, 0, 51, 48, 66, 50, 0, 51, 48, 66, 51, 0, 51, 48, 66, 52, 0, 51, 48, 66, 53, 0, 51, 48, 66, 54, 0, 51, 48, 66, 55, 0, 51, 48, 66, 56, 0, 51, 48, 66, 57, 0, 51, 48, 66, 65, 0, 51, 48, 66, 66, 0, 51, 48, 66, 67, 0, 51, 48, 66, 68, 0, 51, 48, 66, 69, 0, 51, 48, 66, 70, 0, 51, 48, 67, 48, 0, 51, 48, 67, 49, 0, 51, 48, 67, 50, 0, 51, 48, 67, 51, 0, 51, 48, 67, 52, 0, 51, 48, 67, 53, 0, 51, 48, 67, 54, 0, 51, 48, 67, 55, 0, 51, 48, 67, 56, 0, 51, 48, 67, 57, 0, 51, 48, 67, 65, 0, 51, 48, 67, 66, 0, 51, 48, 67, 67, 0, 51, 48, 67, 68, 0, 51, 48, 67, 69, 0, 51, 48, 67, 70, 0, 51, 48, 68, 48, 0, 51, 48, 68, 49, 0, 51, 48, 68, 50, 0, 51, 48, 68, 51, 0, 51, 48, 68, 52, 0, 51, 48, 68, 53, 0, 51, 48, 68, 54, 0, 51, 48, 68, 55, 0, 51, 48, 68, 56, 0, 51, 48, 68, 57, 0, 51, 48, 68, 65, 0, 51, 48, 68, 66, 0, 51, 48, 68, 67, 0, 51, 48, 68, 68, 0, 51, 48, 68, 69, 0, 51, 48, 68, 70, 0, 51, 48, 69, 48, 0, 51, 48, 69, 49, 0, 51, 48, 69, 50, 0, 51, 48, 69, 51, 0, 51, 48, 69, 52, 0, 51, 48, 69, 53, 0, 51, 48, 69, 54, 0, 51, 48, 69, 55, 0, 51, 48, 69, 56, 0, 51, 48, 69, 57, 0, 51, 48, 69, 65, 0, 51, 48, 69, 66, 0, 51, 48, 69, 67, 0, 51, 48, 69, 68, 0, 51, 48, 69, 69, 0, 51, 48, 69, 70, 0, 51, 48, 70, 48, 0, 51, 48, 70, 49, 0, 51, 48, 70, 50, 0, 51, 48, 70, 51, 0, 51, 48, 70, 52, 0, 51, 48, 70, 53, 0, 51, 48, 70, 54, 0, 51, 48, 70, 55, 0, 51, 48, 70, 56, 0, 51, 48, 70, 57, 0, 51, 48, 70, 65, 0, 51, 48, 70, 66, 0, 51, 48, 70, 67, 0, 51, 48, 70, 68, 0, 51, 48, 70, 69, 0}, new char [] {(char) 33, (char) 39, (char) 43, (char) 46, (char) 63, (char) 95, (char) 124, (char) 12444, (char) 12449, (char) 12450, (char) 12451, (char) 12452, (char) 12453, (char) 12454, (char) 12455, (char) 12456, (char) 12457, (char) 12458, (char) 12459, (char) 12460, (char) 12461, (char) 12462, (char) 12463, (char) 12464, (char) 12465, (char) 12466, (char) 12467, (char) 12468, (char) 12469, (char) 12470, (char) 12471, (char) 12472, (char) 12473, (char) 12474, (char) 12475, (char) 12476, (char) 12477, (char) 12478, (char) 12479, (char) 12480, (char) 12481, (char) 12482, (char) 12483, (char) 12484, (char) 12485, (char) 12486, (char) 12487, (char) 12488, (char) 12489, (char) 12490, (char) 12491, (char) 12492, (char) 12493, (char) 12494, (char) 12495, (char) 12496, (char) 12497, (char) 12498, (char) 12499, (char) 12500, (char) 12501, (char) 12502, (char) 12503, (char) 12504, (char) 12505, (char) 12506, (char) 12507, (char) 12508, (char) 12509, (char) 12510, (char) 12511, (char) 12512, (char) 12513, (char) 12514, (char) 12515, (char) 12516, (char) 12517, (char) 12518, (char) 12519, (char) 12520, (char) 12521, (char) 12522, (char) 12523, (char) 12524, (char) 12525, (char) 12526, (char) 12527, (char) 12528, (char) 12529, (char) 12530, (char) 12531, (char) 12532, (char) 12533, (char) 12534, (char) 12535, (char) 12536, (char) 12537, (char) 12538, (char) 12539, (char) 12540, (char) 12541, (char) 12542}),
            new PhoneMapCompressed ( 0x804, 422, new byte [] {45, 0, 33, 0, 38, 0, 42, 0, 44, 0, 46, 0, 63, 0, 95, 0, 43, 0, 49, 0, 50, 0, 51, 0, 52, 0, 53, 0, 65, 0, 65, 73, 0, 65, 78, 0, 65, 78, 71, 0, 65, 79, 0, 66, 65, 0, 66, 65, 73, 0, 66, 65, 78, 0, 66, 65, 78, 71, 0, 66, 65, 79, 0, 66, 69, 73, 0, 66, 69, 78, 0, 66, 69, 78, 71, 0, 66, 73, 0, 66, 73, 65, 78, 0, 66, 73, 65, 79, 0, 66, 73, 69, 0, 66, 73, 78, 0, 66, 73, 78, 71, 0, 66, 79, 0, 66, 85, 0, 67, 65, 0, 67, 65, 73, 0, 67, 65, 78, 0, 67, 65, 78, 71, 0, 67, 65, 79, 0, 67, 69, 0, 67, 69, 78, 0, 67, 69, 78, 71, 0, 67, 72, 65, 0, 67, 72, 65, 73, 0, 67, 72, 65, 78, 0, 67, 72, 65, 78, 71, 0, 67, 72, 65, 79, 0, 67, 72, 69, 0, 67, 72, 69, 78, 0, 67, 72, 69, 78, 71, 0, 67, 72, 73, 0, 67, 72, 79, 78, 71, 0, 67, 72, 79, 85, 0, 67, 72, 85, 0, 67, 72, 85, 65, 73, 0, 67, 72, 85, 65, 78, 0, 67, 72, 85, 65, 78, 71, 0, 67, 72, 85, 73, 0, 67, 72, 85, 78, 0, 67, 72, 85, 79, 0, 67, 73, 0, 67, 79, 78, 71, 0, 67, 79, 85, 0, 67, 85, 0, 67, 85, 65, 78, 0, 67, 85, 73, 0, 67, 85, 78, 0, 67, 85, 79, 0, 68, 65, 0, 68, 65, 73, 0, 68, 65, 78, 0, 68, 65, 78, 71, 0, 68, 65, 79, 0, 68, 69, 0, 68, 69, 73, 0, 68, 69, 78, 0, 68, 69, 78, 71, 0, 68, 73, 0, 68, 73, 65, 0, 68, 73, 65, 78, 0, 68, 73, 65, 79, 0, 68, 73, 69, 0, 68, 73, 78, 71, 0, 68, 73, 85, 0, 68, 79, 78, 71, 0, 68, 79, 85, 0, 68, 85, 0, 68, 85, 65, 78, 0, 68, 85, 73, 0, 68, 85, 78, 0, 68, 85, 79, 0, 69, 0, 69, 73, 0, 69, 78, 0, 69, 82, 0, 70, 65, 0, 70, 65, 78, 0, 70, 65, 78, 71, 0, 70, 69, 73, 0, 70, 69, 78, 0, 70, 69, 78, 71, 0, 70, 79, 0, 70, 79, 85, 0, 70, 85, 0, 71, 65, 0, 71, 65, 73, 0, 71, 65, 78, 0, 71, 65, 78, 71, 0, 71, 65, 79, 0, 71, 69, 0, 71, 69, 73, 0, 71, 69, 78, 0, 71, 69, 78, 71, 0, 71, 79, 78, 71, 0, 71, 79, 85, 0, 71, 85, 0, 71, 85, 65, 0, 71, 85, 65, 73, 0, 71, 85, 65, 78, 0, 71, 85, 65, 78, 71, 0, 71, 85, 73, 0, 71, 85, 78, 0, 71, 85, 79, 0, 72, 65, 0, 72, 65, 73, 0, 72, 65, 78, 0, 72, 65, 78, 71, 0, 72, 65, 79, 0, 72, 69, 0, 72, 69, 73, 0, 72, 69, 78, 0, 72, 69, 78, 71, 0, 72, 79, 78, 71, 0, 72, 79, 85, 0, 72, 85, 0, 72, 85, 65, 0, 72, 85, 65, 73, 0, 72, 85, 65, 78, 0, 72, 85, 65, 78, 71, 0, 72, 85, 73, 0, 72, 85, 78, 0, 72, 85, 79, 0, 74, 73, 0, 74, 73, 65, 0, 74, 73, 65, 78, 0, 74, 73, 65, 78, 71, 0, 74, 73, 65, 79, 0, 74, 73, 69, 0, 74, 73, 78, 0, 74, 73, 78, 71, 0, 74, 73, 79, 78, 71, 0, 74, 73, 85, 0, 74, 85, 0, 74, 85, 65, 78, 0, 74, 85, 69, 0, 74, 85, 78, 0, 75, 65, 0, 75, 65, 73, 0, 75, 65, 78, 0, 75, 65, 78, 71, 0, 75, 65, 79, 0, 75, 69, 0, 75, 69, 73, 0, 75, 69, 78, 0, 75, 69, 78, 71, 0, 75, 79, 78, 71, 0, 75, 79, 85, 0, 75, 85, 0, 75, 85, 65, 0, 75, 85, 65, 73, 0, 75, 85, 65, 78, 0, 75, 85, 65, 78, 71, 0, 75, 85, 73, 0, 75, 85, 78, 0, 75, 85, 79, 0, 76, 65, 0, 76, 65, 73, 0, 76, 65, 78, 0, 76, 65, 78, 71, 0, 76, 65, 79, 0, 76, 69, 0, 76, 69, 73, 0, 76, 69, 78, 71, 0, 76, 73, 0, 76, 73, 65, 0, 76, 73, 65, 78, 0, 76, 73, 65, 78, 71, 0, 76, 73, 65, 79, 0, 76, 73, 69, 0, 76, 73, 78, 0, 76, 73, 78, 71, 0, 76, 73, 85, 0, 76, 79, 0, 76, 79, 78, 71, 0, 76, 79, 85, 0, 76, 85, 0, 76, 85, 65, 78, 0, 76, 85, 69, 0, 76, 85, 78, 0, 76, 85, 79, 0, 76, 86, 0, 77, 65, 0, 77, 65, 73, 0, 77, 65, 78, 0, 77, 65, 78, 71, 0, 77, 65, 79, 0, 77, 69, 0, 77, 69, 73, 0, 77, 69, 78, 0, 77, 69, 78, 71, 0, 77, 73, 0, 77, 73, 65, 78, 0, 77, 73, 65, 79, 0, 77, 73, 69, 0, 77, 73, 78, 0, 77, 73, 78, 71, 0, 77, 73, 85, 0, 77, 79, 0, 77, 79, 85, 0, 77, 85, 0, 78, 65, 0, 78, 65, 73, 0, 78, 65, 78, 0, 78, 65, 78, 71, 0, 78, 65, 79, 0, 78, 69, 0, 78, 69, 73, 0, 78, 69, 78, 0, 78, 69, 78, 71, 0, 78, 73, 0, 78, 73, 65, 78, 0, 78, 73, 65, 78, 71, 0, 78, 73, 65, 79, 0, 78, 73, 69, 0, 78, 73, 78, 0, 78, 73, 78, 71, 0, 78, 73, 85, 0, 78, 79, 78, 71, 0, 78, 79, 85, 0, 78, 85, 0, 78, 85, 65, 78, 0, 78, 85, 69, 0, 78, 85, 79, 0, 78, 86, 0, 79, 0, 79, 85, 0, 80, 65, 0, 80, 65, 73, 0, 80, 65, 78, 0, 80, 65, 78, 71, 0, 80, 65, 79, 0, 80, 69, 73, 0, 80, 69, 78, 0, 80, 69, 78, 71, 0, 80, 73, 0, 80, 73, 65, 78, 0, 80, 73, 65, 79, 0, 80, 73, 69, 0, 80, 73, 78, 0, 80, 73, 78, 71, 0, 80, 79, 0, 80, 79, 85, 0, 80, 85, 0, 81, 73, 0, 81, 73, 65, 0, 81, 73, 65, 78, 0, 81, 73, 65, 78, 71, 0, 81, 73, 65, 79, 0, 81, 73, 69, 0, 81, 73, 78, 0, 81, 73, 78, 71, 0, 81, 73, 79, 78, 71, 0, 81, 73, 85, 0, 81, 85, 0, 81, 85, 65, 78, 0, 81, 85, 69, 0, 81, 85, 78, 0, 82, 65, 78, 0, 82, 65, 78, 71, 0, 82, 65, 79, 0, 82, 69, 0, 82, 69, 78, 0, 82, 69, 78, 71, 0, 82, 73, 0, 82, 79, 78, 71, 0, 82, 79, 85, 0, 82, 85, 0, 82, 85, 65, 78, 0, 82, 85, 73, 0, 82, 85, 78, 0, 82, 85, 79, 0, 83, 65, 0, 83, 65, 73, 0, 83, 65, 78, 0, 83, 65, 78, 71, 0, 83, 65, 79, 0, 83, 69, 0, 83, 69, 78, 0, 83, 69, 78, 71, 0, 83, 72, 65, 0, 83, 72, 65, 73, 0, 83, 72, 65, 78, 0, 83, 72, 65, 78, 71, 0, 83, 72, 65, 79, 0, 83, 72, 69, 0, 83, 72, 69, 73, 0, 83, 72, 69, 78, 0, 83, 72, 69, 78, 71, 0, 83, 72, 73, 0, 83, 72, 79, 85, 0, 83, 72, 85, 0, 83, 72, 85, 65, 0, 83, 72, 85, 65, 73, 0, 83, 72, 85, 65, 78, 0, 83, 72, 85, 65, 78, 71, 0, 83, 72, 85, 73, 0, 83, 72, 85, 78, 0, 83, 72, 85, 79, 0, 83, 73, 0, 83, 79, 78, 71, 0, 83, 79, 85, 0, 83, 85, 0, 83, 85, 65, 78, 0, 83, 85, 73, 0, 83, 85, 78, 0, 83, 85, 79, 0, 84, 65, 0, 84, 65, 73, 0, 84, 65, 78, 0, 84, 65, 78, 71, 0, 84, 65, 79, 0, 84, 69, 0, 84, 69, 73, 0, 84, 69, 78, 71, 0, 84, 73, 0, 84, 73, 65, 78, 0, 84, 73, 65, 79, 0, 84, 73, 69, 0, 84, 73, 78, 71, 0, 84, 79, 78, 71, 0, 84, 79, 85, 0, 84, 85, 0, 84, 85, 65, 78, 0, 84, 85, 73, 0, 84, 85, 78, 0, 84, 85, 79, 0, 87, 65, 0, 87, 65, 73, 0, 87, 65, 78, 0, 87, 65, 78, 71, 0, 87, 69, 73, 0, 87, 69, 78, 0, 87, 69, 78, 71, 0, 87, 79, 0, 87, 85, 0, 88, 73, 0, 88, 73, 65, 0, 88, 73, 65, 78, 0, 88, 73, 65, 78, 71, 0, 88, 73, 65, 79, 0, 88, 73, 69, 0, 88, 73, 78, 0, 88, 73, 78, 71, 0, 88, 73, 79, 78, 71, 0, 88, 73, 85, 0, 88, 85, 0, 88, 85, 65, 78, 0, 88, 85, 69, 0, 88, 85, 78, 0, 89, 65, 0, 89, 65, 78, 0, 89, 65, 78, 71, 0, 89, 65, 79, 0, 89, 69, 0, 89, 73, 0, 89, 73, 78, 0, 89, 73, 78, 71, 0, 89, 79, 0, 89, 79, 78, 71, 0, 89, 79, 85, 0, 89, 85, 0, 89, 85, 65, 78, 0, 89, 85, 69, 0, 89, 85, 78, 0, 90, 65, 0, 90, 65, 73, 0, 90, 65, 78, 0, 90, 65, 78, 71, 0, 90, 65, 79, 0, 90, 69, 0, 90, 69, 73, 0, 90, 69, 78, 0, 90, 69, 78, 71, 0, 90, 72, 65, 0, 90, 72, 65, 73, 0, 90, 72, 65, 78, 0, 90, 72, 65, 78, 71, 0, 90, 72, 65, 79, 0, 90, 72, 69, 0, 90, 72, 69, 73, 0, 90, 72, 69, 78, 0, 90, 72, 69, 78, 71, 0, 90, 72, 73, 0, 90, 72, 79, 78, 71, 0, 90, 72, 79, 85, 0, 90, 72, 85, 0, 90, 72, 85, 65, 0, 90, 72, 85, 65, 73, 0, 90, 72, 85, 65, 78, 0, 90, 72, 85, 65, 78, 71, 0, 90, 72, 85, 73, 0, 90, 72, 85, 78, 0, 90, 72, 85, 79, 0, 90, 73, 0, 90, 79, 78, 71, 0, 90, 79, 85, 0, 90, 85, 0, 90, 85, 65, 78, 0, 90, 85, 73, 0, 90, 85, 78, 0, 90, 85, 79, 0}, new char [] {(char) 1, (char) 2, (char) 3, (char) 9, (char) 4, (char) 5, (char) 6, (char) 7, (char) 8, (char) 10, (char) 11, (char) 12, (char) 13, (char) 14, (char) 15, (char) 16, (char) 17, (char) 18, (char) 19, (char) 20, (char) 21, (char) 22, (char) 23, (char) 24, (char) 25, (char) 26, (char) 27, (char) 28, (char) 29, (char) 30, (char) 31, (char) 32, (char) 33, (char) 34, (char) 35, (char) 36, (char) 37, (char) 38, (char) 39, (char) 40, (char) 41, (char) 42, (char) 43, (char) 44, (char) 45, (char) 46, (char) 47, (char) 48, (char) 49, (char) 50, (char) 51, (char) 52, (char) 53, (char) 54, (char) 55, (char) 56, (char) 57, (char) 58, (char) 59, (char) 60, (char) 61, (char) 62, (char) 63, (char) 64, (char) 65, (char) 66, (char) 67, (char) 68, (char) 69, (char) 70, (char) 71, (char) 72, (char) 73, (char) 74, (char) 75, (char) 76, (char) 77, (char) 78, (char) 79, (char) 80, (char) 81, (char) 82, (char) 83, (char) 84, (char) 85, (char) 86, (char) 87, (char) 88, (char) 89, (char) 90, (char) 91, (char) 92, (char) 93, (char) 94, (char) 95, (char) 96, (char) 97, (char) 98, (char) 99, (char) 100, (char) 101, (char) 102, (char) 103, (char) 104, (char) 105, (char) 106, (char) 107, (char) 108, (char) 109, (char) 110, (char) 111, (char) 112, (char) 113, (char) 114, (char) 115, (char) 116, (char) 117, (char) 118, (char) 119, (char) 120, (char) 121, (char) 122, (char) 123, (char) 124, (char) 125, (char) 126, (char) 127, (char) 128, (char) 129, (char) 130, (char) 131, (char) 132, (char) 133, (char) 134, (char) 135, (char) 136, (char) 137, (char) 138, (char) 139, (char) 140, (char) 141, (char) 142, (char) 143, (char) 144, (char) 145, (char) 146, (char) 147, (char) 148, (char) 149, (char) 150, (char) 151, (char) 152, (char) 153, (char) 154, (char) 155, (char) 156, (char) 157, (char) 158, (char) 159, (char) 160, (char) 161, (char) 162, (char) 163, (char) 164, (char) 165, (char) 166, (char) 167, (char) 168, (char) 169, (char) 170, (char) 171, (char) 172, (char) 173, (char) 174, (char) 175, (char) 176, (char) 177, (char) 178, (char) 179, (char) 180, (char) 181, (char) 182, (char) 183, (char) 184, (char) 185, (char) 186, (char) 187, (char) 188, (char) 189, (char) 190, (char) 191, (char) 192, (char) 193, (char) 194, (char) 195, (char) 196, (char) 197, (char) 198, (char) 199, (char) 200, (char) 201, (char) 202, (char) 203, (char) 204, (char) 205, (char) 206, (char) 207, (char) 208, (char) 209, (char) 210, (char) 211, (char) 212, (char) 213, (char) 214, (char) 215, (char) 216, (char) 217, (char) 218, (char) 219, (char) 220, (char) 221, (char) 222, (char) 223, (char) 224, (char) 225, (char) 226, (char) 227, (char) 228, (char) 229, (char) 230, (char) 231, (char) 232, (char) 233, (char) 234, (char) 235, (char) 236, (char) 237, (char) 238, (char) 239, (char) 240, (char) 241, (char) 242, (char) 243, (char) 244, (char) 245, (char) 246, (char) 247, (char) 248, (char) 249, (char) 250, (char) 251, (char) 252, (char) 253, (char) 254, (char) 255, (char) 256, (char) 257, (char) 258, (char) 259, (char) 260, (char) 261, (char) 262, (char) 263, (char) 264, (char) 265, (char) 266, (char) 267, (char) 268, (char) 269, (char) 270, (char) 271, (char) 272, (char) 273, (char) 274, (char) 275, (char) 276, (char) 277, (char) 278, (char) 279, (char) 280, (char) 281, (char) 282, (char) 283, (char) 284, (char) 285, (char) 286, (char) 287, (char) 288, (char) 289, (char) 290, (char) 291, (char) 292, (char) 293, (char) 294, (char) 295, (char) 296, (char) 297, (char) 298, (char) 299, (char) 300, (char) 301, (char) 302, (char) 303, (char) 304, (char) 305, (char) 306, (char) 307, (char) 308, (char) 309, (char) 310, (char) 311, (char) 312, (char) 313, (char) 314, (char) 315, (char) 316, (char) 317, (char) 318, (char) 319, (char) 320, (char) 321, (char) 322, (char) 323, (char) 324, (char) 325, (char) 326, (char) 327, (char) 328, (char) 329, (char) 330, (char) 331, (char) 332, (char) 333, (char) 334, (char) 335, (char) 336, (char) 337, (char) 338, (char) 339, (char) 340, (char) 341, (char) 342, (char) 343, (char) 344, (char) 345, (char) 346, (char) 347, (char) 348, (char) 349, (char) 350, (char) 351, (char) 352, (char) 353, (char) 354, (char) 355, (char) 356, (char) 357, (char) 358, (char) 359, (char) 360, (char) 361, (char) 362, (char) 363, (char) 364, (char) 365, (char) 366, (char) 367, (char) 368, (char) 369, (char) 370, (char) 371, (char) 372, (char) 373, (char) 374, (char) 375, (char) 376, (char) 377, (char) 378, (char) 379, (char) 380, (char) 381, (char) 382, (char) 383, (char) 384, (char) 385, (char) 386, (char) 387, (char) 388, (char) 389, (char) 390, (char) 391, (char) 392, (char) 393, (char) 394, (char) 395, (char) 396, (char) 397, (char) 398, (char) 399, (char) 400, (char) 401, (char) 402, (char) 403, (char) 404, (char) 405, (char) 406, (char) 407, (char) 408, (char) 409, (char) 410, (char) 411, (char) 412, (char) 413, (char) 414, (char) 415, (char) 416, (char) 417, (char) 418, (char) 419, (char) 420, (char) 421, (char) 422}),
         };
        private static char [] _updIds = new char [] { (char) 1 , (char) 2 , (char) 3 , (char) 4 , (char) 33 , (char) 46 , (char) 97 , (char) 98 , (char) 99 , (char) 100 , (char) 101 , (char) 102 , (char) 103 , (char) 104 , (char) 105 , (char) 106 , (char) 107 , (char) 108 , (char) 109 , (char) 110 , (char) 111 , (char) 112 , (char) 113 , (char) 114 , (char) 115 , (char) 116 , (char) 117 , (char) 118 , (char) 119 , (char) 120 , (char) 121 , (char) 122 , (char) 124 , (char) 230 , (char) 231 , (char) 240 , (char) 248 , (char) 295 , (char) 331 , (char) 339 , (char) 448 , (char) 449 , (char) 450 , (char) 451 , (char) 592 , (char) 593 , (char) 594 , (char) 595 , (char) 596 , (char) 597 , (char) 598 , (char) 599 , (char) 600 , (char) 601 , (char) 602 , (char) 603 , (char) 604 , (char) 605 , (char) 606 , (char) 607 , (char) 608 , (char) 609 , (char) 610 , (char) 611 , (char) 612 , (char) 613 , (char) 614 , (char) 615 , (char) 616 , (char) 618 , (char) 619 , (char) 620 , (char) 621 , (char) 622 , (char) 623 , (char) 624 , (char) 625 , (char) 626 , (char) 627 , (char) 628 , (char) 629 , (char) 630 , (char) 632 , (char) 633 , (char) 634 , (char) 635 , (char) 637 , (char) 638 , (char) 640 , (char) 641 , (char) 642 , (char) 643 , (char) 644 , (char) 646 , (char) 647 , (char) 648 , (char) 649 , (char) 650 , (char) 651 , (char) 652 , (char) 653 , (char) 654 , (char) 655 , (char) 656 , (char) 657 , (char) 658 , (char) 659 , (char) 660 , (char) 661 , (char) 662 , (char) 663 , (char) 664 , (char) 665 , (char) 667 , (char) 668 , (char) 669 , (char) 671 , (char) 672 , (char) 673 , (char) 674 , (char) 675 , (char) 676 , (char) 677 , (char) 678 , (char) 679 , (char) 680 , (char) 688 , (char) 689 , (char) 690 , (char) 692 , (char) 695 , (char) 700 , (char) 712 , (char) 716 , (char) 720 , (char) 721 , (char) 728 , (char) 734 , (char) 736 , (char) 737 , (char) 740 , (char) 768 , (char) 769 , (char) 771 , (char) 772 , (char) 774 , (char) 776 , (char) 778 , (char) 779 , (char) 783 , (char) 792 , (char) 793 , (char) 794 , (char) 796 , (char) 797 , (char) 798 , (char) 799 , (char) 800 , (char) 804 , (char) 805 , (char) 809 , (char) 810 , (char) 812 , (char) 815 , (char) 816 , (char) 817 , (char) 820 , (char) 825 , (char) 826 , (char) 827 , (char) 828 , (char) 829 , (char) 865 , (char) 946 , (char) 952 , (char) 967 , (char) 1856 , (char) 8214 , (char) 8255 , (char) 8319 , (char) 8593 , (char) 8594 , (char) 8595, (char) 8599, (char) 8600 };

        private static readonly PhonemeConverter _upsConverter;

        #endregion

        //*******************************************************************
        //
        // Private Types
        //
        //*******************************************************************

        #region Private Types

        private class PhoneMap
        {
            internal PhoneMap () { }

#if BUILD_PHONEMAP
            internal PhoneMap (int lcid, PhoneId [] phoneIds)
            {
                _lcid = lcid;
                _phoneIds = phoneIds;
            }
#endif
            internal int _lcid;
            internal PhoneId [] _phoneIds;
        }

        private class PhoneId : IComparer<PhoneId>
        {
            internal PhoneId () { }

#if BUILD_PHONEMAP
            internal PhoneId (string phone, char [] cp)
            {
                _phone = phone;
                _cp = cp;
            }
#endif
            internal string _phone;
            internal char [] _cp;

            int IComparer<PhoneId>.Compare (PhoneId x, PhoneId y)
            {
                return String.Compare(x._phone, y._phone, StringComparison.CurrentCulture);
            }

        }

        /// <summary>
        /// Compressed version for the phone map so that the size for the pronunciation table is small in the dll.
        /// A single large arrays of bytes (ascii) is used to store the 'pron' string. Each string is zero terminated.
        /// A single large array of char is used to store the code point for the 'pron' string. Each binary array for the pron by default 
        /// has a length of 1 character. If the length is greater than 1, then the 'pron' string is appended with -1 values, one per extra code 
        /// point.
        /// </summary>
        private class PhoneMapCompressed
        {
            internal PhoneMapCompressed () { }

            internal PhoneMapCompressed (int lcid, int count, byte [] phoneIds, char [] cps)
            {
                _lcid = lcid;
                _count = count;
                _phones = phoneIds;
                _cps = cps;
            }

            // Language Id
            internal int _lcid;

            // Number of phonemes
            internal int _count;

            // Array of zero terminated ascii strings 
            internal byte [] _phones;

            // Array of code points for the 'pron'. By default each code point for a 'pron' is 1 char long, unless the 'pron' string is prepended with -1
            internal char [] _cps;
        }

        #endregion
    }
}
