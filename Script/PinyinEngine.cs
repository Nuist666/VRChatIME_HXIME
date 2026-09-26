
using System;
using System.Linq;
using System.Text;
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

namespace HX2xianglong90.HXIME{
public class PinyinEngine : UdonSharpBehaviour
{
    // Segmenter 分词器
    private string[] pinyin_dict = {
            "a", "ai", "an", "ang", "ao",
            "ba", "bai", "ban", "bang", "bao", "bei", "ben", "beng", "bi", "bian", 
            "biao", "bie", "bin", "bing", "bo", "bu",
            "ca", "cai", "can", "cang", "cao", "ce", "cen", "ceng", "cha", "chai", 
            "chan", "chang", "chao", "che", "chen", "cheng", "chi", "chong", 
            "chou", "chu", "chua", "chuai", "chuan", "chuang", "chui", "chun", 
            "chuo", "ci", "cong", "cou", "cu", "cuan", "cui", "cun", "cuo",
            "da", "dai", "dan", "dang", "dao", "de", "dei", "den", "deng", "di", 
            "dian", "diao", "die", "ding", "diu", "dong", "dou", "du", "duan", 
            "dui", "dun", "duo",
            "e", "ei", "en", "er",
            "fa", "fan", "fang", "fei", "fen", "feng", "fo", "fou", "fu",
            "ga", "gai", "gan", "gang", "gao", "ge", "gei", "gen", "geng", 
            "gong", "gou", "gu", "gua", "guai", "guan", "guang", "gui", "gun", "guo",
            "ha", "hai", "han", "hang", "hao", "he", "hei", "hen", "heng", 
            "hong", "hou", "hu", "hua", "huai", "huan", "huang", "hui", "hun", "huo",
            "ji", "jia", "jian", "jiang", "jiao", "jie", "jin", "jing", "jiong", 
            "jiu", "ju", "juan", "jue", "jun",
            "ka", "kai", "kan", "kang", "kao", "ke", "kei", "ken", "keng", 
            "kong", "kou", "ku", "kua", "kuai", "kuan", "kuang", "kui", "kun", "kuo",
            "la", "lai", "lan", "lang", "lao", "le", "lei", "leng", "li", "lia", 
            "lian", "liang", "liao", "lie", "lin", "ling", "liu", "lo", "long", 
            "lou", "lu", "luan", "lue", "lun", "luo", "lv",
            "ma", "mai", "man", "mang", "mao", "me", "mei", "men", "meng", 
            "mi", "mian", "miao", "mie", "min", "ming", "miu", "mo", "mou", "mu",
            "na", "nai", "nan", "nang", "nao", "ne", "nei", "nen", "neng", 
            "ni", "nian", "niang", "niao", "nie", "nin", "ning", "niu", "nong", 
            "nou", "nu", "nuan", "nue", "nuo", "nv", "nve",
            "o", "ou",
            "pa", "pai", "pan", "pang", "pao", "pei", "pen", "peng", "pi", 
            "pian", "piao", "pie", "pin", "ping", "po", "pou", "pu",
            "qi", "qia", "qian", "qiang", "qiao", "qie", "qin", "qing", 
            "qiong", "qiu", "qu", "quan", "que", "qun",
            "ran", "rang", "rao", "re", "ren", "reng", "ri", "rong", "rou", 
            "ru", "ruan", "rui", "run", "ruo",
            "sa", "sai", "san", "sang", "sao", "se", "sen", "seng", "sha", 
            "shai", "shan", "shang", "shao", "she", "shei", "shen", "sheng", 
            "shi", "shou", "shu", "shua", "shuai", "shuan", "shuang", "shui", 
            "shun", "shuo", "si", "song", "sou", "su", "suan", "sui", "sun", "suo",
            "ta", "tai", "tan", "tang", "tao", "te", "teng", "ti", "tian", 
            "tiao", "tie", "ting", "tong", "tou", "tu", "tuan", "tui", "tun", "tuo",
            "wa", "wai", "wan", "wang", "wei", "wen", "weng", "wo", "wu",
            "xi", "xia", "xian", "xiang", "xiao", "xie", "xin", "xing", "xiong", 
            "xiu", "xu", "xuan", "xue", "xun",
            "ya", "yan", "yang", "yao", "ye", "yi", "yin", "ying", "yo", 
            "yong", "you", "yu", "yuan", "yue", "yun",
            "za", "zai", "zan", "zang", "zao", "ze", "zei", "zen", "zeng", 
            "zha", "zhai", "zhan", "zhang", "zhao", "zhe", "zhei", "zhen", 
            "zheng", "zhi", "zhong", "zhou", "zhu", "zhua", "zhuai", "zhuan", 
            "zhuang", "zhui", "zhun", "zhuo", "zi", "zong", "zou", "zu", 
            "zuan", "zui", "zun", "zuo"
    };
    private int max_pinyin_len = 6;
    private string[] short_pinyin_dict = {"g", "h", "m", "p", "b", "w", "k", "a", "e", "s", "x", "y", "c", "t", "l", "j", "d", "o", "n", "q", "f", "z", "r"};
    private string[] pinyin_syllable_h = {"hun", "huo", "hang", "hong", "heng", "huai", "he", "ha", "hen", "hua", "han", "hai", "huang", "hei", "hu", "hui", "hao", "huan", "hou"};
    private string[] pinyin_syllable_z = {"zhuai", "zhi", "ze", "zhuan", "zha", "zu", "zhong", "zhuang", "za", "zhen", "zei", "zhan", "zai", "zun", "zong", "zhe", "zheng", "zhu", "zhun", "zan", "zeng", "zhai", "zuan", "zou", "zhei", "zi", "zhou", "zhuo", "zang", "zhao", "zen", "zao", "zhua", "zui", "zhui", "zuo", "zhang"};
    private string[] pinyin_syllable_j = {"ju", "jia", "ji", "jie", "jiang", "jin", "juan", "jing", "jun", "jiao", "jiu", "jian", "jue", "jiong"};
    private string[] pinyin_syllable_p = {"pou", "pi", "po", "ping", "pian", "pan", "pang", "pin", "peng", "pa", "pai", "pao", "pei", "pen", "pu", "pie", "piao"};
    private string[] pinyin_syllable_s = {"sa", "san", "shen", "shou", "sha", "shuo", "sen", "shun", "sang", "sai", "su", "shao", "sun", "shu", "shai", "shuan", "sao", "shan", "shei", "shuang", "se", "song", "shui", "shuai", "si", "shang", "shi", "suan", "she", "suo", "sui", "sou", "seng", "shua", "sheng"};
    private string[] pinyin_syllable_q = {"qun", "qin", "quan", "que", "qie", "qi", "qia", "qing", "qiang", "qian", "qu", "qiu", "qiong", "qiao"};
    private string[] pinyin_syllable_c = {"chou", "chuai", "cai", "chang", "ce", "cen", "che", "chong", "chao", "chuo", "chu", "chen", "ceng", "cong", "chuang", "chuan", "chui", "chan", "cu", "cou", "chun", "cao", "cha", "cun", "chai", "cheng", "can", "chi", "cuan", "ca", "chua", "cang", "cui", "ci", "cuo"};
    private string[] pinyin_syllable_d = {"dang", "dun", "dou", "duo", "die", "den", "diu", "dian", "de", "dan", "da", "di", "duan", "dai", "dei", "deng", "du", "dao", "diao", "ding", "dui", "dong"};
    private string[] pinyin_syllable_t = {"tou", "ta", "tai", "tuo", "tan", "tang", "tian", "tun", "teng", "tu", "tao", "tong", "te", "tie", "tui", "ting", "tuan", "tiao", "ti"};
    private string[] pinyin_syllable_f = {"fou", "feng", "fan", "fa", "fu", "fei", "fen", "fo", "fang"};
    private string[] pinyin_syllable_g = {"ga", "gong", "gai", "guang", "gou", "gao", "guo", "gen", "gui", "geng", "ge", "gang", "gu", "gan", "guan", "gei", "gun", "gua", "guai"};
    private string[] pinyin_syllable_l = {"liu", "lou", "lo", "le", "lai", "liao", "long", "lang", "lian", "lao", "liang", "lu", "luo", "lv", "lan", "lie", "ling", "lia", "lun", "lin", "la", "luan", "lei", "li", "lue", "leng"};
    private string[] pinyin_syllable_x = {"xin", "xiang", "xing", "xu", "xia", "xun", "xian", "xi", "xuan", "xiao", "xiong", "xue", "xiu", "xie"};
    private string[] pinyin_syllable_o = {"o", "ou"};
    private string[] pinyin_syllable_m = {"miao", "me", "min", "mai", "mou", "mi", "ming", "miu", "ma", "meng", "men", "man", "mao", "mang", "mian", "mei", "mie", "mu", "mo"};
    private string[] pinyin_syllable_y = {"yue", "ye", "you", "yi", "yan", "ya", "ying", "yang", "yao", "yin", "yuan", "yong", "yu", "yo", "yun"};
    private string[] pinyin_syllable_b = {"ban", "ben", "bian", "bang", "beng", "bi", "bei", "bu", "bo", "bing", "biao", "bai", "bin", "bie", "ba", "bao"};
    private string[] pinyin_syllable_k = {"kao", "kong", "ka", "ken", "kun", "ku", "kai", "kuang", "kua", "kuo", "kei", "kan", "kang", "kou", "kui", "kuai", "keng", "ke", "kuan"};
    private string[] pinyin_syllable_w = {"wa", "wan", "wai", "wei", "weng", "wu", "wen", "wang", "wo"};
    private string[] pinyin_syllable_n = {"nuan", "ning", "nian", "neng", "nie", "nao", "nang", "nai", "nou", "nue", "niang", "niao", "nu", "nan", "ne", "nuo", "nve", "nin", "niu", "nong", "ni", "nei", "nen", "nv", "na"};
    private string[] pinyin_syllable_a = {"ai", "an", "ang", "a", "ao"};
    private string[] pinyin_syllable_r = {"rong", "rou", "reng", "rui", "ran", "rang", "ri", "ruan", "rao", "ruo", "re", "ru", "run", "ren"};
    private string[] pinyin_syllable_e = {"ei", "er", "en", "e"};
    // Candidate Matcher 匹配器
    // Dict Pool 拼音字典数据池
    [SerializeField] private PinyinDict[] dicts;
    [Header("扩展语言词库（按此顺序加入语言切换）")]
    public PinyinDict[] additionalDicts = new PinyinDict[0];
    // 由 HXIMEUI 在 Start 时按预制件上的开关写入；中文与英文始终启用。
    [HideInInspector] public bool enableJapanese = true;
    [HideInInspector] public bool enableKorean = true;

