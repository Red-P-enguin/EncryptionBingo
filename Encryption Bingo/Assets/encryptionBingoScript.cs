using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class encryptionBingoScript : MonoBehaviour {

    public KMBossModule BossModule;
    public KMBombModule Module;

    //general stuff
    public KMBombInfo bomb;
    public KMAudio audio;
    static int ModuleIdCounter = 1;
    int ModuleId;
    private bool moduleSolved = false;
    private bool incorrect = false;
    public KMSelectable[] buttons;
    public Renderer ballRenderer;
    public Renderer[] stampLocations;
    public Material stamp;
    private int index = 0;
    //bingo stuff
    private int encryptionIndex = 0;
    private int numberOfStamps = 0;
    private int selLine = 0;
    private int correctSquare = 0;
    private int balls = 0;
    private int localRotation = 0;
    private int whichButtonPressed = 0;
    private bool animationActive = false;
    private List<int> stampedSquares = new List<int>();
    private List<int> stampedStamps = new List<int>();
    public SpriteRenderer bingoText;
    public Sprite[] bingos;
    private bool somethingActive = false;
    private bool stageDone = false;
    private bool ballOut = false;
    private string chart1 = "ELWGN" +
                            "ROZTB" +
                            "UDPVC" +
                            "XQYFM" +
                            "HAJIS";
    private string chart2 = "OQTKU" +
                            "NEVWI" +
                            "XLMAP" +
                            "GHYCZ" +
                            "SBFRD";
    private string chart3 = "DGTAR" +
                            "MYESB" +
                            "CIHUX" +
                            "OJWQN" +
                            "PFVKL";

    int[,] lines = new int[12, 5]      {{ 0,1,2,3,4 },
                                        { 5,6,7,8,9 },
                                        { 10,11,12,13,14 },
                                        { 15,16,17,18,19 },
                                        { 20,21,22,23,24 },
                                        { 0,5,10,15,20 },
                                        { 1,6,11,16,21 },
                                        { 2,7,12,17,22 },
                                        { 3,8,13,18,23 },
                                        { 4,9,14,19,24 },
                                        { 0,6,12,18,24 },
                                        { 4,8,12,16,20 }};
    //encryptions
    private string[] encryptions = { "Morse Code", "Tap Code", "Maritime Flags", "Semaphore", "Pigpen", "Lombax", "Braille", "Wingdings", "Zoni", "Galatic Alphabet", "Arrow", "Listening", "Regular Number", "Chinese Number", "Cube Symbols", "Runes", "New York Point", "Fontana", "ASCII Hex Code" };
    private string[] morseLetters = { ".",".-..",".--","--.","-.",".-.","---","--..","-","-...","..-","-..",".--.","...-","-.-.","-..-","--.-","-.--","..-.","--","....",".-",".---","..","..." };
    public Material[] morseMats;
    private string[] tapLetters = { ". .....", "... .", "..... ..", ".. ..", "... ...", ".... ..", "... ....", "..... .....", ".... ....", ". ..", ".... .....", ". ....", "... .....", "..... .", ". ...", "..... ...", ".... .", "..... ....", ".. .", "... ..", ".. ...", ". .", ".. .....", ".. ....", ".... ..." };
    public Material tapMat;
    public Material[] maritimeMats;
    public Material[] semaphoreMats;
    public Material[] pigpenMats;
    public Material[] lombaxMats;
    public Material[] brailleMats;
    public Material[] wingMats;
    public Material[] zoniMats;
    public Material[] galacticMats;
    public Material[] arrowMats;
    public Material[] gridlockMats1;
    public Material[] gridlockMats2;
    private int arrowHoriz = 0;
    private int arrowVert = 0;
    public Material listeningMat;
    public string[] listeningNames;
    private int[] listeningLength = { 4,};
    public AudioClip[] listeningClips;
    public Material[] numberMats;
    public Material[] chineseNumberMats;
    public Material[] cubeMats;
    private char[] cubeNotLetters = { 'R','S','T','U','V','W','Y','Z' };
    public Material[] runeMats;
    private char[] runeNotLetters = { 'C', 'K', 'Q', 'V', 'W' };
    public Material[] nypMats;
    public Material[] fontanaMats;
    private char[] fontanaNotLetters = { 'J', 'V', 'W' };
    public Material[] asciiMats;
    //boss module stuff
    private static readonly string[] defaultIgnoredModules = @"Divided Squares,Forget Me Not,Forget Everything,Forget This,Hogwarts,Turn The Key,The Time Keeper,Souvenir,The Swan,Simon's Stages,Purgatory,Alchemy,Timing is Everything".Split(',');
    private string[] ignoredModules;
    private int count = 0;
    private int stage;
    private int ticker;
    private bool done;

    void Start ()
    {
        ballRenderer.transform.localPosition = new Vector3(0.06f, 0.0245f, 0.0215f);
        stageDone = false;
        selLine = UnityEngine.Random.Range(0, 12);
        numberOfStamps = UnityEngine.Random.Range(5, 16);
        stampedStamps.Clear();
        //how many modules can be solved before this one
        count = bomb.GetSolvableModuleNames().Where(x => !ignoredModules.Contains(x)).Count();
        if (count == 0)
        { //Prevent deadlock
            DebugMsg("No valid modules, releasing multiple balls until solved.");
            done = true;
        }
        for (int i = 0; i < 5; i++)
        {
            stampedStamps.Add(lines[selLine, i]);
        }
        chooseRandomStamps();
    }

    void chooseRandomStamps()
    {
        if (stampedStamps.Count < numberOfStamps)
        {
            index = UnityEngine.Random.Range(0,24);
            if(!stampedStamps.Contains(index))
            {
                stampedStamps.Add(index);
            }
            chooseRandomStamps();
        }
        else
        {
            ChooseBall();
        }
    }

    private IEnumerator solvedAnimation()
    {
        while (localRotation != 75)
        {
            yield return new WaitForSeconds(.02f);
            index = UnityEngine.Random.Range(0, 8);
            bingoText.sprite = bingos[index];
            localRotation++;
        }
        bingoText.sprite = bingos[4];
        localRotation = 0;
    }

    //animation, localRotation comes from The Cube
    private IEnumerator ComeHereBall()
    {
        animationActive = true;
        index = UnityEngine.Random.Range(0,3);
        if(index == 0)
        {
            audio.PlaySoundAtTransform("roll1", transform);
        }
        else if(index == 1)
        {
            audio.PlaySoundAtTransform("roll2", transform);
        }
        else
        {
            audio.PlaySoundAtTransform("roll3", transform);
        }
        while (localRotation != 36)
        {
            yield return new WaitForSeconds(.02f);
            ballRenderer.transform.localPosition = new Vector3(ballRenderer.transform.localPosition.x, ballRenderer.transform.localPosition.y, ballRenderer.transform.localPosition.z - 0.001125f);
            ballRenderer.transform.localRotation = Quaternion.Euler(-5.0f, 0.0f, 0.0f) * ballRenderer.transform.localRotation; ;
            localRotation++;
        }
        localRotation = 0;
        animationActive = false;
    }

    private IEnumerator GoAwayBall()
    {
        animationActive = true;
        if (whichButtonPressed != correctSquare)
        {
            incorrect = true;
        }
        if (!incorrect)
        {
            index = UnityEngine.Random.Range(0, 3);
            if (index == 0)
            {
                audio.PlaySoundAtTransform("stamp1", transform);
            }
            else if (index == 1)
            {
                audio.PlaySoundAtTransform("stamp2", transform);
            }
            else
            {
                audio.PlaySoundAtTransform("stamp3", transform);
            }
            balls--;
            stampedSquares.Add(whichButtonPressed);
            stampLocations[whichButtonPressed].material = stamp;
        }
        else
        {
            GetComponent<KMBombModule>().HandleStrike();
            DebugMsg("Strike! Pressed " + buttons[whichButtonPressed].name + ", when the correct button was " + buttons[correctSquare].name + ".");
        }
        yield return new WaitForSeconds(.25f);
        index = UnityEngine.Random.Range(0, 3);
        if (index == 0)
        {
            audio.PlaySoundAtTransform("roll1", transform);
        }
        else if (index == 1)
        {
            audio.PlaySoundAtTransform("roll2", transform);
        }
        else
        {
            audio.PlaySoundAtTransform("roll3", transform);
        }
        while (localRotation != 36)
        {
            yield return new WaitForSeconds(.02f);
            ballRenderer.transform.localPosition = new Vector3(ballRenderer.transform.localPosition.x, ballRenderer.transform.localPosition.y, ballRenderer.transform.localPosition.z - 0.001125f);
            ballRenderer.transform.localRotation = Quaternion.Euler(-5.0f, 0.0f, 0.0f) * ballRenderer.transform.localRotation; ;
            localRotation++;
        }
        localRotation = 0;
        index = UnityEngine.Random.Range(0, 2);
        if (index == 0)
        {
            audio.PlaySoundAtTransform("thud1", transform);
        }
        else
        {
            audio.PlaySoundAtTransform("thud2", transform);
        }
        while (localRotation != 5)
        {
            yield return new WaitForSeconds(.02f);
            ballRenderer.transform.localPosition = new Vector3(ballRenderer.transform.localPosition.x, ballRenderer.transform.localPosition.y -0.0049f, ballRenderer.transform.localPosition.z);
            localRotation++;
        }
        localRotation = 0;
        ballRenderer.transform.localPosition = new Vector3(0.06f, 0.0245f, 0.0215f);
        yield return new WaitForSeconds(.25f);
        ballOut = false;
        if (!incorrect)
        {
            if (isSolved())
            {
                audio.PlaySoundAtTransform("solve", transform);
                DebugMsg("Bingo! Module solved.");
                moduleSolved = true;
                GetComponent<KMBombModule>().HandlePass();
                StartCoroutine(solvedAnimation());
            }
            else
            {
                if (done == true)
                {
                    ChooseBall();
                }
                else if (balls > 0)
                {
                    ChooseBall();
                }
                else
                {
                    stageDone = true;
                }
            }
        }
        else
        {
            incorrect = false;
            ChooseBall();
        }
    }

    void ChooseBall() //pretty self explanatory
    {
        ballOut = true;
        encryptionIndex = UnityEngine.Random.Range(0, 19);
        if (encryptionIndex == 10 && stampedSquares.Count == 0 || encryptionIndex == 12 && stampedSquares.Count == 0 || encryptionIndex == 12 && stampedSquares[stampedSquares.Count - 1] == 24)
        {
            ChooseBall();
        }
        else
        {
            StartCoroutine(ComeHereBall());
            if (balls == 0 && !done)
            {
                stageDone = false;
                balls = UnityEngine.Random.Range(1, 3);
                DebugMsg("This stage has " + balls + " ball(s).");
            }
            DebugMsg("The next ball is a " + encryptions[encryptionIndex] + " ball.");
            if (encryptionIndex == 0)
            {
                morseCode();
            }
            if (encryptionIndex == 1)
            {
                ballRenderer.material = tapMat;
                tapCode();
            }
            if (encryptionIndex == 10)
            {
                index = UnityEngine.Random.Range(0, 3);
                if (index == 0)
                {
                    index = UnityEngine.Random.Range(0, 8);
                    ballRenderer.material = arrowMats[index];
                }
                else if (index == 1)
                {
                    index = UnityEngine.Random.Range(0, 8);
                    ballRenderer.material = gridlockMats1[index];
                }
                else
                {
                    index = UnityEngine.Random.Range(0, 8);
                    ballRenderer.material = gridlockMats2[index];
                }
                arrowHoriz = stampedSquares[stampedSquares.Count - 1] % 5;
                arrowVert = (stampedSquares[stampedSquares.Count - 1] - arrowHoriz) / 5;
                arrows();
            }
            if (encryptionIndex == 11)
            {
                listening();
            }
            if(encryptionIndex == 12)
            {
                numbers();
            }
            if (encryptionIndex == 13)
            {
                chineseNumbers();
            }
            if (encryptionIndex == 14)
            {
                for(int i = 0; i < stampedStamps.Count; i++)
                {
                    if(cubeNotLetters.Contains(chart3[stampedStamps[i]]) && i == stampedStamps.Count - 1)
                    {
                        ChooseBall();
                    }
                    else
                    {
                        i = stampedStamps.Count;
                        cube();
                    }
                }
            }
            if (encryptionIndex == 15)
            {
                runes();
            }
            if (encryptionIndex == 17)
            {
                fontana();
            }
            if (encryptionIndex == 2 || encryptionIndex == 3 || encryptionIndex == 4 || encryptionIndex == 5 || encryptionIndex == 6 || encryptionIndex == 7 || encryptionIndex == 8 || encryptionIndex == 9 || encryptionIndex == 16 || encryptionIndex == 18)
            {
                picture();
            }
        }
    }

    void Update()
    {
        ticker++;
        if(ticker == 15)
        {
            //every 15 things i don't know i literally just ctrl+c and ctrl+v'd this from forget enigma it's not plagurism it's being resourceful
            ticker = 0;
            int progress = bomb.GetSolvedModuleNames().Where(x => !ignoredModules.Contains(x)).Count();
            if (progress > stage && !done && !moduleSolved)
            {
                DebugMsg("A module was solved.");
                stage++;
                if (stageDone == false)
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    DebugMsg("Not all active balls were stamped. Module striked.");
                }
                if (stage >= count)
                {
                    DebugMsg("All modules solved. Now releasing multiple balls until solved.");
                    done = true;
                    stage = 0;
                    if (ballOut == false)
                    {
                        ChooseBall();
                    }
                }
                else if (stageDone == true)
                {
                    DebugMsg("All active balls stamped. Releasing next ball.");
                    ChooseBall();
                }
            }
        }
    }

    void Awake()
    {
        ModuleId = ModuleIdCounter++;

        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate () { buttonPressed(pressedButton); return false; };
        }

        if (ignoredModules == null)
        {
            //not sure what this does
            ignoredModules = BossModule.GetIgnoredModules(Module, defaultIgnoredModules);
        }
    }

    void buttonPressed(KMSelectable pressedButton)
    {
        pressedButton.AddInteractionPunch();
        if (moduleSolved || balls == 0 && done != true || animationActive == true)
        {
            return;
        }
        if (pressedButton == buttons[25])
        {
            //this means the ball is pressed
            ballPressed();
        }
        else
        {
            somethingActive = false;
            for (int i = 0; i < 25; i++)
            {
                if (buttons[i] == pressedButton && !stampedSquares.Contains(i))
                {
                    whichButtonPressed = i;
                    i = 25;
                    StopAllCoroutines();
                    //stop any sounds or flashy flashes
                    StartCoroutine(GoAwayBall());
                }
            }
        }
    }

    void ballPressed()
    {
        if (somethingActive == false)
        {
            somethingActive = true;
            if (encryptionIndex == 1)
            {
                //tap sound code
                StartCoroutine(TapSound(tapLetters[index]));
            }
            else if (encryptionIndex == 11)
            {
                //listening sound
                StartCoroutine(ListenSound());
            }
            else
            {
                somethingActive = false;
            }
        }
    }

    private bool isSolved()
    {
        if ((stampedSquares.Contains(0) && stampedSquares.Contains(6) && stampedSquares.Contains(12) && stampedSquares.Contains(18) && stampedSquares.Contains(24)) || (stampedSquares.Contains(4) && stampedSquares.Contains(8) && stampedSquares.Contains(12) && stampedSquares.Contains(16) && stampedSquares.Contains(20)) || (stampedSquares.Contains(0) && stampedSquares.Contains(5) && stampedSquares.Contains(10) && stampedSquares.Contains(15) && stampedSquares.Contains(20)) || (stampedSquares.Contains(1) && stampedSquares.Contains(6) && stampedSquares.Contains(11) && stampedSquares.Contains(16) && stampedSquares.Contains(21)) || (stampedSquares.Contains(2) && stampedSquares.Contains(7) && stampedSquares.Contains(12) && stampedSquares.Contains(17) && stampedSquares.Contains(22)) || (stampedSquares.Contains(3) && stampedSquares.Contains(8) && stampedSquares.Contains(13) && stampedSquares.Contains(18) && stampedSquares.Contains(23)) || (stampedSquares.Contains(4) && stampedSquares.Contains(9) && stampedSquares.Contains(14) && stampedSquares.Contains(19) && stampedSquares.Contains(24)) || (stampedSquares.Contains(0) && stampedSquares.Contains(1) && stampedSquares.Contains(2) && stampedSquares.Contains(3) && stampedSquares.Contains(4)) || (stampedSquares.Contains(5) && stampedSquares.Contains(6) && stampedSquares.Contains(7) && stampedSquares.Contains(8) && stampedSquares.Contains(9)) || (stampedSquares.Contains(10) && stampedSquares.Contains(11) && stampedSquares.Contains(12) && stampedSquares.Contains(13) && stampedSquares.Contains(14)) || (stampedSquares.Contains(15) && stampedSquares.Contains(16) && stampedSquares.Contains(17) && stampedSquares.Contains(18) && stampedSquares.Contains(19)) || (stampedSquares.Contains(20) && stampedSquares.Contains(21) && stampedSquares.Contains(22) && stampedSquares.Contains(23) && stampedSquares.Contains(24)))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void morseCode()
    {
        //pre-flashy flash flashes for morse
        index = UnityEngine.Random.Range(0,numberOfStamps);
        index = stampedStamps[index];
        if (!stampedSquares.Contains(index))
        {
            DebugMsg("The letter in morse code is " + chart1[index] + ".");
            correctSquare = index;
            ballRenderer.material = morseMats[0];
            StartCoroutine(LightBlinker(morseLetters[index]));
        }
        else
        {
            morseCode();
        }
    }

    IEnumerator LightBlinker(string morse)
    {
        //flashy flash flashes for morse
        while (true)
        {
            yield return new WaitForSeconds(.75f);
            for (int i = 0; i < morse.Length; i++)
            {
                ballRenderer.material = morseMats[1];
                if (morse[i] == '-')
                {
                    yield return new WaitForSeconds(.75f);
                }
                else
                {
                    yield return new WaitForSeconds(.25f);
                }
                ballRenderer.material = morseMats[0];
                yield return new WaitForSeconds(.25f);
            }
        }
    }

    void tapCode()
    {
        //pre-tapping sounds
        index = UnityEngine.Random.Range(0, stampedStamps.Count);
        index = stampedStamps[index];
        if (!stampedSquares.Contains(index))
        {
            DebugMsg("The letter in tap code is " + chart1[index] + ".");
            correctSquare = index;
        }
        else
        {
            tapCode();
        }
    }

    IEnumerator TapSound(string tap)
    {
        //tapping sounds
        for (int i = 0; i < tap.Length; i++)
        {
            if (tap[i] == '.')
            {
                audio.PlaySoundAtTransform("Tap", transform);
                yield return new WaitForSeconds(.5f);
            }
            else
            {
                yield return new WaitForSeconds(.5f);
            }
        }
        somethingActive = false;
    }

    void picture()
    {
        //pictures is go here
        index = UnityEngine.Random.Range(0, stampedStamps.Count);
        index = stampedStamps[index];
        if (!stampedSquares.Contains(index))
        {
            if (encryptionIndex == 2)
            {
                DebugMsg("The letter in Maritime Flags is " + chart1[index] + ".");
                ballRenderer.material = maritimeMats[index];
                correctSquare = index;
            }
            else if(encryptionIndex == 3)
            {
                DebugMsg("The letter in Semaphore is " + chart1[index] + ".");
                ballRenderer.material = semaphoreMats[index];
                correctSquare = index;
            }
            else if (encryptionIndex == 4)
            {
                DebugMsg("The letter in Pigpen is " + chart1[index] + ".");
                ballRenderer.material = pigpenMats[index];
                correctSquare = index;
            }
            else if (encryptionIndex == 5)
            {
                DebugMsg("The letter in Lombax is " + chart2[index] + ".");
                ballRenderer.material = lombaxMats[index];
                correctSquare = index;
            }
            else if (encryptionIndex == 6)
            {
                DebugMsg("The letter in Braille is " + chart2[index] + ".");
                ballRenderer.material = brailleMats[index];
                correctSquare = index;
            }
            else if (encryptionIndex == 7)
            {
                DebugMsg("The letter in Wingdings is " + chart2[index] + ".");
                ballRenderer.material = wingMats[index];
                correctSquare = index;
            }
            else if (encryptionIndex == 8)
            {
                DebugMsg("The letter in Zoni is " + chart2[index] + ".");
                ballRenderer.material = zoniMats[index];
                correctSquare = index;
            }
            else if (encryptionIndex == 9)
            {
                DebugMsg("The letter in Galactic is " + chart2[index] + ".");
                ballRenderer.material = galacticMats[index];
                correctSquare = index;
            }
            else if (encryptionIndex == 16)
            {
                DebugMsg("The letter in New York Point is " + chart3[index] + ".");
                ballRenderer.material = nypMats[index];
                correctSquare = index;
            }
            else
            {
                DebugMsg("The ASCII Hex Code is referring to the letter " + chart3[index] + ".");
                ballRenderer.material = asciiMats[index];
                correctSquare = index;
            }
        }
        else
        {
            picture();
        }
    }

    void arrows()
    {
        if (index == 0)
        {
            arrowVert--;
        }
        else if (index == 1)
        {
            arrowVert--;
            arrowHoriz++;
        }
        else if (index == 2)
        {
            arrowHoriz++;
        }
        else if (index == 3)
        {
            arrowHoriz++;
            arrowVert++;
        }
        else if (index == 4)
        {
            arrowVert++;
        }
        else if (index == 5)
        {
            arrowVert++;
            arrowHoriz--;
        }
        else if (index == 6)
        {
            arrowHoriz--;
        }
        else
        {
            arrowHoriz--;
            arrowVert--;
        }
        if(arrowVert < 0)
        {
            arrowVert = arrowVert + 5;
        }
        if(arrowHoriz < 0)
        {
            arrowHoriz = arrowHoriz + 5;
        }
        arrowVert = arrowVert % 5;
        arrowHoriz = arrowHoriz % 5;
        if (!stampedSquares.Contains(arrowHoriz + arrowVert * 5))
        {
            correctSquare = arrowHoriz + arrowVert * 5;
            DebugMsg("The button the arrow is pointing to is " + buttons[correctSquare].name + ".");
        }
        else
        {
            arrows();
        }
    }

    void listening()
    {
        ballRenderer.material = listeningMat;
        index = UnityEngine.Random.Range(0, stampedStamps.Count);
        index = stampedStamps[index];
        if (!stampedSquares.Contains(index))
        {
            DebugMsg("The sound in Listening is " + listeningNames[index] + ".");
            correctSquare = index;
        }
        else
        {
            listening();
        }
    }

    IEnumerator ListenSound()
    {
        //sounds
        audio.PlaySoundAtTransform(listeningNames[index], transform);
        yield return new WaitForSeconds(listeningClips[index].length);
        somethingActive = false;
    }

    void numbers()
    {
        index = UnityEngine.Random.Range(1, 25);
        if (!stampedSquares.Contains(stampedSquares[stampedSquares.Count - 1] + index) && stampedSquares[stampedSquares.Count - 1] + index < 25)
        {
            correctSquare = stampedSquares[stampedSquares.Count - 1] + index;
            ballRenderer.material = numberMats[index - 1 + 24 * UnityEngine.Random.Range(0, 5)];
            DebugMsg("The button the number is telling you to press is " + buttons[correctSquare].name + ".");
        }
        else
        {
            numbers();
        }
    }

    void chineseNumbers()
    {
        index = UnityEngine.Random.Range(0, stampedStamps.Count);
        index = stampedStamps[index];
        ballRenderer.material = chineseNumberMats[index];
        if (!stampedSquares.Contains(index))
        {
            correctSquare = index;
            DebugMsg("The button you need to press for Chinese Numbers is " + buttons[correctSquare].name + ".");
        }
        else
        {
            chineseNumbers();
        }
    }

    void cube()
    {
        index = UnityEngine.Random.Range(0, stampedStamps.Count);
        index = stampedStamps[index];
        if (!stampedSquares.Contains(index) && !cubeNotLetters.Contains(chart3[index]))
        {
            correctSquare = index;
            ballRenderer.material = cubeMats[index];
            DebugMsg("The letter in Cube Symbols is " + chart3[index] + ".");
        }
        else
        {
            cube();
        }
    }

    void runes()
    {
        index = UnityEngine.Random.Range(0, stampedStamps.Count);
        index = stampedStamps[index];
        if (!stampedSquares.Contains(index) && !runeNotLetters.Contains(chart3[index]))
        {
            correctSquare = index;
            ballRenderer.material = runeMats[correctSquare];
            DebugMsg("The letter in Runes is " + chart3[index] + ".");
        }
        else
        {
            runes();
        }
    }

    void fontana()
    {
        index = UnityEngine.Random.Range(0, stampedStamps.Count);
        index = stampedStamps[index];
        if (!stampedSquares.Contains(index) && !fontanaNotLetters.Contains(chart3[index]))
        {
            correctSquare = index;
            ballRenderer.material = fontanaMats[index];
            DebugMsg("The letter in Fontana is " + chart3[index] + ".");
        }
        else
        {
            fontana();
        }
    }

#pragma warning disable 414
    //private readonly string TwitchHelpMessage = @"!{0} up [Presses the up arrow button] | !{0} right [Presses the right arrow button] | !{0} down [Presses the down arrow button once] | !{0} left [Presses the left arrow button once] | !{0} left right down up [Chain button presses] | !{0} reset [Resets the module back to the start] | Movement words can be substituted as one letter (Ex. right as r)";
    private readonly string TwitchHelpMessage = @"!{0} D3 presses button D3. !{0} ball presses the ball. !{0} zoom is a general TP command that lets you see the encryptions better.";
#pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string cmd)
    {
        var parts = cmd.ToLowerInvariant().Split(new[] { ' ' });

        if (parts.Length == 1)
        {
            if (parts[0].Equals("ball"))
            {
                yield return null;
                yield return new KMSelectable[] { buttons[25] };
            }
            else
            {
                for(int i = 0; i < 25; i++)
                {
                    if(buttons[i].name.ToLower().Equals(parts[0].ToLower()))
                    {
                        yield return null;
                        yield return new KMSelectable[] { buttons[i] };
                        i = 25;
                    }
                    else if(i == 24)
                    {
                        yield break;
                    }
                }
            }
        }
        else
        {
            yield break;
        }
    }

    void DebugMsg(string msg)
    {
        Debug.LogFormat("[Encryption Bingo #{0}] {1}", ModuleId, msg);
    }
}
