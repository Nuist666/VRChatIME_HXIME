
using System;
using System.Linq;
using TMPro;
using UdonSharp;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
namespace HX2xianglong90.HXIME{
public class HXIMEUI : UdonSharpBehaviour
{
    [Header("是否启用日文?")]
    [SerializeField] private bool enableJapanese = true;
    [Header("是否启用韩文?")]
    [SerializeField] private bool enableKorean = true;
    [Header("是否默认中文输入?")]
    [SerializeField] private bool defaultZhCN = true;
    [Header("是否默认简体字?")]
    [SerializeField] private bool defaultSimplified = true;
    [Header("是否默认双拼输入?")]
    [SerializeField] private bool defaultUlpb = false;
    [Header("是否默认显示握把？")]
    [SerializeField] private bool defaultHandleOn = true;
    [Header("是否默认选词和键盘拆开？")]
    [SerializeField] private bool splitInputbarKeyboard = true;
    
    [Header("目标输入框")]
    public TMP_InputField targetInputfield;
    private bool targetWarningShown;
    private int langMode = 1; // 0: English, 1: Chinese, 2+: additionalDicts
    private bool isSimplified = true; //是简体字
    private bool ulpb = true;
    private bool capsed = false;
    private bool shifted = false;
    private int candidateLimits = 30;
    private bool accurateMode = false;
    // pinyin initials
    private const string pinyinInitials = "bpmfdtnlgkhjqxzcsry";
    // word candidates
    private string[] wordCandidates = new string[0];
    private int pageResultNum = 5;
    private int currentPageIndex = 0;
    private int maxPageIndex =0;
    [Header("皮肤预设导入")]
    public string[] skinData;
    public Texture2D[] skinPics;
    private string[] skinTitle;
    private string[] skinDescription;
    private Color[] buttonColor;//按钮色
    private Color[] mainColor1;//主色1
    private Color[] mainColor2;//主色2
    [Header("下面为各个连接的组件，非必要不要乱动")]
    // objects
    [SerializeField] private GameObject inputBarHandle;
    [SerializeField] private GameObject switchBarHandle;
    [SerializeField] private GameObject keyboardHandle;
    [SerializeField] private PinyinEngine engine;
    private Renderer[] handleRenderers = new Renderer[3];
    private Renderer[] bgRenderers = new Renderer[6];
    // switch bar
    private TMP_Text switchBarLangSwitchButton;
    private TMP_Text switchBarSimpTradButton;
    private TMP_Text switchBarUlpbButton;
    private GameObject settingsPanel;
    private Toggle accurateModeToggle;
    private Color settingsTextColor = Color.white;
    private Image toggleBackgroundImage;
    private Image toggleCheckmarkImage;
    private Image keyboardToggleImage;
    private Image handleToggleImage;
    private TMP_Text statementText;
    // setting panel
    private Image settingsImage;
    private Image[] settingPanelButtonsImage;
    private Image[] settingPanelButtonsIcon;
    private TMP_Text[] settingPanelButtonsText;
    private TMP_Text[] closeButtonTexts = new TMP_Text[2];
    private TMP_Text creditText;
    // skin center
    private TMP_Text skinCenterLabel;
    private TMP_Text[] skinSelections;
    // input bar
    private Vector3 inputBarInitPos;
    private Quaternion inputBarInitRotation;
    private ParentConstraint inputBarConstraint;
    private TMP_InputField IMEInputField;
    private TMP_Text[] IMEInputFieldTexts = new TMP_Text[2];
    private Button[] wordCandidateButtons;
    private TMP_Text[] wordCandidateButtonTexts;
    private Image[] candidatePageButtons;
    // keyboard
    private Vector3 keyboardInitPos;
    private Quaternion keyboardInitRotation;
    private Button[] alphabetButtons;
    private TMP_Text[] alphabetButtonTexts;
    private Button[] markButtons;
    private TMP_Text[] markButtonTexts;
    private Button[] specialButtons;
    private TMP_Text[] specialButtonTexts;
    private string markButtonEn = @"`1234567890-=[];'\,./";
    private string markButtonEnShift = "~!@#$%^&*()_+{}:\"|<>?";
    private string markButtonZh = @"·1234567890-=【】；‘、，。/";
    private string markButtonZhShift = "~！@#￥%…&*（）—+{}：“|《》？";
    void Start()
    {
        //set lang mode
        if(defaultZhCN){langMode = 1;}else{langMode=0;};
        // 中文与英文始终启用；日文、韩文按预制件最上面的开关交给引擎。
        if (engine != null)
        {
            engine.SetLanguageEnabled(enableJapanese, enableKorean);
        }
        //set simp or trad
        if(defaultSimplified){isSimplified=true;}else{isSimplified=false;};
        ulpb = defaultUlpb;
        //init inputbar
        IMEInputField = (TMP_InputField)inputBarHandle.transform.Find("InputBar/InputField").GetComponent(typeof(TMP_InputField));
        wordCandidateButtons = (Button[])inputBarHandle.transform.Find("InputBar/Candidates").GetComponentsInChildren(typeof(Button));
        wordCandidateButtonTexts = new TMP_Text[wordCandidateButtons.Length];
        for(int i=0; i<wordCandidateButtons.Length;i++){
            wordCandidateButtonTexts[i]=(TMP_Text)wordCandidateButtons[i].transform.GetChild(0).GetComponent(typeof(TMP_Text));
        }
        inputBarConstraint = (ParentConstraint)inputBarHandle.GetComponent(typeof(ParentConstraint));
        IMEInputFieldTexts[0] = (TMP_Text)inputBarHandle.transform.Find("InputBar/InputField/TextArea/Placeholder").GetComponent(typeof(TMP_Text));
        IMEInputFieldTexts[1] = (TMP_Text)inputBarHandle.transform.Find("InputBar/InputField/TextArea/Text").GetComponent(typeof(TMP_Text));
        candidatePageButtons = (Image[])inputBarHandle.transform.Find("InputBar/PageControl").GetComponentsInChildren(typeof(Image));
        //pos and rotation
        inputBarInitPos = inputBarHandle.transform.position;
        inputBarInitRotation = inputBarHandle.transform.rotation;
        //init switchbar buttons
        switchBarLangSwitchButton = (TMP_Text)switchBarHandle.transform.Find("SwitchBar/LangState").GetComponent(typeof(TMP_Text));
        switchBarSimpTradButton = (TMP_Text)switchBarHandle.transform.Find("SwitchBar/SimpTradSwitch").GetComponent(typeof(TMP_Text));
        // Older prefabs have no full/double-pinyin button. Keep their layout usable.
        Transform ulpbSwitch = switchBarHandle.transform.Find("SwitchBar/UlpbSwitch");
        if (ulpbSwitch != null)
            switchBarUlpbButton = (TMP_Text)ulpbSwitch.GetComponent(typeof(TMP_Text));
        settingsPanel = switchBarHandle.transform.Find("SettingsPanel").gameObject;
        statementText = (TMP_Text)switchBarHandle.transform.Find("SettingsPanel/Statement").GetComponent(typeof(TMP_Text));
        accurateModeToggle = (Toggle)switchBarHandle.transform.Find("SettingsPanel/Buttons/AccurateMode").GetComponent(typeof(Toggle));
        settingPanelButtonsImage = (Image[])switchBarHandle.transform.Find("SettingsPanel/Buttons").GetComponentsInChildren(typeof(Image));
        settingPanelButtonsIcon = new Image[switchBarHandle.transform.Find("SettingsPanel/Buttons").childCount];
        settingPanelButtonsText = new TMP_Text[switchBarHandle.transform.Find("SettingsPanel/Buttons").childCount];
        for(int i=0;i<switchBarHandle.transform.Find("SettingsPanel/Buttons").childCount;i++){
            settingPanelButtonsIcon[i] = (Image)switchBarHandle.transform.Find("SettingsPanel/Buttons").GetChild(i
            ).Find("Icon").gameObject.GetComponent(typeof(Image));
            settingPanelButtonsText[i] = (TMP_Text)switchBarHandle.transform.Find("SettingsPanel/Buttons").GetChild(i
            ).Find("Text").gameObject.GetComponent(typeof(TMP_Text));
        }
        keyboardToggleImage = (Image)switchBarHandle.transform.Find("SwitchBar/KeyboardToggle").GetComponent(typeof(Image));
        handleToggleImage = (Image)switchBarHandle.transform.Find("SwitchBar/HandleToggle").GetComponent(typeof(Image));
        settingsImage = (Image)switchBarHandle.transform.Find("SwitchBar/Settings").GetComponent(typeof(Image));

        toggleBackgroundImage = (Image)switchBarHandle.transform.Find("SettingsPanel/Buttons/AccurateMode/Background").GetComponent(typeof(Image));
        toggleCheckmarkImage = (Image)switchBarHandle.transform.Find("SettingsPanel/Buttons/AccurateMode/Background/Checkmark").GetComponent(typeof(Image));
        creditText = (TMP_Text)switchBarHandle.transform.Find("CreditInfo/Credit").GetComponent(typeof(TMP_Text));
        closeButtonTexts[0] = (TMP_Text)switchBarHandle.transform.Find("SkinCenter/Close/Text").GetComponent(typeof(TMP_Text));
        closeButtonTexts[1] = (TMP_Text)switchBarHandle.transform.Find("CreditInfo/Close/Text").GetComponent(typeof(TMP_Text));
        //skin center
        skinCenterLabel = (TMP_Text)switchBarHandle.transform.Find("SkinCenter/Title").GetComponent(typeof(TMP_Text));
        skinSelections = (TMP_Text[])switchBarHandle.transform.Find("SkinCenter/Content").GetComponentsInChildren(typeof(TMP_Text));
        //init bg renderers
        bgRenderers[0] = (Renderer)switchBarHandle.transform.Find("SwitchBar/Background").GetComponent(typeof(Renderer));
        bgRenderers[1] = (Renderer)switchBarHandle.transform.Find("SettingsPanel/Background").GetComponent(typeof(Renderer));
        bgRenderers[2] = (Renderer)switchBarHandle.transform.Find("CreditInfo/Background").GetComponent(typeof(Renderer));
        bgRenderers[3] = (Renderer)switchBarHandle.transform.Find("SkinCenter/Background").GetComponent(typeof(Renderer));
        bgRenderers[4] = (Renderer)inputBarHandle.transform.Find("InputBar/Background").GetComponent(typeof(Renderer));
        bgRenderers[5] = (Renderer)keyboardHandle.transform.Find("Keyboard/Background").GetComponent(typeof(Renderer));
        
        //init keyboard buttons
        alphabetButtons = (Button[])keyboardHandle.transform.Find("Keyboard/AlphabetButtons").GetComponentsInChildren(typeof(Button));
        markButtons = (Button[])keyboardHandle.transform.Find("Keyboard/MarkButtons").GetComponentsInChildren(typeof(Button));
        specialButtons = (Button[])keyboardHandle.transform.Find("Keyboard/SpecialButtons").GetComponentsInChildren(typeof(Button));
        alphabetButtonTexts = new TMP_Text[alphabetButtons.Length];
        markButtonTexts = new TMP_Text[markButtons.Length];
        specialButtonTexts = new TMP_Text[specialButtons.Length];
        for(int i=0; i<alphabetButtons.Length;i++){
            alphabetButtonTexts[i]=(TMP_Text)alphabetButtons[i].transform.GetChild(0).GetComponent(typeof(TMP_Text));
        }
        for(int i=0; i<markButtons.Length;i++){
            markButtonTexts[i]=(TMP_Text)markButtons[i].transform.GetChild(0).GetComponent(typeof(TMP_Text));
        }
        for(int i=0; i<specialButtons.Length;i++){
            specialButtonTexts[i]=(TMP_Text)specialButtons[i].transform.GetChild(0).GetComponent(typeof(TMP_Text));
        }
        //init handle renderers
        handleRenderers[0] = (Renderer)keyboardHandle.GetComponent(typeof(Renderer));
        handleRenderers[1] = (Renderer)inputBarHandle.GetComponent(typeof(Renderer));
        handleRenderers[2] = (Renderer)switchBarHandle.GetComponent(typeof(Renderer));
        //pos and rotation
        keyboardInitPos=keyboardHandle.transform.position;
        keyboardInitRotation=keyboardHandle.transform.rotation;
        //close all word candidate buttons
        foreach(Button go in wordCandidateButtons){
            go.gameObject.SetActive(false);
        }
        RefreshLangSwitch();
        RefreshSimpTrad();
        RefreshUlpb();
        //set simplified trad
        if(isSimplified){
        	engine.SwitchSimp();
        }else{
            engine.SwitchTrad();
        }
        //set handle
        if(defaultHandleOn){
            foreach(Renderer h in handleRenderers){
                h.enabled = true;
            }
        }else{
            foreach(Renderer h in handleRenderers){
                h.enabled = false;
            }
        }
        //Set accurate mode
        accurateModeToggle.isOn = accurateMode;
        //Set split
        inputBarConstraint.enabled = !splitInputbarKeyboard;
        // Confirm statement
        statementText.text = "HXIME v 0.9.5\n©HX2 xianglong90";
        // Load Skin Data
        skinTitle = new string[skinData.Length];
        skinDescription = new string[skinData.Length];
        buttonColor = new Color[skinData.Length];//按钮色
        mainColor1 = new Color[skinData.Length];//主色1
        mainColor2 = new Color[skinData.Length];//主色2
        for(int i = 0; i<skinData.Length;i++){
            ParseSkinData(skinData[i],i);
        }
        //set skin selections
        foreach(TMP_Text t in skinSelections){t.gameObject.SetActive(false);}
        for(int i = 0;i<skinData.Length;i++){
            if(i==9){break;};
            skinSelections[i].gameObject.SetActive(true);
            skinSelections[i].text = skinTitle[i]+"\n<size=25>"+skinDescription[i];
        }
        //load default skin
        SetSkin0();
    }
    // 解析皮肤数据
    private void ParseSkinData(string input,int index)
    {
        // 用分号分隔字符串
        string[] values = input.Split(';');

        // 将数据分配到对应的变量中
        skinTitle[index] = values[0];
        skinDescription[index] = values[1];

        // 转换颜色数据并存入数组
        buttonColor[index] = ParseColor(values[2]);
        mainColor1[index] = ParseColor(values[3]);
        mainColor2[index] = ParseColor(values[4]);
    }
    // 解析颜色
    private Color ParseColor(string colorString)
    {
        // 移除括号并分割成组件
        string cleanedSet = colorString.Trim('(', ')');
        string[] colorComponents = cleanedSet.Split(',');
        // 转换为 float 并创建 Color 对象
        float r = float.Parse(colorComponents[0]) / 255f;
        float g = float.Parse(colorComponents[1]) / 255f;
        float b = float.Parse(colorComponents[2]) / 255f;
        float a = float.Parse(colorComponents[3]) / 255f;
        return new Color(r, g, b, a);
    }
    // 各个按键的处理中介
    private bool HasTargetInputField()
    {
        if (targetInputfield != null && targetInputfield != IMEInputField) return true;
        if (!targetWarningShown)
        {
            Debug.LogWarning("HXIME: Target Inputfield 必须绑定独立的目标 TMP_InputField，不能绑定输入法自身的拼音预编辑框；当前输入尚未提交。");
            targetWarningShown = true;
        }
        return false;
    }
    private void AlphabetButtonPressedAgent(int buttonIndex){
        string inputText = alphabetButtonTexts[buttonIndex].text;
        if(langMode==0){ //英文
            if (!HasTargetInputField()) return;
            targetInputfield.text = targetInputfield.text + inputText;
            return;
        }
        else if (langMode == 1 && ulpb)
        {
            if (IMEInputField.text.Length >1 && IMEInputField.text[IMEInputField.text.Length-2] != ' ' && IMEInputField.text[IMEInputField.text.Length-1] != ' ')
            {
                inputText = " " + inputText;
            }
        }
        else if(langMode ==1 && accurateMode==false){//中文
            // 如果是后鼻音就直接加空格
            if(IMEInputField.text.EndsWith("ng")){
                inputText = " " + inputText;
            }else{
            // 检查输入是否是声母，如果是，则在前面加一个空格
            if (pinyinInitials.Contains(inputText)&&IMEInputField.text!=""){
                //输入字是h的情况下上一个字不是zcs, 即支持输入zh ch sh
                if(!(inputText=="h"&&"zcs".Contains(IMEInputField.text[IMEInputField.text.Length-1]))){
                    //前鼻音
                    if(!(inputText=="n"&&"aoeiu".Contains(IMEInputField.text[IMEInputField.text.Length-1]))){
                        //后鼻音
                        if(!(IMEInputField.text.EndsWith("an")||IMEInputField.text.EndsWith("in")||IMEInputField.text.EndsWith("on"))){
                            inputText = " " + inputText;
                        }
                    }
                }
                }
            }
            }
        IMEInputField.text = IMEInputField.text+inputText;
    }
    private void MarkButtonPressedAgent(int buttonIndex){
        if(langMode!=0){ // Composition
            if(IMEInputField.text==""){
                if (!HasTargetInputField()) return;
                targetInputfield.text = targetInputfield.text+markButtonTexts[buttonIndex].text;
            }
            else{
                if(buttonIndex ==1||buttonIndex==2||buttonIndex==3||buttonIndex==4||buttonIndex==5){
                    if(wordCandidateButtons[buttonIndex-1].gameObject.activeSelf){
                        WordCandidatePressedAgent(buttonIndex-1);
                    }else{
                        IMEInputField.text = IMEInputField.text+markButtonTexts[buttonIndex].text;
                    }
                }else{
                    IMEInputField.text = IMEInputField.text+markButtonTexts[buttonIndex].text;
                }
            }
        }else if(langMode==0){ //英文
            if (!HasTargetInputField()) return;
            targetInputfield.text = targetInputfield.text+markButtonTexts[buttonIndex].text;
        }
    }
    private void WordCandidatePressedAgent(int buttonIndex){
        int candidateIndex = currentPageIndex * pageResultNum + buttonIndex;
        if (buttonIndex < 0 || buttonIndex >= pageResultNum || candidateIndex >= wordCandidates.Length) return;
        if (!HasTargetInputField()) return;
        // Both fields can invoke onValueChanged synchronously. Capture the selected
        // word and remaining composition before either callback refreshes the UI.
        string selectedWord = wordCandidates[candidateIndex];
        string remaining = "";
        if (langMode == 1)
        {
            string[] syllables = IMEInputField.text.Split(' ');
            int consumed = 0;
            for (int i = 0; i < syllables.Length; i++)
            {
                if (syllables[i].Length == 0) continue;
                if (consumed >= selectedWord.Length)
                    remaining += (remaining.Length == 0 ? "" : " ") + syllables[i];
                consumed++;
            }
        }
        IMEInputField.text = remaining;
        targetInputfield.text += selectedWord;
        GetWordCandidates();
    }
    // 功能键
    public void ShiftPressed(){
        if(shifted){
            shifted = false;
        }else{
            shifted = true;
        }
        if((shifted&&capsed)||(!shifted&&!capsed)){
            foreach (TMP_Text abtext in alphabetButtonTexts){
                abtext.text = abtext.text.ToLower();
            }
        }else{
            foreach (TMP_Text abtext in alphabetButtonTexts){
                abtext.text = abtext.text.ToUpper();
            }
        }
        // mark buttons
        if (shifted){
            if(langMode != 1){ // Latin punctuation
                for(int i = 0;i<markButtonTexts.Length;i++){
                    markButtonTexts[i].text = markButtonEnShift[i].ToString();
                }
            }else if(langMode == 1){ //中文
                for(int i = 0;i<markButtonTexts.Length;i++){
                    markButtonTexts[i].text = markButtonZhShift[i].ToString();
                }
            }
        }else{ // not shifted
            if(langMode != 1){ // Latin punctuation
                for(int i = 0;i<markButtonTexts.Length;i++){
                    markButtonTexts[i].text = markButtonEn[i].ToString();
                }
            }else if(langMode == 1){ //中文
                for(int i = 0;i<markButtonTexts.Length;i++){
                    markButtonTexts[i].text = markButtonZh[i].ToString();
                }
            }
        }
    }
    public void CapsPressed(){
        if(capsed){
            capsed = false;
        }else{
            capsed = true;
        }
        if((shifted&&capsed)||(!shifted&&!capsed)){
            foreach (TMP_Text abtext in alphabetButtonTexts){
                abtext.text = abtext.text.ToLower();
            }
        }else{
            foreach (TMP_Text abtext in alphabetButtonTexts){
                abtext.text = abtext.text.ToUpper();
            }
        }
    }
    public void BackSpacePressed(){
        if(langMode==0){ //英文
            if (!HasTargetInputField()) return;
            if(targetInputfield.text.Length!=0){
                targetInputfield.text = targetInputfield.text.Remove(targetInputfield.text.Length-1);
            }
        }else if(langMode!=0){ // Composition
            if(IMEInputField.text.Length!=0){
                IMEInputField.text = IMEInputField.text.Remove(IMEInputField.text.Length-1);
                if (langMode == 1 && IMEInputField.text.Length > 0 && IMEInputField.text[IMEInputField.text.Length - 1] == ' ')
                {
                    IMEInputField.text = IMEInputField.text.Remove(IMEInputField.text.Length - 1);
                }
            }else{
                if (!HasTargetInputField()) return;
                if(targetInputfield.text.Length!=0){
                    targetInputfield.text = targetInputfield.text.Remove(targetInputfield.text.Length-1);
                }
            }
        }
    }
    public void TabPressed(){
        if(langMode ==0){
            if (!HasTargetInputField()) return;
            targetInputfield.text = targetInputfield.text + "\t";
        }else if(langMode!=0)
        {
            IMEInputField.text = "";
        }
    }
    public void SpacePressed(){
        if(langMode==0 || IMEInputField.text==""){ //英文，或者没有预编辑文本
            if (!HasTargetInputField()) return;
            targetInputfield.text += " ";
            return;
        }
        if (wordCandidates.Length == 0) EnterPressed();
        else WordCandidatePressedAgent(0);
    }
    //获取候选
    public void GetWordCandidates(){
        if (IMEInputField == null || wordCandidateButtons == null) return;
        if(langMode != 0){
            wordCandidates = langMode == 1
                ? engine.Match(IMEInputField.text,candidateLimits,accurateMode, ulpb)
                : engine.MatchLanguage(langMode, IMEInputField.text, candidateLimits);
            currentPageIndex = 0;
            maxPageIndex = wordCandidates.Length/pageResultNum;
            if(wordCandidates.Length%pageResultNum==0){
                maxPageIndex = wordCandidates.Length/pageResultNum-1;
            }
        }
        else { wordCandidates = new string[0]; currentPageIndex = 0; maxPageIndex = 0; }
        maxPageIndex = Math.Max(0, maxPageIndex);
        RefreshWordCandidates();
    }
    private void RefreshWordCandidates(){
        foreach(Button go in wordCandidateButtons){
            go.gameObject.SetActive(false);
        }
        if(currentPageIndex==maxPageIndex && wordCandidates.Length%pageResultNum!=0){
            for (int i=0;i<wordCandidates.Length%pageResultNum;i++){
                wordCandidateButtons[i].gameObject.SetActive(true);
                wordCandidateButtonTexts[i].text=wordCandidates[i+currentPageIndex*pageResultNum];
            }
        }
        else if(wordCandidates.Length!=0){
            for (int i=0;i<pageResultNum;i++){
                    wordCandidateButtons[i].gameObject.SetActive(true);
                    wordCandidateButtonTexts[i].text=wordCandidates[i+currentPageIndex*pageResultNum];
            }
        }
        else{
            foreach(TMP_Text t in wordCandidateButtonTexts){
                t.text = "";
            }
        }
    }
    //其他功能键
    public void EnterPressed(){
        if (!HasTargetInputField()) return;
        Debug.Log("回车！");
        if (langMode==0 || IMEInputField.text==""){
            targetInputfield.text=targetInputfield.text+"\n";
            return;
        }
        // 有预编辑文本，预编辑文本直接上屏
        targetInputfield.text += langMode == 1 ? IMEInputField.text.Replace(" ", "") : IMEInputField.text;
        IMEInputField.text = "";
    }
    public void PrevPressed(){
        Debug.Log("上一页");
        if (currentPageIndex-1>=0){
            currentPageIndex = currentPageIndex - 1;
            RefreshWordCandidates();
        }
    }
    public void NextPressed(){
        Debug.Log("下一页");
        if (currentPageIndex<maxPageIndex){
            currentPageIndex += 1;
            RefreshWordCandidates();
        }
    }
    //工具栏
    //切换中英文
    public void LangChangePressed(){
        // Commit the old composition before changing its interpretation.
        if (IMEInputField.text.Length > 0)
        {
            if (!HasTargetInputField()) return;
            targetInputfield.text += langMode == 1 ? IMEInputField.text.Replace(" ", "") : IMEInputField.text;
            IMEInputField.text = "";
        }
        langMode = engine.NextLanguage(langMode);
        RefreshLangSwitch();
    }
    private void RefreshLangSwitch(){
        
        if(langMode ==0){
            switchBarLangSwitchButton.text = "En";
            IMEInputField.text = "";
        }else{
            switchBarLangSwitchButton.text = engine.LanguageLabel(langMode);
            IMEInputField.text = "";
        }
        RefreshInputBarVisibility();
        RefreshKeyboardLabels();
        // Exact pinyin matching only applies to the Chinese engine.
        accurateModeToggle.interactable = langMode == 1;
        RefreshSettingsTextColors();
        switchBarSimpTradButton.gameObject.SetActive(langMode == 1);
        if (switchBarUlpbButton != null) switchBarUlpbButton.gameObject.SetActive(langMode == 1);
        GetWordCandidates();
        // mark buttons
        if (shifted){
            if(langMode != 1){ // Latin punctuation
                for(int i = 0;i<markButtonTexts.Length;i++){
                    markButtonTexts[i].text = markButtonEnShift[i].ToString();
                }
            }else if(langMode == 1){ //中文
                for(int i = 0;i<markButtonTexts.Length;i++){
                    markButtonTexts[i].text = markButtonZhShift[i].ToString();
                }
            }
        }else{ // not shifted
            if(langMode != 1){ // Latin punctuation
                for(int i = 0;i<markButtonTexts.Length;i++){
                    markButtonTexts[i].text = markButtonEn[i].ToString();
                }
            }else if(langMode == 1){ //中文
                for(int i = 0;i<markButtonTexts.Length;i++){
                    markButtonTexts[i].text = markButtonZh[i].ToString();
                }
            }
        }
    }
    // Identify the dictionary by its label, not its position in additionalDicts.
    private void RefreshKeyboardLabels(){
        string label = engine.LanguageLabel(langMode);
        bool ja = label == "Ja" || label == "JP" || label == "ja" || label == "日本語";
        bool ko = label == "Ko" || label == "KO" || label == "ko" || label == "한국어";
        bool zh = langMode == 1;
        if (ja) switchBarLangSwitchButton.text = "JP";
        for (int i = 0; i < specialButtons.Length; i++){
            string key = specialButtons[i].name;
            TMP_Text text = specialButtonTexts[i];
            if (key == "LangChange") text.text = ja ? "JP" : ko ? "Ko" : zh ? "中" : label;
            else if (key == "Tab") text.text = ja ? "クリア" : ko ? "지우기" : zh ? "重输" : "Tab";
            else if (key == "EnterButton") text.text = ja ? "Enter\n確定" : ko ? "Enter\n입력" : zh ? "Enter\n回车" : "Enter";
            else if (key == "PrevPage") text.text = ja ? "前へ" : ko ? "이전" : zh ? "前页" : "Prev";
            else if (key == "NextPage") text.text = ja ? "次へ" : ko ? "다음" : zh ? "后页" : "Next";
        }
        IMEInputFieldTexts[0].text = ja ? "このキーボードで入力" : ko ? "이 키보드로 입력하세요" : zh ? "请使用本输入法键盘输入" : "Type with this keyboard";
        skinCenterLabel.text = ja ? "スキン" : ko ? "스킨" : zh ? "皮肤中心" : "Skins";
        for (int i = 0; i < settingPanelButtonsText.Length; i++){
            TMP_Text text = settingPanelButtonsText[i];
            string key = settingsPanel.transform.Find("Buttons").GetChild(i).name;
            if (key == "AccurateMode") text.text = ja ? "完全一致" : ko ? "정확 일치" : zh ? "强制全拼" : "Exact";
            else if (key == "Skin") text.text = ja ? "スキン" : ko ? "스킨" : zh ? "皮肤" : "Skin";
            else if (key == "SplitMode") text.text = ja ? "分離" : ko ? "분리" : zh ? "拆合" : "Split";
            else if (key == "Credit") text.text = ja ? "情報" : ko ? "정보" : zh ? "信息" : "Info";
            else if (key == "KeyBoardPosReset") text.text = ja ? "リセット" : ko ? "초기화" : zh ? "复位" : "Reset";
        }
    }
    //切换简体繁体
	public void SwitchSimpTrad(){
        if (langMode != 1) return;
        isSimplified=!isSimplified;
        RefreshSimpTrad();
    }
	public void RefreshSimpTrad(){
        if(isSimplified){
            switchBarSimpTradButton.text="简";
        	engine.SwitchSimp();
            GetWordCandidates();
        }else{
            switchBarSimpTradButton.text="繁";
            engine.SwitchTrad();
            GetWordCandidates();
        }
    }
    //切换全拼双拼
	public void SwitchUlpb(){
        if (langMode != 1) return;
        ulpb = !ulpb;
        RefreshUlpb();
    }
	public void RefreshUlpb(){
        if (switchBarUlpbButton != null) switchBarUlpbButton.text = ulpb ? "双" : "全";
        GetWordCandidates();
    }
    //切换键盘
    public void ToggleKeyboard(){
        keyboardHandle.SetActive(!keyboardHandle.activeSelf);
        RefreshInputBarVisibility();
    }
    private void RefreshInputBarVisibility(){
        // Hide the entire composition handle in English, as in the original UI.
        inputBarHandle.SetActive(langMode != 0 && keyboardHandle.activeSelf);
    }
    //握把开关
    public void ToggleHandleRenderers(){
        foreach(Renderer r in handleRenderers){
            r.enabled = !r.enabled;
        }
    }
    //打开设置
    public void ToggleSettingsPanel(){
        settingsPanel.SetActive(!settingsPanel.activeSelf);
    }
    //重置输入栏和键盘位置
    public void ResetInputBarAndKeyboard(){
        inputBarHandle.transform.SetPositionAndRotation(inputBarInitPos,inputBarInitRotation);
        keyboardHandle.transform.SetPositionAndRotation(keyboardInitPos,keyboardInitRotation);
    }
    //强制全拼
    public void ToggleAccurateMode(){
        if (langMode != 1) return;
        accurateMode = accurateModeToggle.isOn;
    }
    private void RefreshSettingsTextColors(){
        for (int i = 0; i < settingPanelButtonsText.Length; i++){
            bool disabled = settingsPanel.transform.Find("Buttons").GetChild(i).name == "AccurateMode" && langMode != 1;
            settingPanelButtonsText[i].color = disabled
                ? new Color(0.5f, 0.5f, 0.5f, settingsTextColor.a)
                : settingsTextColor;
        }
    }
    //切换拆合：位置跟随设置适用于所有语言。
    public void ToggleSplit(){
        inputBarConstraint.enabled = !inputBarConstraint.enabled;
    }
    //设置皮肤 (已开放)
    public void SetSkin(int i){
        //设置背景
        foreach(Renderer r in bgRenderers){r.material.mainTexture = skinPics[i];}
        //设置按钮
        foreach(Button btn in alphabetButtons){btn.image.color = buttonColor[i];}
        foreach(Button btn in markButtons){btn.image.color = buttonColor[i];}
        foreach(Button btn in specialButtons){btn.image.color = buttonColor[i];}
        foreach(Button btn in wordCandidateButtons){btn.image.color = buttonColor[i];}
        foreach(Image img in settingPanelButtonsImage){img.color = buttonColor[i];}
        toggleBackgroundImage.color = buttonColor[i];
        //设置主色1
        foreach(TMP_Text t in alphabetButtonTexts){t.color = mainColor1[i];}
        foreach(TMP_Text t in markButtonTexts){t.color = mainColor1[i];}
        foreach(TMP_Text t in specialButtonTexts){t.color = mainColor1[i];}
        foreach(TMP_Text t in wordCandidateButtonTexts){t.color = mainColor1[i];}
        foreach(TMP_Text t in IMEInputFieldTexts){t.color = mainColor1[i];}
        switchBarLangSwitchButton.color = mainColor1[i];
        toggleCheckmarkImage.color = mainColor1[i];
        creditText.color = mainColor1[i];
        skinCenterLabel.color = mainColor1[i];
        statementText.color = mainColor1[i];
        foreach(TMP_Text t in skinSelections){t.color = mainColor1[i];}
        //设置主色2
        switchBarSimpTradButton.color = mainColor2[i];
        if (switchBarUlpbButton != null) switchBarUlpbButton.color = mainColor2[i];
        foreach(TMP_Text t in closeButtonTexts){t.color = mainColor2[i];}
        foreach(Image img in settingPanelButtonsIcon){img.color = mainColor2[i];}
        settingsTextColor = mainColor2[i];
        RefreshSettingsTextColors();
        keyboardToggleImage.color = mainColor2[i];
        handleToggleImage.color = mainColor2[i];
        settingsImage.color = mainColor2[i];
        foreach(TMP_Text t in closeButtonTexts){t.color = mainColor2[i];}
        foreach(Image img in candidatePageButtons){img.color = mainColor2[i];}
    }
    // alphabet buttons
    public void AlphabetButtonPressed0(){AlphabetButtonPressedAgent(0);}
    public void AlphabetButtonPressed1(){AlphabetButtonPressedAgent(1);}
    public void AlphabetButtonPressed2(){AlphabetButtonPressedAgent(2);}
    public void AlphabetButtonPressed3(){AlphabetButtonPressedAgent(3);}
    public void AlphabetButtonPressed4(){AlphabetButtonPressedAgent(4);}
    public void AlphabetButtonPressed5(){AlphabetButtonPressedAgent(5);}
    public void AlphabetButtonPressed6(){AlphabetButtonPressedAgent(6);}
    public void AlphabetButtonPressed7(){AlphabetButtonPressedAgent(7);}
    public void AlphabetButtonPressed8(){AlphabetButtonPressedAgent(8);}
    public void AlphabetButtonPressed9(){AlphabetButtonPressedAgent(9);}
    public void AlphabetButtonPressed10(){AlphabetButtonPressedAgent(10);}
    public void AlphabetButtonPressed11(){AlphabetButtonPressedAgent(11);}
    public void AlphabetButtonPressed12(){AlphabetButtonPressedAgent(12);}
    public void AlphabetButtonPressed13(){AlphabetButtonPressedAgent(13);}
    public void AlphabetButtonPressed14(){AlphabetButtonPressedAgent(14);}
    public void AlphabetButtonPressed15(){AlphabetButtonPressedAgent(15);}
    public void AlphabetButtonPressed16(){AlphabetButtonPressedAgent(16);}
    public void AlphabetButtonPressed17(){AlphabetButtonPressedAgent(17);}
    public void AlphabetButtonPressed18(){AlphabetButtonPressedAgent(18);}
    public void AlphabetButtonPressed19(){AlphabetButtonPressedAgent(19);}
    public void AlphabetButtonPressed20(){AlphabetButtonPressedAgent(20);}
    public void AlphabetButtonPressed21(){AlphabetButtonPressedAgent(21);}
    public void AlphabetButtonPressed22(){AlphabetButtonPressedAgent(22);}
    public void AlphabetButtonPressed23(){AlphabetButtonPressedAgent(23);}
    public void AlphabetButtonPressed24(){AlphabetButtonPressedAgent(24);}
    public void AlphabetButtonPressed25(){AlphabetButtonPressedAgent(25);}
    public void MarkButtonPressd0(){MarkButtonPressedAgent(0);}
    public void MarkButtonPressd1(){MarkButtonPressedAgent(1);}
    public void MarkButtonPressd2(){MarkButtonPressedAgent(2);}
    public void MarkButtonPressd3(){MarkButtonPressedAgent(3);}
    public void MarkButtonPressd4(){MarkButtonPressedAgent(4);}
    public void MarkButtonPressd5(){MarkButtonPressedAgent(5);}
    public void MarkButtonPressd6(){MarkButtonPressedAgent(6);}
    public void MarkButtonPressd7(){MarkButtonPressedAgent(7);}
    public void MarkButtonPressd8(){MarkButtonPressedAgent(8);}
    public void MarkButtonPressd9(){MarkButtonPressedAgent(9);}
    public void MarkButtonPressd10(){MarkButtonPressedAgent(10);}
    public void MarkButtonPressd11(){MarkButtonPressedAgent(11);}
    public void MarkButtonPressd12(){MarkButtonPressedAgent(12);}
    public void MarkButtonPressd13(){MarkButtonPressedAgent(13);}
    public void MarkButtonPressd14(){MarkButtonPressedAgent(14);}
    public void MarkButtonPressd15(){MarkButtonPressedAgent(15);}
    public void MarkButtonPressd16(){MarkButtonPressedAgent(16);}
    public void MarkButtonPressd17(){MarkButtonPressedAgent(17);}
    public void MarkButtonPressd18(){MarkButtonPressedAgent(18);}
    public void MarkButtonPressd19(){MarkButtonPressedAgent(19);}
    public void MarkButtonPressd20(){MarkButtonPressedAgent(20);}
    public void WordCandidatePressedAgent0(){WordCandidatePressedAgent(0);}
    public void WordCandidatePressedAgent1(){WordCandidatePressedAgent(1);}
    public void WordCandidatePressedAgent2(){WordCandidatePressedAgent(2);}
    public void WordCandidatePressedAgent3(){WordCandidatePressedAgent(3);}
    public void WordCandidatePressedAgent4(){WordCandidatePressedAgent(4);}
    public void SetSkin0(){SetSkin(0);}
    public void SetSkin1(){SetSkin(1);}
    public void SetSkin2(){SetSkin(2);}
    public void SetSkin3(){SetSkin(3);}
    public void SetSkin4(){SetSkin(4);}
    public void SetSkin5(){SetSkin(5);}
    public void SetSkin6(){SetSkin(6);}
    public void SetSkin7(){SetSkin(7);}
    public void SetSkin8(){SetSkin(8);}
}
}