    // HXIMEUI 通过它设置开关（跨脚本调用方法在 Udon 上最稳妥）。
    public void SetLanguageEnabled(bool japanese, bool korean)
    {
        enableJapanese = japanese;
        enableKorean = korean;
    }

    public int NextLanguage(int current)
    {
        int count = additionalDicts == null ? 0 : additionalDicts.Length;
        for (int mode = current + 1; mode < count + 2; mode++)
        {
            if (mode == 1) return mode;
            if (additionalDicts[mode - 2] != null && IsLanguageEnabled(additionalDicts[mode - 2])) return mode;
        }
        return 0;
    }

    // 只对日语、韩语生效；其他语言标签始终视为启用。
    private bool IsLanguageEnabled(PinyinDict dictionary)
    {
        string label = dictionary.languageLabel;
        if (label == "Ja" || label == "JP" || label == "ja" || label == "日本語") return enableJapanese;
        if (label == "Ko" || label == "KO" || label == "ko" || label == "한국어") return enableKorean;
        return true;
    }

    public string LanguageLabel(int mode)
    {
        if (mode == 0) return "En";
        if (mode == 1) return "中";
        return additionalDicts[mode - 2].languageLabel;
    }

    // All languages share indexed lookup and Top-K; Chinese enables extra match tiers.
    public string[] MatchLanguage(int mode, string input, int limit)
    {
        if (additionalDicts == null || mode < 2 || mode - 2 >= additionalDicts.Length)
            return new string[0];
        PinyinDict dictionary = additionalDicts[mode - 2];
        if (dictionary == null || !IsLanguageEnabled(dictionary)) return new string[0];
        return Lookup(dictionary, input, limit, false, false);
    }

