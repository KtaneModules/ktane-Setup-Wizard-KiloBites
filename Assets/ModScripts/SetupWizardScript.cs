using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using static UnityEngine.Random;
using static UnityEngine.Debug;

public class SetupWizardScript : MonoBehaviour {

	public KMBombInfo Bomb;
	public KMAudio Audio;
	public KMBombModule Module;

	public KMSelectable[] mainButtons, keyboardLetters, keyboardNumbers, folderButtons, accountPrompts, page2Buttons;
	public KMSelectable backSpace, shift, reset, submit;

	public GameObject window;
	public GameObject[] pages;

	public Material[] blackScreens, backgrounds, windowIcons;
	public MeshRenderer windowIcon, screen;

	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved, isActivated, moduleSelected, canTypeUser, canTypePassword, shiftedLetters = true, canSubmit, canSolve;

	private Folder[] folders =
	{
		new Folder("Downloads", new int[] { 1, 4 }),
		new Folder("Pictures", new int[] { 3, 5 }),
		new Folder("Documents", new int[] { 0, 1 }),
		new Folder("Music", new int[] { 0, 5 }),
		new Folder("Homework", null, 2),
		new Folder("Videos", new int[] { 2, 4 })
	};

	private Folder startingFolder;
	private Folder[] shuffledFolders;

	private ObtainUsername username;

	private Expression[] generatedPuzzle, modifiedPuzzle;
	private EquationSystem equationSystem;

	private int currentPos;

	private string usernameInput, passwordInput, passwordAsterisk;

	private int[] randomIxes = new int[6], passwordDigits = new int[6];

	private string FinalPassword(string s, int count) => s.Remove(count) + s.Substring(s.Length - count, count);

	private Folder[] SwapFolders(int[] swaps)
	{
		var foldersSwapped = new Folder[2];

		var foldersToSwap = swaps.Select(x => folders[x].FolderName).ToArray();
		var foldersDirectories = swaps.Select(x => folders[x].Directories).ToArray();
		var foldersSingleDirectory = swaps.Select(x => folders[x].SingleDirectory).ToArray();

		int ix = 0;

		while (ix < 2)
		{
			if (foldersDirectories[ix] == null)
			{
				if (ix == 0)
				{
					var temp = foldersToSwap[1];

					foldersSwapped[ix] = new Folder(temp, null, foldersSingleDirectory[ix]);
				}
				else
				{
					var temp = foldersToSwap[0];

					foldersSwapped[ix] = new Folder(temp, null, foldersSingleDirectory[ix]);
				}
				ix++;
				continue;
			}

			if (ix == 0)
			{
				var temp = foldersToSwap[1];

				foldersSwapped[ix] = new Folder(temp, foldersDirectories[ix]);
			}
			else
			{
				var temp = foldersToSwap[0];

				foldersSwapped[ix] = new Folder(temp, foldersDirectories[ix]);
			}

			ix++;
		}


		return foldersSwapped;
	}

	private Expression[] SwapAnswers(int[] swaps)
	{
		var swappedAnswers = new Expression[2];

		var answersToSwap = swaps.Select(x => generatedPuzzle[x].Answer).ToArray();
		var expressionALetters = swaps.Select(x => generatedPuzzle[x].NumIxA).ToArray();
		var expressionEquationExp = swaps.Select(x => generatedPuzzle[x].EquationExpression).ToArray();
		var expressionBLetters = swaps.Select(x => generatedPuzzle[x].NumIxB).ToArray();

		for (int i = 0; i < 2; i++)
			swappedAnswers[i] = new Expression(expressionALetters[i], expressionEquationExp[i], expressionBLetters[i], answersToSwap[i == 0 ? 1 : 0]);

        return swappedAnswers;
	}

	private int GetLetterIndex(char c) => "QWERTYUIOPASDFGHJKLZXCVBNM".IndexOf(c);
	private int GetNumberIndex(char n) => "1234567890".IndexOf(n);

