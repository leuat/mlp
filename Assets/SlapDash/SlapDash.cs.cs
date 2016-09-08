using UnityEngine;
using System.Collections;
using System.Collections.Generic;


namespace LemonSpawn
{

    public class Syllable {
        public string syllable;
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
        public Language(string n, int min, int max, int DC, bool allowDCE)
        {
            name = n;
            minSyllables = min;
            maxSyllables = max;
            maxDoubleC = DC;
            allowDoubleCEnd = allowDCE;
        }

        public void InitializeSyllables(string [] list, Syllable.Types type)
        {
            foreach (string s in list)
                syllables.Add(new Syllable(s, type));
        }

        public List<Syllable> findType(Syllable.Types type)
        {
            List<Syllable> lst = new List<Syllable>();
            foreach (Syllable s in syllables)
            {
                if (s.type == type)
                    lst.Add(s);
            }
            return lst;
        }

        public Syllable findRandomType(Syllable.Types t, System.Random rnd)
        {
            List<Syllable> lst = findType(t);
            return lst[rnd.Next() % lst.Count];
        }
        public Syllable findRandom(System.Random rnd)
        {
            return syllables[rnd.Next() % syllables.Count];
        }

        public Syllable findRandomTypes(Syllable.Types[] tlst, System.Random rnd)
        {
            bool ok = false;
            int cnt = 0;
            List<Syllable> lst = null;
            while (!ok) { 
                Syllable.Types t = tlst[rnd.Next()%tlst.Length];
                lst = findType(t);
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


        public Syllable findBasedOnSyllable(Syllable prev, System.Random rnd, bool isFinal)
        {

            Syllable.Types[] afterV = new Syllable.Types[] { Syllable.Types.C, Syllable.Types.VC, Syllable.Types.CC, Syllable.Types.CV };

            if (isFinal)
            {
                // Do not allow for ending in consonant clusters
                afterV = new Syllable.Types[] { Syllable.Types.C, Syllable.Types.VC, Syllable.Types.CV };

            }

            if (prev.type == Syllable.Types.C)
                return findRandomTypes(new Syllable.Types[] {Syllable.Types.VC, Syllable.Types.V, Syllable.Types.CV }, rnd);
            if (prev.type == Syllable.Types.V || prev.type == Syllable.Types.CV)
                return findRandomTypes(afterV, rnd);
            if (prev.type == Syllable.Types.CC)
                return findRandomType(Syllable.Types.V, rnd);
            if (prev.type == Syllable.Types.VC)
                return findRandomType(Syllable.Types.V, rnd);

            return null;
        }

        public string GenerateWord(System.Random r)
        {
            int noSyllables = r.Next() % (maxSyllables - minSyllables) + minSyllables;
            List<Syllable> syll = new List<Syllable>();
            Syllable s = findRandom(r);
//            Debug.Log(s.syllable);
            syll.Add(s);
//            Debug.Log(noSyllables + " " + minSyllables + " " + maxSyllables);
  //          return "";
            for (int i=0;i<noSyllables-1;i++)
            {

                s = findBasedOnSyllable(s, r, i==noSyllables-2);
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
                            if (!exceptDoubles.Contains(sb.syllable[0].ToString()))
                                word += sb.syllable[0];
                            remaining--;
                        }
                    }
                }
                word += sb.syllable;
            }
            return word;
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


        public void Initialize()
        {
            InitializeHapanese();
            InitializeKvorsk();
            InitializePinglish();
        }

        void InitializeHapanese()
        {
            Language l = new Language("Hapanese", 2, 6, 1, false);
            languages.Add(l);
            string[] C = new string[] { "n" };
            string[] V = new string[] { "u", "a","e","i","o" };

            string[] CV = new string[] {"wa", "we","wi","wo", "wu", "ka", "ke", "ki", "ko", "sa", "se", "so", "su",
                                        "ta", "te", "chi", "to", "tsu", "na", "ne", "ni", "no", "nu",
                                        "ma", "me", "mi", "mo", "mu", "ha", "he", "hi", "ho", "hu",
                                        "ra", "re", "ri", "ro", "ru", "ga", "ge", "gi", "go", "gu",
                                        "za", "zu", "ji", "ze", "zo", "da", "de", "do", "ba", "be", "bi", "bo", "bu",
                                        "pa", "pe", "pi", "po", "pu", "ya", "yu", "yo", "chu", "cha", "cho",
                                        "shu", "sha", "sho", "kyu", "kya", "kyo", "ryu", "rya", "ryo",
                                        "ja", "ju", "jo", "bya", "byo", "byu", "tsu", "fu" };

            l.InitializeSyllables(C, Syllable.Types.C);
            l.InitializeSyllables(V, Syllable.Types.V);
            l.InitializeSyllables(CV, Syllable.Types.CV);

            l.exceptDoubles = "wshzjdbrft";


        }

        void InitializeKvorsk()
        {
            Language l = new Language("Kvorsk", 2, 7, 1, true);
            languages.Add(l);
            string[] C = new string[] { "b", "d", "f", "g", "h", "j", "k", "l", "m", "n", "p", "r", "s", "t", "v" };
            string[] V = new string[] { "a", "e", "i", "o", "u", "æ","ø","å", "a", "e", "i", "o", "y" };

            string[] CC = new string[] {"kj", "skj", "sj", "fr","bl","br","dr","fj","kl","kn","kr","gr","gl","hj","pr","pj","pl","tj","tr","tv","vr",
                                        "skr"};

            l.InitializeSyllables(C, Syllable.Types.C);
            l.InitializeSyllables(V, Syllable.Types.V);
            l.InitializeSyllables(CC, Syllable.Types.CC);
            l.exceptDoubles = "hvj";


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