    private PinyinDict dict_pool;
    private void Start()
    {
        if (dict_pool == null) SwitchSimp();
    }
    public void SwitchSimp(){ // Excute when dict pool changed
        SelectChineseDictionary(0);
    }
    public void SwitchTrad(){
        SelectChineseDictionary(1);
    }
    private void SelectChineseDictionary(int index)
    {
        dict_pool = dicts != null && index < dicts.Length ? dicts[index] : null;
    }
    private string[] Segment(string pinyinString,string mode="mixed"){
        string pinyin_str = pinyinString.ToLower();
        string[] result = new string[pinyin_str.Length];
        int resultCount = 0; // 当前有效元素数量
        int i = 0;
        int n = pinyin_str.Length;
        
        while (i < n){
            bool matched = false;
            
            // 1. 全拼匹配
            for (int l = Math.Min(max_pinyin_len, n - i); l > 0; l--)
            {
                string substr = pinyin_str.Substring(i, l);
                int idx = Array.IndexOf(pinyin_dict, substr);

                if (idx >= 0 && idx < pinyin_dict.Length && pinyin_dict[idx] == substr)
                {
                    result[resultCount++] = substr; // 直接存入数组
                    i += l;
                    matched = true;
                    break;
                }
            }
            // 2. 短拼匹配
            if (!matched && (mode == "short" || mode == "mixed"))
            {
                if(Array.IndexOf(short_pinyin_dict,pinyin_str[i])!=-1)
                {
                    string pinyinStrCase = pinyin_str[i].ToString();
                    string[] possiblePinyins;
                    switch (pinyinStrCase)
                    {
                        case "h":
                            possiblePinyins = pinyin_syllable_h;
                            break;
                        case "z":
                            possiblePinyins = pinyin_syllable_z;
                            break;
                        case "j":
                            possiblePinyins = pinyin_syllable_j;
                            break;
                        case "p":
                            possiblePinyins = pinyin_syllable_p;
                            break;
                        case "s":
                            possiblePinyins = pinyin_syllable_s;
                            break;
                        case "q":
                            possiblePinyins = pinyin_syllable_q;
                            break;
                        case "c":
                            possiblePinyins = pinyin_syllable_c;
                            break;
                        case "d":
                            possiblePinyins = pinyin_syllable_d;
                            break;
                        case "t":
                            possiblePinyins = pinyin_syllable_t;
                            break;
                        case "f":
                            possiblePinyins = pinyin_syllable_f;
                            break;
                        case "g":
                            possiblePinyins = pinyin_syllable_g;
                            break;
                        case "l":
                            possiblePinyins = pinyin_syllable_l;
                            break;
                        case "x":
                            possiblePinyins = pinyin_syllable_x;
                            break;
                        case "o":
                            possiblePinyins = pinyin_syllable_o;
                            break;
                        case "m":
                            possiblePinyins = pinyin_syllable_m;
                            break;
                        case "y":
                            possiblePinyins = pinyin_syllable_y;
                            break;
                        case "b":
                            possiblePinyins = pinyin_syllable_b;
                            break;
                        case "k":
                            possiblePinyins = pinyin_syllable_k;
                            break;
                        case "w":
                            possiblePinyins = pinyin_syllable_w;
                            break;
                        case "n":
                            possiblePinyins = pinyin_syllable_n;
                            break;
                        case "a":
                            possiblePinyins = pinyin_syllable_a;
                            break;
                        case "r":
                            possiblePinyins = pinyin_syllable_r;
                            break;
                        case "e":
                            possiblePinyins = pinyin_syllable_e;
                            break;
                        default:
                            // 如果有未匹配的情况，可以在这里处理
                            possiblePinyins = null;
                            break;
                    }

                    if (possiblePinyins != null && i + 1 < n) // At least 2 characters needed
                    {
                        // Find maximum length of possible pinyins without using LINQ
                        int maxPossibleLen = 0;
                        foreach (string py in possiblePinyins)
                        {
                            if (py.Length > maxPossibleLen)
                            {
                                maxPossibleLen = py.Length;
                            }
                        }
                        
                        // Determine the maximum length we can check
                        int maxLengthToCheck = Math.Min(maxPossibleLen, n - i);
                        
                        // Check from longest to shortest possible matches
                        for (int l = maxLengthToCheck; l > 1; l--)
                        {
                            string substr = pinyin_str.Substring(i, l);
                            
                            // Perform binary search
                            int idx = Array.IndexOf(possiblePinyins, substr);
                            
                            // Verify the match
                            if (idx >= 0 && idx < possiblePinyins.Length && possiblePinyins[idx] == substr)
                            {
                                result[resultCount++] = pinyin_str[i].ToString();
                                i += l;
                                matched = true;
                                break;
                            }
                        }
                    }

                    if (!matched)
                    {
                        result[resultCount++] = pinyin_str[i].ToString();
                        i += 1;
                        matched = true;
                    }
                }
            }
            
            // 3. 都不匹配，直接取当前字符
            if (!matched)
            {
                result[resultCount++] = pinyin_str[i].ToString();
                i += 1;
            }
        }
        // 返回有效部分（去除未使用的空间）
        string[] finalResult = new string[resultCount];
        Array.Copy(result, finalResult, resultCount);
        return finalResult;
    }