    void Awake()
    {

		moduleId = moduleIdCounter++;

		foreach (KMSelectable letter in keyboardLetters)
			letter.OnInteract += delegate () { KeyboardLetterPress(letter); return false; };

		foreach (KMSelectable number in keyboardNumbers)
			number.OnInteract += delegate () { KeyboardNumberPress(number); return false; };

		Module.OnActivate += delegate () { StopAllCoroutines(); StartCoroutine(Startup()); };
		Module.GetComponent<KMSelectable>().OnFocus += delegate { moduleSelected = true; };
		Module.GetComponent<KMSelectable>().OnDefocus += delegate { moduleSelected = false; };

    }

	
	void Start()
    {
		foreach (var obj in pages)
			obj.SetActive(false);

		submit.gameObject.SetActive(false);

		window.SetActive(false);

		StartCoroutine(Initialize());


		currentPos = Range(0, 6);

		shuffledFolders = folders;

		var foldersToShuffle = Enumerable.Range(0, 6).Where(x => x != currentPos).ToList().Shuffle().Take(2).ToArray();
		
		var swappedFolders = SwapFolders(foldersToShuffle);

		for (int i = 0; i < 2; i++)
			shuffledFolders[foldersToShuffle[i]] = swappedFolders[i];

		username = new ObtainUsername(foldersToShuffle.Select(x => folders[x]).ToArray(), folders, folders[currentPos]);

		Log($"[Setup Wizard #{moduleId}] The swapped folders for step 1 were: {foldersToShuffle.Select(x => folders[x].FolderName).Join(", ")}");


    }

	void KeyboardLetterPress(KMSelectable letter)
	{
		if (moduleSolved || !isActivated || !canSubmit && !canTypeUser)
			return;

		if (usernameInput.Length < 10)
		{
            usernameInput += letter.GetComponentInChildren<TextMesh>().text;
        }
			
	}

	void KeyboardNumberPress(KMSelectable number)
	{
		if (moduleSolved || !isActivated || !canSubmit && !canTypePassword)
			return;

		if (passwordInput.Length < 6)
		{
            passwordInput += number.GetComponentInChildren<TextMesh>().text;
			passwordAsterisk += '*';
        }
			
	}

	void ShiftPress()
	{
		if (moduleSolved || !isActivated || !canSubmit)
			return;

		shiftedLetters = !shiftedLetters;

		var letters = "QWERTYYUIOPASDFGHJKLZXCVBNM";

		for (int i = 0; i < 26; i++)
			keyboardLetters[i].GetComponentInChildren<TextMesh>().text = shiftedLetters ? letters[i].ToString().ToUpperInvariant() : letters[i].ToString().ToLowerInvariant();
	}

	void ResetPress()
	{

	}

	void BackSpacePress()
	{
		if (moduleSolved || !isActivated || !canSubmit)
			return;

		if (canTypeUser)
			if (usernameInput.Length > 0)
			{
				usernameInput.Remove(usernameInput.Length - 1);
			}
		
		else if (canTypePassword)
				if (passwordInput.Length > 0)
				{
					passwordInput.Remove(passwordInput.Length - 1);
					passwordAsterisk.Remove(passwordAsterisk.Length - 1);
				}
	}

	IEnumerator Startup()
	{
		Audio.PlaySoundAtTransform("Window Setup", transform);
		screen.material = backgrounds.PickRandom();

		yield return new WaitForSeconds(1);

		window.SetActive(true);
		pages[0].SetActive(true);
		isActivated = true;
	}

	IEnumerator Initialize()
	{
		yield return new WaitForSeconds(0.5f);
		screen.material = blackScreens[1];

	}
	
	
	void Update()
    {
		if (moduleSolved || !isActivated)
			return;

		if (moduleSelected && canTypeUser || canTypePassword)
		{
			for (int ltr = 0; ltr < 26; ltr++)
				if (Input.GetKeyDown(((char)('a' + ltr)).ToString()))
				{
					keyboardLetters[GetLetterIndex((char)('A' + ltr))].OnInteract();
					return;
				}

			var validNumPresses = new[] { KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4, KeyCode.Alpha5, KeyCode.Alpha6, KeyCode.Alpha7, KeyCode.Alpha8, KeyCode.Alpha9, KeyCode.Alpha0 };

			for (int num = 0; num < validNumPresses.Length; num++)
				if (Input.GetKeyDown(validNumPresses[num]))
				{
					keyboardNumbers[int.Parse("1234567890"[num].ToString())].OnInteract();
					return;
				}

			if (Input.GetKeyDown(KeyCode.Backspace))
				backSpace.OnInteract();


		}

    }

	// Twitch Plays


#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} something";
#pragma warning restore 414

	IEnumerator ProcessTwitchCommand (string command)
    {
		string[] split = command.ToUpperInvariant().Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries);
		yield return null;
    }

	IEnumerator TwitchHandleForcedSolve()
    {
		yield return null;
    }


}





