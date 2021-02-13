using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class spotTheSneakScript : MonoBehaviour {

    public KMBombInfo Bomb;
    public KMAudio Audio;

    public KMSelectable[] Selectables;
    public GameObject[] FakeModules;
    public GameObject SpotTheSneak;
    public TextMesh[] Texts;
    public GameObject[] Changeables;
    public GameObject StatusLight;

    public Material[] Mats;

    private List<string> fakeModList = new List<string> { "Letter Keys", "Bitmaps", "Colour Flash", "Piano Keys", "Cruel Piano Keys", "Festive Piano Keys", "Anagrams", "Word Scramble" };
    private int chosenModule = -1;
    private int chosenChange = -1;
    private List<int> chosenMultichange = new List<int> { };

    private int RNG = 0;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    private Coroutine someHold;
	private bool holding = false;

    static string alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    static string digits = "0123456789";
    static string alphanumerics = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    void Awake () {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable something in Selectables) {
            something.OnInteract += delegate () { Press(); return false; };
            something.OnInteractEnded += delegate () { Release(); };
        }

    }

    // Use this for initialization
    void Start () {
        RNG = UnityEngine.Random.Range(0, 1000000);

        for (int i = 0; i < FakeModules.Count(); i++) {
            FakeModules[i].gameObject.SetActive(false);
        }
        chosenModule = UnityEngine.Random.Range(0, FakeModules.Count());
        chosenModule = 7; //used for forcing a certain module to appear; comment this out when released.
        FakeModules[chosenModule].gameObject.SetActive(true);
        Debug.LogFormat("[Spot the Sneak #{0}] I may look like {1}, but do not be fooled...", moduleId, fakeModList[chosenModule]);

        switch (fakeModList[chosenModule]) {
            case "Bitmaps": FakeBitmaps(); break;
            case "Colour Flash": StartCoroutine(FakeColourFlash()); break;
            case "Letter Keys": FakeLetterKeys(); break;
            case "Piano Keys": FakePianoKeys(); break;
            case "Cruel Piano Keys": FakeCruelPianoKeys(); break;
            case "Festive Piano Keys": FakeFestivePianoKeys(); break;
            case "Anagrams": FakeAnagrams(); break;
            case "Word Scramble": FakeWordScramble(); break;
            default:
                Debug.LogFormat("[Spot the Sneak #{0}] Couldn't decide what module to be. This is a bug, please tell Blananas2 immediately.", moduleId);
                GetComponent<KMBombModule>().HandlePass();
                break;
        }

        StartCoroutine(Laugh());
    }

    void Press() {
        Selectables[0].AddInteractionPunch();
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        if (someHold != null)
		{
			holding = false;
			StopCoroutine(someHold);
			someHold = null;
		}
		someHold = StartCoroutine(HoldChecker());
    }

    void Release () {
        StopCoroutine(someHold);
        if (holding == true) {
            Debug.LogFormat("[Spot the Sneak #{0}] You correctly identified that I'm Spot the Sneak, module solved.", moduleId);
            Audio.PlaySoundAtTransform("fuck", transform);
            FakeModules[chosenModule].gameObject.SetActive(false);
            SpotTheSneak.gameObject.SetActive(true);
            GetComponent<KMBombModule>().HandlePass();
        } else {
            Debug.LogFormat("[Spot the Sneak #{0}] You weren't able to identify that I'm Spot the Sneak. Flashing change...", moduleId);
            if (!(Bomb.GetSolvableModuleNames().Contains("Organization"))) { //If an Org is on the bomb, don't give the strike, as Org will likely strike anyway upon solve.
                GetComponent<KMBombModule>().HandleStrike();
            }
            if (chosenChange != -1) {
                StopAllCoroutines();
                StartCoroutine(Flicker());
            } else {
                StopAllCoroutines();
                StartCoroutine(FlickerMore());
            }
        }
    }

    IEnumerator HoldChecker()
    {
    	yield return new WaitForSeconds(3f);
        Audio.PlaySoundAtTransform("fuck", transform);
    	holding = true;
    }

    IEnumerator Laugh() {
        yield return new WaitForSeconds((float) 15 + (RNG % 1500)/100);
        Audio.PlaySoundAtTransform("hello", transform);
    }

    IEnumerator Flicker()
    {
        Audio.PlaySoundAtTransform("hehe", transform);
        for (int i = 0; i < 6; i++) {
            Changeables[chosenChange].gameObject.SetActive(false);
            yield return new WaitForSeconds(0.25f);
            Changeables[chosenChange].gameObject.SetActive(true);
            yield return new WaitForSeconds(0.25f);
        }
        Audio.PlaySoundAtTransform("hehe", transform);
        FakeModules[chosenModule].gameObject.SetActive(false);
        SpotTheSneak.gameObject.SetActive(true);
        GetComponent<KMBombModule>().HandlePass();
        Debug.LogFormat("[Spot the Sneak #{0}] Module solved.", moduleId);
    }

    IEnumerator FlickerMore()
    {
        Audio.PlaySoundAtTransform("hehe", transform);
        for (int j = 0; j < 6; j++) {
            for (int i = 0; i < chosenMultichange.Count(); i++) { Changeables[chosenMultichange[i]].gameObject.SetActive(false); }
            yield return new WaitForSeconds(0.25f);
            for (int i = 0; i < chosenMultichange.Count(); i++) { Changeables[chosenMultichange[i]].gameObject.SetActive(true); }
            yield return new WaitForSeconds(0.25f);
        }
        Audio.PlaySoundAtTransform("hehe", transform);
        FakeModules[chosenModule].gameObject.SetActive(false);
        SpotTheSneak.gameObject.SetActive(true);
        GetComponent<KMBombModule>().HandlePass();
        Debug.LogFormat("[Spot the Sneak #{0}] Module solved.", moduleId);
    }

    //////////Actual fake module methods start here

    void FakeAnagrams() { //Selectables 46-53, Texts 15-24, Changables 14-23
        StatusLight.transform.localPosition = new Vector3(-0.075167f, 0.01986f, 0.076057f);

        string[] anagrams = {"STREAM", "MASTER", "TAMERS", "LOOPED", "POODLE", "POOLED", "CELLAR", "CALLER", "RECALL", "SEATED", "SEDATE", "TEASED", "RESCUE", "SECURE", "RECUSE", "RASHES", "SHEARS", "SHARES", "BARELY", "BARLEY", "BLEARY", "DUSTER", "RUSTED", "RUDEST"};
        string[] wordscramble = {"ARCHER", "ATTACK", "BANANA", "BLASTS", "BURSTS", "BUTTON", "CANNON", "CASING", "CHARGE", "DAMAGE", "DEFUSE", "DEVICE", "DISARM", "FLAMES", "KABOOM", "KEVLAR", "KEYPAD", "LETTER", "MODULE", "MORTAR", "NAPALM", "OTTAWA", "PERSON", "ROBOTS", "ROCKET", "SAPPER", "SEMTEX", "WEAPON", "WIDGET", "WIRING" };
        string chosenA = anagrams.PickRandom();

        string unfuckedword = wordscramble.PickRandom();
        List<string> fuckedword = new List<string> {};
        for (int i = 0; i < 6; i++) {
            fuckedword.Add(unfuckedword[i].ToString());
        }
        fuckedword.Shuffle();
        string chosenWS = fuckedword[0] + fuckedword[1] + fuckedword[2] + fuckedword[3] + fuckedword[4] + fuckedword[5];

        switch (RNG % 3) {
            case 0: //Word Scramble instead
                Texts[23].text = chosenWS;
                for (int i = 0; i < 6; i++) {
                    Texts[15+i].text = chosenWS[0+i].ToString();
                }
                Debug.LogFormat("[Spot the Sneak #{0}] ...there's a scrambled word on the screen, that doesn't seem normal.", moduleId);
                chosenMultichange.Add(22); chosenMultichange.Add(15); chosenMultichange.Add(16); chosenMultichange.Add(17); chosenMultichange.Add(18); chosenMultichange.Add(19); chosenMultichange.Add(14);
            break;
            case 1: //DEL & ENT on wrong side
                Texts[23].text = chosenA;
                Texts[15].text = "DEL"; Texts[18].text = "OK";
                int[] jank = { 16, 17, 21, 19, 20, 22 };
                for (int i = 0; i < 6; i++) {
                    Texts[jank[i]].text = chosenA[i].ToString();
                }
                Debug.LogFormat("[Spot the Sneak #{0}] ...DEL and OK are on the left, that doesn't seem normal.", moduleId);
                chosenMultichange.Add(15); chosenMultichange.Add(16); chosenMultichange.Add(17); chosenMultichange.Add(18); chosenMultichange.Add(19); chosenMultichange.Add(14); chosenMultichange.Add(20); chosenMultichange.Add(21);
            break;
            case 2: //Bottom screen has text instead of top
                Texts[23].text = "";
                Texts[24].text = chosenA;
                for (int i = 0; i < 6; i++) {
                    Texts[15+i].text = chosenA[0+i].ToString();
                }
                Debug.LogFormat("[Spot the Sneak #{0}] ...the anagram is on the bottom screen, that doesn't seem normal.", moduleId);
                chosenMultichange.Add(23);
            break;
            default:
                Debug.LogFormat("[Spot the Sneak #{0}] Couldn't decide what to change. This is a bug, please tell Blananas2 immediately.", moduleId);
                GetComponent<KMBombModule>().HandlePass();
            break;
        }
    }

    void FakeBitmaps() { //Selectables 4-7, Texts 5-8, Changeables 5-8; Mats 0
        int xcord = UnityEngine.Random.Range(0, 6);
        int ycord = UnityEngine.Random.Range(0, 6);
        Mats[0].mainTextureOffset = new Vector2((float)xcord/6, (float)ycord/6);
        int bit = (RNG%8)/4;
        if (bit == 0) {
            string avoid = digits[(RNG%4) + 1].ToString();
            string changeTo = digits.PickRandom().ToString();
            while (changeTo == avoid) {
                changeTo = digits.PickRandom().ToString();
            }
            chosenChange = (RNG%4)+5;
            Texts[(RNG%4)+5].text = changeTo.ToString();
            string[] positions = {"1st", "2nd", "3rd", "4th"};
            Debug.LogFormat("[Spot the Sneak #{0}] ...the {1} button is {2}, that doesn't seem normal.", moduleId, positions[(RNG%4)], changeTo);
        } else {
            switch (RNG%4) {
                case 0:
                    chosenMultichange.Add(5); chosenMultichange.Add(6);
                    Texts[5].text = " 2 "; Texts[6].text = " 1 ";
                    Debug.LogFormat("[Spot the Sneak #{0}] ...the '1' and '2' have swapped, that doesn't seem normal.", moduleId);
                break;
                case 1:
                    chosenMultichange.Add(6); chosenMultichange.Add(7);
                    Texts[6].text = " 3 "; Texts[7].text = " 2 ";
                    Debug.LogFormat("[Spot the Sneak #{0}] ...the '2' and '3' have swapped, that doesn't seem normal.", moduleId);
                break;
                case 2:
                    chosenMultichange.Add(7); chosenMultichange.Add(8);
                    Texts[7].text = " 4 "; Texts[8].text = " 3 ";
                    Debug.LogFormat("[Spot the Sneak #{0}] ...the '3' and '4' have swapped, that doesn't seem normal.", moduleId);
                break;
                case 3:
                    chosenMultichange.Add(5); chosenMultichange.Add(6); chosenMultichange.Add(7); chosenMultichange.Add(8);
                    Texts[5].text = " 4 "; Texts[6].text = " 3 "; Texts[7].text = " 2 "; Texts[8].text = " 1 ";
                    Debug.LogFormat("[Spot the Sneak #{0}] ...the numbers at the bottom are in reverse order, that doesn't seem normal.", moduleId);
                break;
                default:
                    Debug.LogFormat("[Spot the Sneak #{0}] Couldn't decide what to change. This is a bug, please tell Blananas2 immediately.", moduleId);
                    GetComponent<KMBombModule>().HandlePass();
                break;
            }
        }
    }

    IEnumerator FakeColourFlash () { //Selectables 8-9, Texts 9-11, Changeables 9-10
        string[] fakeYes = {"YEE", "YAS", "YEP", "YEA", "YEH", "YAH"};
        string[] fakeNo = {"NOPE", "NAH", "NAW", "NOT", "NIL", "NADA"};
        int number = UnityEngine.Random.Range(0, 6);
        switch (RNG % 3) {
            case 0:
                Texts[10].text = fakeYes[number];
                chosenChange = 9;
                Debug.LogFormat("[Spot the Sneak #{0}] ...the left button says {1}, that doesn't seem normal.", moduleId, fakeYes[number]);
            break;
            case 1:
                Texts[11].text = fakeNo[number];
                chosenChange = 10;
                Debug.LogFormat("[Spot the Sneak #{0}] ...the right button says {1}, that doesn't seem normal.", moduleId, fakeNo[number]);
            break;
            case 2:
                Texts[10].text = "NO";
                Texts[11].text = "YES";
                chosenMultichange.Add(9); chosenMultichange.Add(10);
                Debug.LogFormat("[Spot the Sneak #{0}] ...the 'YES' and 'NO' buttons have swapped, that doesn't seem normal.", moduleId, fakeNo[number]);
            break;
            default:
                Debug.LogFormat("[Spot the Sneak #{0}] Couldn't decide what to change. This is a bug, please tell Blananas2 immediately.", moduleId);
                GetComponent<KMBombModule>().HandlePass();
            break;
        }
        ///// ^ ABOVE ^ is quirk, v BELOW v is color cycling function
        string[] colors = {"RED", "YELLOW", "GREEN", "BLUE", "MAGENTA", "WHITE"};
        float[] fcolors = {
            1f, 0f, 0f,
            1f, 1f, 0f,
            0f, 1f, 0f,
            0f, 0f, 1f,
            1f, 0f, 1f,
            1f, 1f, 1f,
        };
        List<string> wordSeq = new List<string> {};
        List<int> colSeq = new List<int> {};
        for (int i = 0; i < 8; i++) {
            wordSeq.Add(colors[UnityEngine.Random.Range(0,6)]);
            colSeq.Add(UnityEngine.Random.Range(0,6));
        }
        StartOver:
        for (int j = 0; j < 8; j++) {
            Texts[9].text = wordSeq[j];
            Texts[9].color = new Color(fcolors[3*colSeq[j]], fcolors[3*colSeq[j]+1], fcolors[3*colSeq[j]+2]);
            yield return new WaitForSeconds(0.75f);
        }
        Texts[9].text = "";
        yield return new WaitForSeconds(2f);
        goto StartOver;
        yield return null;
    }

    void FakeCruelPianoKeys() { //Selectables 22-33, Text 13, Changable 12
        chosenChange = 12;
        string set = "nb#mTcCUB";
        List<string> symbols = new List<string> {};
        for (int i = 0; i < 4; i++) {
            symbols.Add(set.PickRandom().ToString());
        }
        //01 02 03 12 13 23
        if ((symbols[0] == symbols[1]) || (symbols[0] == symbols[2]) || (symbols[0] == symbols[3])
        || (symbols[1] == symbols[2]) || (symbols[1] == symbols[3]) || (symbols[2] == symbols[3])) { //had to split this line bc atom got angry
            Debug.LogFormat("[Spot the Sneak #{0}] ...the display has identical symbols, that doesn't seem normal.", moduleId);
        } else {
            string dummys = "\"%*>^_vwx"; //Sadly I can only use symbols here that are in either Normal or Festive. :(
            symbols.Add(dummys.PickRandom().ToString());
            symbols[RNG%4] = symbols[4];
            Debug.LogFormat("[Spot the Sneak #{0}] ...the display has a strange symbol ({1}), that doesn't seem normal.", moduleId, PianoSymbol(symbols[4]));
        }
        Texts[13].text = symbols[0] + "  " + symbols[1] + "  " + symbols[2] + "  " + symbols[3];
        Debug.LogFormat("[Spot the Sneak #{0}] Symbols: {1}, {2}, {3}, {4}", moduleId, PianoSymbol(symbols[0]), PianoSymbol(symbols[1]), PianoSymbol(symbols[2]), PianoSymbol(symbols[3]));
    }

    void FakeFestivePianoKeys() { //Selectables 34-45, Text 14, Changable 13
        chosenChange = 13;
        string set = "mB\"%x*v^w>";
        List<string> symbols = new List<string> {};
        for (int i = 0; i < 3; i++) {
            symbols.Add(set.PickRandom().ToString());
        }
        if ((symbols[0] == symbols[1]) || (symbols[0] == symbols[2]) || (symbols[1] == symbols[2])) {
            Debug.LogFormat("[Spot the Sneak #{0}] ...the display has identical symbols, that doesn't seem normal.", moduleId);
        } else {
            string dummys = "nb#TcCU"; //Sadly I can only use symbols here that are in either Cruel or Normal. :(
            symbols.Add(dummys.PickRandom().ToString());
            symbols[RNG%3] = symbols[3];
            Debug.LogFormat("[Spot the Sneak #{0}] ...the display has a strange symbol ({1}), that doesn't seem normal.", moduleId, PianoSymbol(symbols[3]));
        }
        Texts[14].text = symbols[0] + "  " + symbols[1] + "  " + symbols[2];
        Debug.LogFormat("[Spot the Sneak #{0}] Symbols: {1}, {2}, {3}", moduleId, PianoSymbol(symbols[0]), PianoSymbol(symbols[1]), PianoSymbol(symbols[2]));
    }

    void FakeLetterKeys() { //Selectables 0-3, Texts 0-4, Changeables 0-4
        int number = UnityEngine.Random.Range(0, 100);
        Texts[0].text = number.ToString();
        char[] letters = {'A', 'B', 'C', 'D'};
        letters.Shuffle();
        for (int i = 0; i < 4; i++) {
            Texts[1+i].text = letters[i].ToString();
        }
        if (RNG % 5 == 0) {
            chosenChange = 0;
            string letter = alphabet.PickRandom().ToString();
            while (letter == "O".ToString() || letter == "I".ToString()) {
                letter = alphabet.PickRandom().ToString();
            }
            if (RNG % 2 == 0) {
                Texts[0].text = letter + "" + number%10;
            } else {
                Texts[0].text = number/10 + "" + letter;
                if (number / 10 == 0) {
                    Texts[0].text = letter.ToString();
                }
            }
            Debug.LogFormat("[Spot the Sneak #{0}] ...the number on the display is {1}, that doesn't seem normal.", moduleId, Texts[0].text);
        } else {
            chosenChange = RNG%5;
            string avoid = Texts[RNG%5].text.ToString();
            string changeTo = alphanumerics.PickRandom().ToString();
            while (changeTo == avoid) {
                changeTo = alphanumerics.PickRandom().ToString();
            }
            Texts[RNG%5].text = changeTo.ToString();
            string[] places = {"top-left", "top-right", "bottom-left", "bottom-right"};
            Debug.LogFormat("[Spot the Sneak #{0}] ...the {1} button is {2}, that doesn't seem normal.", moduleId, places[(RNG%5)-1], changeTo);
        }
    }

    void FakePianoKeys() { //Selectables 10-21, Text 12, Changable 11
        chosenChange = 11;
        string set = "nb#mTcCUB";
        List<string> symbols = new List<string> {};
        for (int i = 0; i < 3; i++) {
            symbols.Add(set.PickRandom().ToString());
        }
        if ((symbols[0] == symbols[1]) || (symbols[0] == symbols[2]) || (symbols[1] == symbols[2])) {
            Debug.LogFormat("[Spot the Sneak #{0}] ...the display has identical symbols, that doesn't seem normal.", moduleId);
        } else {
            string dummys = "\"%*>^_vwx"; //Sadly I can only use symbols here that are in either Cruel or Festive. :(
            symbols.Add(dummys.PickRandom().ToString());
            symbols[RNG%3] = symbols[3];
            Debug.LogFormat("[Spot the Sneak #{0}] ...the display has a strange symbol ({1}), that doesn't seem normal.", moduleId, PianoSymbol(symbols[3]));
        }
        Texts[12].text = symbols[0] + "  " + symbols[1] + "  " + symbols[2];
        Debug.LogFormat("[Spot the Sneak #{0}] Symbols: {1}, {2}, {3}", moduleId, PianoSymbol(symbols[0]), PianoSymbol(symbols[1]), PianoSymbol(symbols[2]));
    }

    void FakeWordScramble() { //Selectables 54-61, Text 25-34, Changables 24-33
        string[] anagrams = {"STREAM", "MASTER", "TAMERS", "LOOPED", "POODLE", "POOLED", "CELLAR", "CALLER", "RECALL", "SEATED", "SEDATE", "TEASED", "RESCUE", "SECURE", "RECUSE", "RASHES", "SHEARS", "SHARES", "BARELY", "BARLEY", "BLEARY", "DUSTER", "RUSTED", "RUDEST"};
        string[] wordscramble = {"ARCHER", "ATTACK", "BANANA", "BLASTS", "BURSTS", "BUTTON", "CANNON", "CASING", "CHARGE", "DAMAGE", "DEFUSE", "DEVICE", "DISARM", "FLAMES", "KABOOM", "KEVLAR", "KEYPAD", "LETTER", "MODULE", "MORTAR", "NAPALM", "OTTAWA", "PERSON", "ROBOTS", "ROCKET", "SAPPER", "SEMTEX", "WEAPON", "WIDGET", "WIRING" };
        string chosenA = anagrams.PickRandom();

        string unfuckedword = wordscramble.PickRandom();
        List<string> fuckedword = new List<string> {};
        for (int i = 0; i < 6; i++) {
            fuckedword.Add(unfuckedword[i].ToString());
        }
        fuckedword.Shuffle();
        string chosenWS = fuckedword[0] + fuckedword[1] + fuckedword[2] + fuckedword[3] + fuckedword[4] + fuckedword[5];

        switch (RNG % 3) {
            case 0: //Anagrams instead
                Texts[33].text = chosenA;
                for (int i = 0; i < 6; i++) {
                    Texts[25+i].text = chosenA[0+i].ToString();
                }
                Debug.LogFormat("[Spot the Sneak #{0}] ...there's an anagram on the screen, that doesn't seem normal.", moduleId);
                chosenMultichange.Add(32); chosenMultichange.Add(25); chosenMultichange.Add(26); chosenMultichange.Add(27); chosenMultichange.Add(28); chosenMultichange.Add(29); chosenMultichange.Add(24);
            break;
            case 1: //DEL & ENT on wrong side
                Texts[33].text = chosenWS;
                Texts[25].text = "DEL"; Texts[28].text = "OK";
                int[] jank = { 26, 27, 31, 29, 30, 32 };
                for (int i = 0; i < 6; i++) {
                    Texts[jank[i]].text = chosenWS[i].ToString();
                }
                Debug.LogFormat("[Spot the Sneak #{0}] ...DEL and OK are on the left, that doesn't seem normal.", moduleId);
                chosenMultichange.Add(25); chosenMultichange.Add(26); chosenMultichange.Add(27); chosenMultichange.Add(28); chosenMultichange.Add(29); chosenMultichange.Add(24); chosenMultichange.Add(30); chosenMultichange.Add(31);
            break;
            case 2: //Bottom screen has text instead of top
                Texts[33].text = "";
                Texts[34].text = chosenWS;
                for (int i = 0; i < 6; i++) {
                    Texts[25+i].text = chosenWS[0+i].ToString();
                }
                Debug.LogFormat("[Spot the Sneak #{0}] ...the scrambled word is on the bottom screen, that doesn't seem normal.", moduleId);
                chosenMultichange.Add(33);
            break;
            default:
                Debug.LogFormat("[Spot the Sneak #{0}] Couldn't decide what to change. This is a bug, please tell Blananas2 immediately.", moduleId);
                GetComponent<KMBombModule>().HandlePass();
            break;
        }
    }

    //////////Some extra methods go down here

    string PianoSymbol(string x) {
        switch (x) {
            case "n": return "Natural"; break;
            case "b": return "Flat"; break;
            case "#": return "Sharp"; break;
            case "m": return "Mordent"; break;
            case "T": return "Turn"; break;
            case "c": return "Common Time"; break;
            case "C": return "Cut-common Time"; break;
            case "U": return "Fermata"; break;
            case "B": return "C Clef"; break;
            case "": return "Crotchet Rest"; break;
            case "": return "Down-bow"; break;
            case "": return "Breve"; break;
            case "": return "Semiquaver Rest"; break;
            case "": return "Double Sharp"; break;
            case "x": return "Semiquaver Note"; break;
            case "w": return "Semibreve Note"; break;
            case "v": return "Up-bow"; break;
            case "\"": return "Caesura"; break;
            case "%": return "Dal Segno"; break;
            case "^": return "Marcato"; break;
            case "*": return "Pedal Up"; break;
            case ">": return "Accent"; break;
            default: return null; break;
        }
    }

}