    //匹配器

    private string MapSingleUlpbToPinyin(string ulpb)
    {
        if (ulpb.Length == 0)
        {
            return "";
        }
        switch (ulpb)
        {
            case "aa": return "a";
            case "oo": return "o";
        }
        var result = new StringBuilder();
        switch (ulpb[0])
        {
            case 'u': result.Append("sh"); break;
            case 'i': result.Append("ch"); break;
            case 'v': result.Append("zh"); break;
            default: result.Append(ulpb[0]); break;
        }
        if (ulpb.Length == 1)
        {
            return result.ToString();
        }

        var pending1 = "";
        var pending2 = "";
        switch (ulpb[1])
        {
            case 'q': result.Append("iu"); break;
            case 'w': result.Append("ei"); break;
            case 'e': result.Append("e"); break;
            case 'r': result.Append("uan"); break;
            case 't': result.Append("ue"); break;
            case 'y': result.Append("un"); break;
            case 'u': result.Append("u"); break;
            case 'i': result.Append("i"); break;
            case 'p': result.Append("ie"); break;
            case 'a': result.Append("a"); break;
            case 'd': result.Append("ai"); break;
            case 'f': result.Append("en"); break;
            case 'g': result.Append("eng"); break;
            case 'h': result.Append("ang"); break;
            case 'j': result.Append("an"); break;
            case 'z': result.Append("ou"); break;
            case 'c': result.Append("ao"); break;
            case 'b': result.Append("in"); break;
            case 'n': result.Append("iao"); break;
            case 'm': result.Append("ian"); break;

            case 'o': 
                pending1 = "o";
                pending2 = "uo";
                break;
            case 's': 
                pending1 = "ong";
                pending2 = "iong";
                break;
            case 'k': 
                pending1 = "uai";
                pending2 = "ing";
                break;
            case 'l': 
                pending1 = "uang";
                pending2 = "iang";
                break;
            case 'x': 
                pending1 = "ua";
                pending2 = "ia";
                break;
            case 'v': 
                pending1 = "ui";
                pending2 = "v";
                break;
        }
        
        if (pending1 == "") return result.ToString();
        if (Array.IndexOf(pinyin_dict, result.ToString() + pending1) != -1)
        {
            result.Append(pending1);
        }
        else
        {
            result.Append(pending2);
        }
        return result.ToString();
    }

