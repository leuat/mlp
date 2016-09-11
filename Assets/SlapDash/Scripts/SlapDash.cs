using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LemonSpawn
{

    public class Syllable {
        public static int Prefix = 1;
        public static int Infix = 1<<1;
        public static int Postfix = 1<<2;

        public string syllable;
        public int fix = Prefix | Infix | Postfix;
//        public bool isPrefix;
//        public bool isPostfix;
//        public bool isInfix;
        public enum Types { C, CC, V, VC, CV };
        public Types type = Types.C;

        public Syllable(string s, Types t) {
            syllable = s;
            type = t;

        }

        public Syllable(string s, bool _isPrefix, bool _isPostfix, bool _isInfix, Types t)
        {
            syllable = s;
  /*          isPrefix = _isPrefix;
            isPostfix = _isPostfix;
            isInfix = _isInfix;*/
            type = t; 
        }

    }
        

    public class Language
    {
        public int minSyllables;
        public int maxSyllables;
        public int maxDoubleC = 1;
        public bool allowDoubleCEnd = false;
        public List<Syllable> syllables = new List<Syllable>();
        public string name = "";
        public string exceptDoubles = "";
        public string exceptDoubleEndings = ""; // disallow stuff like "hann" "mann" "kunn" etc
        public Language(string n, int min, int max, int DC, bool allowDCE)
        {
            name = n;
            minSyllables = min;
            maxSyllables = max;
            maxDoubleC = DC;
            allowDoubleCEnd = allowDCE;
        }

        public void SetFix(string [] syll, int fix) {
            foreach (string s in syll)
                foreach (Syllable sb in syllables) {
                    if (s == sb.syllable) {
                        sb.fix = fix;
//                        Debug.Log(fix + " for " + s);
                        }
            }

         
        }

        public void InitializeSyllables(string [] list, Syllable.Types type)
        {
            foreach (string s in list)
                syllables.Add(new Syllable(s, type));
        }

        public List<Syllable> findType(Syllable.Types type, int fix)
        {
            List<Syllable> lst = new List<Syllable>();
            foreach (Syllable s in syllables)
            {
                if (s.type == type && ((s.fix & fix) == fix))
                    lst.Add(s);
            }
            return lst;
        }

        public Syllable FindSyllable(string syll) {
            foreach (Syllable sb in syllables)
                if (sb.syllable == syll)
                    return sb;

            return null;
               
        }

        public Syllable findRandomType(Syllable.Types t, System.Random rnd, int fix)
        {
            List<Syllable> lst = findType(t, fix);
            return lst[rnd.Next() % lst.Count];
        }
        public Syllable findRandom(System.Random rnd, int fix)
        {
            Syllable sb = FindSyllable("heim");
//            if (sb!=null)
  //              Debug.Log(sb.fix);

            List<Syllable> lst = new List<Syllable>();
            foreach (Syllable s in syllables)
            {
            //0x001 & 0x011 == 0x001 
                if (((s.fix & fix) == fix)) {
                    lst.Add(s);
                   // Debug.Log("Added candidate : " + fix + " : " + s.syllable + " : sb.fix= "+s.fix);
                    }
            }
            return lst[rnd.Next() % lst.Count];
        }

        public Syllable findRandomTypes(Syllable.Types[] tlst, System.Random rnd, int fix)
        {
            bool ok = false;
            int cnt = 0;
            List<Syllable> lst = null;
            while (!ok) { 
                Syllable.Types t = tlst[rnd.Next()%tlst.Length];
                lst = findType(t, fix);
                if (lst.Count != 0)
                    ok = true;
                cnt++;
                if (cnt>=1000)
                {
                    Debug.Log("LANGUAGE ERROR NO USABLE SYLLABLE FOUND");
                    return null;
                }
            }

            return lst[rnd.Next() % lst.Count];
        }


        public Syllable findBasedOnSyllable(Syllable prev, System.Random rnd, bool isFinal, int fix)
        {

            Syllable.Types[] afterV = new Syllable.Types[] { Syllable.Types.C, Syllable.Types.CC, Syllable.Types.CV };

            if (isFinal)
            {
                // Do not allow for ending in consonant clusters
                afterV = new Syllable.Types[] { Syllable.Types.C, Syllable.Types.CV, Syllable.Types.VC };

            }

//            Debug.Log("prev: " + prev.type);

            if (prev.type == Syllable.Types.C)
                return findRandomTypes(new Syllable.Types[] {Syllable.Types.VC, Syllable.Types.V, Syllable.Types.CV }, rnd, fix);
            if (prev.type == Syllable.Types.V || prev.type == Syllable.Types.CV)
                return findRandomTypes(afterV, rnd, fix);
            if (prev.type == Syllable.Types.CC)
                return findRandomType(Syllable.Types.V, rnd, fix);
            if (prev.type == Syllable.Types.VC)
                return findRandomType(Syllable.Types.V, rnd, fix);

            return null;
        }

        public string RemoveIllegalDoubles(string s) {

            string res = s[0].ToString();
            char prev = s[0];
            bool hasChanged = false;
            for (int i=1;i<s.Length;i++) {
                bool ok = true;
                if ((prev == s[i])  && exceptDoubles.Contains(s[i].ToString())) {
                    ok = false;
                    Debug.Log("Removing " + prev + " in " + s); 
                    hasChanged = true;
                    }

                if (ok) {
                    prev = s[i];
                    res+=s[i];
                    }
            }
/*            if (hasChanged)
                Debug.Log("Before:" + s + " , " */
            return res;
        }

        public string GenerateWord(System.Random r)
        {
            int noSyllables = r.Next() % (maxSyllables - minSyllables) + minSyllables;
            List<Syllable> syll = new List<Syllable>();
            Syllable s = findRandom(r, Syllable.Prefix);
//            Debug.Log(s.syllable);
            syll.Add(s);
//            Debug.Log(noSyllables + " " + minSyllables + " " + maxSyllables);
  //          return "";
            for (int i=0;i<noSyllables-1;i++)
            {
                int fix = Syllable.Infix;
                if (i==noSyllables-2) {
                    fix = Syllable.Postfix;
                    }

                s = findBasedOnSyllable(s, r, i==noSyllables-2, fix);
//                Debug.Log(s.syllable + " : " + fix);
                if (s != null)
                    syll.Add(s);
            }

            string word = "";
            int remaining = maxDoubleC;
            for (int i=0;i<syll.Count ;i++)
            {
                Syllable sb = syll[i];
                if (i>0 && remaining>0)
                {
                    Syllable prev = syll[i - 1];
                    if (i==syll.Count-1 && allowDoubleCEnd)
                    if (prev.type == Syllable.Types.CV || prev.type == Syllable.Types.V) {
                        if (r.NextDouble()>0.66)
                        {
                            // Double consonant
                            if (!(i==syll.Count-1 && exceptDoubleEndings.Contains(sb.syllable[0].ToString())))
//                            Debug.Log("DOUBLE:" + sb.syllable[0]);
                            if (!exceptDoubles.Contains(sb.syllable[0].ToString()))
                                word += sb.syllable[0];
                            remaining--;
                        }
                    }
                }
                word += sb.syllable;
            }
            return RemoveIllegalDoubles(word);
        }

    }


    public class SlapDash
    {
     
        public List<Language> languages = new List<Language>();


        public string getWord(System.Random r)
        {
            Language l = languages[r.Next() % languages.Count];
            return l.GenerateWord(r);

        }

        public string getWord(string language, System.Random r)
        {
            foreach (Language l in languages)
                if (l.name == language)
                    return l.GenerateWord(r);

            return "Language not found";
        }


        void Initialize()
        {
            InitializeHapanese();
            InitializeKvorsk();
            InitializePinglish();
            InitializeLespanol();
           // InitializeHapanese2();

        }

        public SlapDash() {
            Initialize();
        }


        void InitializeLespanol() {
            Language l = new Language("Lespanol", 2, 5, 0, false);
            languages.Add(l);
            //string[] C = new string[] { "b","c","d","f","g","h","l","ll","m","n","ñ","j","p","r","s","t","v","z" };
            string[] CCP = new string[] { "br","cr","cl","bl","fl","fr","gr","gl",
                                          "pr","pl","tr"};

            string[] CCI = new string[] { "dr","lm","lg","lz","lp","lt","lv","mr" ,"nd","ng","nr","ns","nt","nv","nz", "nc", "ñ", "ñd",
                                        "ps", "pt","pr","sc", "sg", "st","sl","sp", "vr"};

            string[] CCPR = new string[] {"esp","esqu","esc","esb","esm","est", "alc", "alm","alp", "im","in","con","di"};
            
            string[] V = new string[] { "a","e","o","ue" };


            string[] CV = new string[] { "ba","be","bi","bo","bu","cu", "que","ce","co","ci", "da","de","du","di","do",
                                         "fa","fe","fi","fo","fu", "ga", "ge", "gi", "gu","go",
                "ha","he","hi","ho","hu","la","le","li","lo","lu","lla","lle","llo","llu","ma","me","mi","mo","mu",
                "na","ne","ni","no","nu","ña","ñe","ñi","ño","ñu","ja","je","ji","jo","ju","pa","pe","pi","po","pu",
                "ra","re","ri","ro","ru","sa","se","si","so","su","ta","te","ti","to","tu","va","ve","vi","vo","vu",
                "za","ze","zi","zo","zu", "qu" };

            string[] PostCV = new string[] {"cu","ca","do","da","fo","fa","go","ga","ho","ha","lo","la",
                "llo","lla","mo","ma","no","na","ño","ña","jo","ja","po","pa","dre","so","sa","to","ta","vo","va"
            };

//            string[] PreVC = new string[] {"im", emp};

            l.InitializeSyllables(CCP, Syllable.Types.CC);
            l.InitializeSyllables(CCI, Syllable.Types.CC);
            l.InitializeSyllables(CCPR, Syllable.Types.VC);
            l.InitializeSyllables(V, Syllable.Types.V);
            l.InitializeSyllables(CV, Syllable.Types.CV);

            l.InitializeSyllables(PostCV, Syllable.Types.CV);

            l.SetFix(CCP, Syllable.Prefix | Syllable.Infix);
            l.SetFix(CCI, Syllable.Infix);
            l.SetFix(CCPR, Syllable.Prefix);

            l.SetFix(CV, Syllable.Prefix | Syllable.Infix);
            l.SetFix(PostCV, Syllable.Postfix);



            string[] POSTC = new string[] {"ción","landia", "lita","lito","cito","cita","cola"};
            string[] POSTV = new string[] {"o","a", "er", "as","os","ar","ear","es", "eco","eo","ana","eño","ísma","ísmo","ita","ito","eza",
                                           "azo","al","an" };

             

            l.InitializeSyllables(POSTC, Syllable.Types.CV);
            l.SetFix(POSTC, Syllable.Postfix);
            l.InitializeSyllables(POSTV, Syllable.Types.VC);
            l.SetFix(POSTV, Syllable.Postfix);

            l.exceptDoubles = "aeiouy";

        }


        void InitializeHapanese()
        {
            Language l = new Language("Hapanese", 2, 6, 1, false);
            languages.Add(l);
            string[] C = new string[] { "n" };
            string[] V = new string[] { "u", "a","e","i","o" };

            string[] CV = new string[] {"wa", "ka", "ke", "ki", "ko", "ku", "sa", "se", "so", "su",
                                        "ta", "te", "chi", "to", "tsu", "na", "ne", "ni", "no", "nu",
                                        "ma", "me", "mi", "mo", "mu", "ha", "he", "hi", "ho",
                                        "ra", "re", "ri", "ro", "ru", "ga", "ge", "gi", "go", "gu",
                                        "za", "zu", "ji", "ze", "zo", "da", "de", "do", "ba", "be", "bi", "bo", "bu",
                                        "pa", "pe", "pi", "po", "pu", "ya", "yu", "yo", "chu", "cha", "cho",
                                        "shu", "sha", "sho", "kyu", "kya", "kyo", "ryu", "rya", "ryo",
                                        "ja", "ju", "jo", "bya", "byo", "myu", "tsu", "fu", "so" };

            l.InitializeSyllables(C, Syllable.Types.C);
            l.InitializeSyllables(V, Syllable.Types.V);
            l.InitializeSyllables(CV, Syllable.Types.CV);

            l.exceptDoubles = "wshzjdbrft";
            l.exceptDoubleEndings = "n";


        }

        void InitializeHapanese2()
        {
            Language l = new Language("日のん語", 2, 6, 0, false);
            languages.Add(l);
            string[] C = new string[] { "ん" };
            string[] V = new string[] { "う", "あ","え","い","お" };

            string[] CV = new string[] {"わ",  "か", "け", "き", "こ", "く", "さ", "せ", "そ", "す",
                                        "た", "て", "ち", "と", "つ", "な", "ね", "に", "の", "ぬ",
                                        "ま", "め", "み", "も", "む", "は", "へ", "ひ", "ほ",
                                        "ら", "れ", "り", "ろ", "る", "が", "げ", "ぎ", "ご", "ぐ",
                                        "ざ", "ず", "じ", "ぜ", "ぞ", "そ", "だ", "で", "ど", "ば", "べ", "び", "ぼ", "ぶ",
                                        "ぱ", "ぺ", "ぴ", "ぽ", "ぷ", "や", "ゆ", "よ", "ちゅ", "ちゃ", "ちょ",
                                        "しゅ", "しゃ", "しょ", "きゅ", "きゃ", "きょ", "りゅ", "りゃ", "りょ",
                                        "じゃ", "じゅ", "じょ", "びゃ", "びょ", "みゅ", "つ", "ふ" };

            l.InitializeSyllables(C, Syllable.Types.C);
            l.InitializeSyllables(V, Syllable.Types.V);
            l.InitializeSyllables(CV, Syllable.Types.CV);

            l.exceptDoubles = "wshzjdbrft";
            l.exceptDoubleEndings = "ん";


        }

        void InitializeKvorsk()
        {
            Language l = new Language("Kvorsk", 3, 6, 1, true);
            languages.Add(l);
            string[] C = new string[] { "b", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "r", "s", "t", "v" };
            string[] V = new string[] { "a", "e", "i", "o", "u", "æ","ø","å", "a", "e", "i", "o", "y" };

            string[] CC = new string[] {"kj", "skj", "sj", "fr","bl","br","dr","fj","kl","kn","kr","gr","kv", "gl","hj","pr","pj","pl","tj","tr","tv","vr",
                                        "skr"};

            l.InitializeSyllables(C, Syllable.Types.C);
            l.InitializeSyllables(V, Syllable.Types.V);
            l.InitializeSyllables(CC, Syllable.Types.CC);

            string[] POSTC = new string[] {"bu","heim","vatn","dal","hov","gard","aker","nes","fjell","mark","land","stad","tveit","torp","by",
                                        "bø","tun","rud", "torp","hus","hoff","vik","mark", "lie"};
            string[] POSTV = new string[] {"øy","ås", "ing", "um", "eng"};

             

            l.InitializeSyllables(POSTC, Syllable.Types.CV);
            l.SetFix(POSTC, Syllable.Postfix);
            l.InitializeSyllables(POSTV, Syllable.Types.VC);
            l.SetFix(POSTV, Syllable.Postfix);

            l.exceptDoubles = "hvjåøæieuao";
            l.exceptDoubleEndings = "m";
//            l.maxDoubleC = 0;

            l.SetFix(CC, Syllable.Prefix | Syllable.Infix);
            l.SetFix(new string[] {"h"}, Syllable.Prefix | Syllable.Infix);


/*          døgeaker  igle  øhyland  obla  ofafri  noti  praltun  filiv  ajæfjå  byvu  blæfjuås  yba  klohæøy  vyko  
            kvyhmark  sjasuby  nadstad  ybråtve  tykvoøy  yfiing  predagard  esukva  knuing  epjå  luhe  
            æglafjell  fitåttorp  iking  ked  blibæj  ifrøti  glusjomark  adasjæ  knib  tjigletorp  sjiviøy  rilbu  
            donås  mytvå  pløås  øjågdal  jåb  pisjo  vrækom  hjobb  pjaklåing  eblahju  fræg  sjepjåttveit  korobby  skjubu  
            hjøfjæøy  ejøøy  tvibu  obæshov  dragostad  amitås  våås  kjotjyøy  sjabæås  klineås  kåbrof  klæfa  uting  nov  æjiving  
            adeg  avland  ekvålfjell  ditorp  tjaskji  plåprastad  fetaås  tvørrud  jåroj  kvifjell  efile  pumo  drimehov  fræsjøbbu  
            druvettorp  tæbri  fraøy  ækoås  råmiv  skjåklahus  pleress  jopiing  hjifrykk  meøy  krytorp  gæhoff  kræjip  kniøy  
            hipryd  krafro  geging  sahyås  votromark  pleraf          

            */
            /*

            vrosøland  livro  knytamark  aplæbla  abtveit  ypef  hjaås  vris  ødyhoff  vrusiås  dræskræ  badheim  gråås  æbrypji 
             sjøing  ilopp  tvej  pobrehus  ligard  åtitje  tatvin  låtiing  dridip  kjamaås  sjiblå  alømark  miskry  avræsås  vreås  
             nybru  såiing  ægleddal  vrupli  kviratorp  odrom  ulæstad  dyj  druing  skrannes  kuøy  knågra  doble  søtun  frikjaøy  
             vutævatn  teøy  apro  drepjå  veskre  hjopenes  tøpafjell  edåvra  kvåggard  ædrå  kjoff  skjoing  lykvi  kjiaker  åtrøiing 
              vite  saknyhov  ubrom  aglij  deteøy  kromo  tapybø  akroås  tøglu  tilås  bejing  kjeås  sjep  ufjøiing  pøv  skjiknohoff 
               fytvåås  plevbø  tjekmark  knaje  nej  miskrygard  prøås  afaing  udabla  skjykje  faviing  usjemark  kliklån  skjøj  
               kvatvåvatn  æhiggard  vubb  drøkvam  fjabætorp  vok  askjiås  movunnes  metun  ufripp  kus 

            */
            }

        void InitializePinglish()
        {
            Language l = new Language("Pinglish", 3, 7, 1, false);
            languages.Add(l);
            string[] C = new string[] { "b", "c", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "q", "r", "s", "t", "v", "w", "z" };
            string[] V = new string[] { "a", "e", "i", "o", "u", "y"};

            string[] CC = new string[] {"bl","br","cl","cr","dr","dw","fh","fl","fr","gh","gl","gn","gr","gw","ph","pl","pr","ps","st","sph","sp","sq","sw",
                                        "th","tr","tw","wh"};

            l.InitializeSyllables(C, Syllable.Types.C);
            l.InitializeSyllables(V, Syllable.Types.V);
            l.InitializeSyllables(CC, Syllable.Types.CC);
            l.exceptDoubles = "chvjklqrwxz";


        }



    }

}