    private string MapUlpbToPinyin(string ulpb)
    {
        var ori = ulpb.Split(' ');
        var result = new string[ori.Length];
        for (int i = 0; i < ori.Length; i++)
        {
            result[i] = MapSingleUlpbToPinyin(ori[i]);
        }
        return string.Join(" ", result);
    }

    private PinyinDict cachedDictionary;
    private string cachedInput;
    private int cachedLimit, cachedVersion;
    private bool cachedChinese, cachedAccurate;
    private string[] cachedResult;

    // Reused query workspace. Only the returned result needs a small allocation.
    private int[] topRows = new int[0];
    private int[] topEntries = new int[0];
    private double[] topScores = new double[0];
    private int topCount, queryLimit;
    private bool queryChinese, indexed;
    private string[] codes, initials, entries;
    private int[] order, initialOrder, entryMap, weights, wordIds;

    public void InvalidateMatchCache() { cachedResult = null; }

    public string[] Match(string inputPinyin, int limit = 20, bool accurateMode = false, bool ulpb = false)
    {
        if (dict_pool == null) SwitchSimp();
        if (string.IsNullOrWhiteSpace(inputPinyin) || limit <= 0) return new string[0];
        string reading = inputPinyin.Trim().ToLowerInvariant();
        if (ulpb) reading = MapUlpbToPinyin(reading);
        return Lookup(dict_pool, reading, limit, true, accurateMode);
    }

    private string[] Lookup(PinyinDict dictionary, string input, int limit, bool chinese, bool accurate)
    {
        if (dictionary == null || limit <= 0 || string.IsNullOrWhiteSpace(input)) return new string[0];
        string reading = input.Trim().ToLowerInvariant();
        bool allowCache = true;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        // Edit-mode validation reads the asset directly. Caching is disabled there because the cached
        // version tracks the asset, which the dictionary lookupVersion comparison below cannot see.
        PinyinDictionaryData editorSource = Application.isPlaying ? null : PinyinLookupBuilder.ResolveData(dictionary);
        allowCache = editorSource == null;
#endif
        if (allowCache && cachedResult != null && cachedDictionary == dictionary && cachedInput == reading
            && cachedLimit == limit && cachedChinese == chinese && cachedAccurate == accurate
            && cachedVersion == dictionary.lookupVersion)
        {
            string[] copy = new string[cachedResult.Length];
            Array.Copy(cachedResult, copy, copy.Length);
            return copy;
        }
        entries = dictionary.entries;
        weights = dictionary.weights;
        codes = dictionary.lookupCodes;
        order = dictionary.codeOrder;
        initials = dictionary.lookupInitials;
        initialOrder = dictionary.initialsOrder;
        entryMap = dictionary.indices;
        wordIds = dictionary.wordIds;
        string[] sourcePinyins = dictionary.pinyins;
        int sourceVersion = dictionary.lookupVersion;
#if UNITY_EDITOR && !COMPILER_UDONSHARP
        if (editorSource != null)
        {
            entries = editorSource.entries;
            weights = editorSource.weights;
            codes = editorSource.lookupCodes;
            order = editorSource.codeOrder;
            initials = editorSource.lookupInitials;
            initialOrder = editorSource.initialsOrder;
            entryMap = editorSource.indices;
            wordIds = editorSource.wordIds;
            sourcePinyins = editorSource.pinyins;
            sourceVersion = editorSource.lookupVersion;
        }
#endif
        if (entries == null || weights == null || sourcePinyins == null
            || entries.Length != weights.Length || entries.Length != sourcePinyins.Length
            || (chinese && (entryMap == null || entryMap.Length != entries.Length))) return new string[0];
        indexed = sourceVersion > 0 && codes != null && order != null
            && codes.Length == entries.Length && order.Length == entries.Length
            && initials != null && initialOrder != null
            && initials.Length == entries.Length && initialOrder.Length == entries.Length;
        if (!indexed) codes = sourcePinyins;
        queryChinese = chinese;
        queryLimit = Math.Min(limit, entries.Length);
        topCount = 0;
        if (topRows.Length < queryLimit)
        {
            topRows = new int[queryLimit];
            topEntries = new int[queryLimit];
            topScores = new double[queryLimit];
        }
        if (indexed)
        {
            int start = Bound(reading, false, false, false);
            int end = Bound(reading, false, true, false);
            for (int i = start; i < end; i++) Offer(order[i], 100);
            // Chinese historically skips fuzzy lookup once enough exact rows exist.
            if ((!chinese || !accurate) && topCount < queryLimit)
            {
                int prefixEnd = Bound(reading, false, true, true);
                for (int i = end; i < prefixEnd; i++) Offer(order[i], 50);
                if (chinese)
                {
                    // Reverse prefix: dictionary code is a proper prefix of the input.
                    for (int length = 1; length < reading.Length; length++)
                    {
                        string prefix = reading.Substring(0, length);
                        int first = Bound(prefix, false, false, false);
                        int last = Bound(prefix, false, true, false);
                        for (int i = first; i < last; i++) Offer(order[i], 50);
                    }
                    string key = GetInitials(reading);
                    int firstInitial = Bound(key, true, false, false);
                    int lastInitial = Bound(key, true, true, false);
                    for (int i = firstInitial; i < lastInitial; i++)
                    {
                        int row = initialOrder[i];
                        string code = codes[row];
                        // The tiers are disjoint, so no dictionary-sized seen array is needed.
                        if (code.Length > 0 && code[0] == reading[0]
                            && !code.StartsWith(reading, StringComparison.Ordinal)
                            && !reading.StartsWith(code, StringComparison.Ordinal)) Offer(row, 30);
                    }
                }
            }
        }
        else
        {
            // Compatibility for existing scene overrides that have not been baked yet.
            // Imports and the scene build hook supply the indexed path for players.
            string key = chinese ? GetInitials(reading) : "";
            for (int row = 0; row < codes.Length; row++)
                if (ReadCode(row) == reading) Offer(row, 100);
            if ((!chinese || !accurate) && topCount < queryLimit)
                for (int row = 0; row < codes.Length; row++)
                {
                    string code = ReadCode(row);
                    if (code.Length == 0 || code == reading) continue;
                    if (code.StartsWith(reading, StringComparison.Ordinal)
                        || (chinese && reading.StartsWith(code, StringComparison.Ordinal))) Offer(row, 50);
                    else if (chinese && code[0] == reading[0] && GetInitials(code) == key) Offer(row, 30);
                }
        }
        cachedDictionary = dictionary;
        cachedInput = reading;
        cachedLimit = limit;
        cachedChinese = chinese;
        cachedAccurate = accurate;
        cachedVersion = sourceVersion;
        cachedResult = new string[topCount];
        for (int i = 0; i < topCount; i++) cachedResult[i] = entries[topEntries[i]];
        string[] result = new string[topCount];
        Array.Copy(cachedResult, result, topCount);
        return result;
    }

    private string ReadCode(int row)
    {
        string code = codes[row] ?? "";
        return queryChinese ? code : code.Trim().ToLowerInvariant();
    }

    // Upper-prefix bound uses StartsWith instead of an alphabet-specific sentinel.
    private int Bound(string key, bool useInitials, bool upper, bool prefix)
    {
        int[] rows = useInitials ? initialOrder : order;
        string[] keys = useInitials ? initials : codes;
        int low = 0, high = rows.Length;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            string value = keys[rows[middle]];
            int comparison = string.CompareOrdinal(value, key);
            if (comparison < 0 || (upper && (comparison == 0
                || (prefix && value.StartsWith(key, StringComparison.Ordinal))))) low = middle + 1;
            else high = middle;
        }
        return low;
    }

    private void Offer(int row, int tier)
    {
        int entry = queryChinese ? entryMap[row] : row;
        if (entry < 0 || entry >= entries.Length || queryLimit == 0) return;
        if (!queryChinese && string.IsNullOrEmpty(entries[entry])) return;
        if (queryChinese && tier != 100)
            for (int i = 0; i < topCount; i++)
                if (topEntries[i] == entry && topScores[i] >= 100000000d) return;
        double score = (queryChinese ? tier * 1000000d : tier == 100 ? 1000000d : 0d)
            + Math.Log10(Math.Max(0, weights[entry]) + 1d);
        // JA/KO deduplicate output words; Chinese preserves existing row semantics.
        if (!queryChinese)
            for (int i = 0; i < topCount; i++)
            {
                bool duplicate = indexed && wordIds != null && wordIds.Length == entries.Length
                    ? wordIds[topEntries[i]] == wordIds[entry] : entries[topEntries[i]] == entries[entry];
                if (!duplicate) continue;
                if (topScores[i] > score || (topScores[i] == score && topRows[i] <= row)) return;
                for (int j = i; j < topCount - 1; j++)
                {
                    topRows[j] = topRows[j + 1]; topEntries[j] = topEntries[j + 1]; topScores[j] = topScores[j + 1];
                }
                topCount--;
                break;
            }
        int position = topCount;
        while (position > 0 && (score > topScores[position - 1]
            || (score == topScores[position - 1] && row < topRows[position - 1]))) position--;
        if (position >= queryLimit) return;
        for (int i = Math.Min(topCount, queryLimit - 1); i > position; i--)
        {
            topRows[i] = topRows[i - 1]; topEntries[i] = topEntries[i - 1]; topScores[i] = topScores[i - 1];
        }
        topRows[position] = row; topEntries[position] = entry; topScores[position] = score;
        topCount = Math.Min(topCount + 1, queryLimit);
    }

    private string GetInitials(string pinyin)
    {
        StringBuilder result = new StringBuilder();
        for (int i = 0; i < pinyin.Length; i++)
            if (pinyin[i] != ' ' && (i == 0 || pinyin[i - 1] == ' ')) result.Append(pinyin[i]);
        return result.ToString();
    }

}
}
