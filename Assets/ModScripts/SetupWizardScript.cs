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

	public TextMesh[] mainTexts, expressionDisplays; 

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

	private int currentPos, currentPage = 0, page2Ix = 0;

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

		foreach (KMSelectable folder in folderButtons)
			folder.OnInteract += delegate () { FolderPress(folder); return false; };

		foreach (KMSelectable mainButton in mainButtons)
			mainButton.OnInteract += delegate () { MainButtonPress(mainButton); return false; };

		foreach (KMSelectable prompt in accountPrompts)
			prompt.OnInteract += delegate () { AccountPromptPress(prompt); return false; };



		reset.OnInteract += delegate () { ResetPress(); return false; };
		backSpace.OnInteract += delegate () { BackSpacePress(); return false; };
		shift.OnInteract += delegate () { ShiftPress(); return false; };
		submit.OnInteract += delegate () { SubmitPress(); return false; };


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
		Log($"[Setup Wizard #{moduleId}] The username should be {username.GetUsername(Bomb)}");

		passwordDigits = equationSystem.GeneratedPassword();

		var randomIxes = new int[6];

		for (int i = 0; i < 6; i++)
			randomIxes[i] = Range(0, 5);

		generatedPuzzle = equationSystem.GeneratedPuzzle(passwordDigits, randomIxes);
		modifiedPuzzle = generatedPuzzle;

		var answersToShuffle = Enumerable.Range(0, 6).ToList().Shuffle().Take(2).ToArray();

		var shuffledAnswers = SwapAnswers(answersToShuffle);

		for (int i = 0; i < 2; i++)
			modifiedPuzzle[answersToShuffle[i]] = shuffledAnswers[i];

    }

	void AccountPromptPress(KMSelectable prompt)
	{
		prompt.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("Click", transform);

		if (moduleSolved || !isActivated || !canSubmit)
			return;

		switch (Array.IndexOf(accountPrompts, prompt))
		{
			case 0:
				if (canTypeUser)
					return;

				canTypeUser = !canTypeUser;
				break;
			case 1:
				if (canTypePassword)
					return;

				canTypePassword = !canTypePassword;
				break;
		}
	}

	void MainButtonPress(KMSelectable button)
	{
		button.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("Click", transform);

		if (moduleSolved || !isActivated)
			return;

		switch (Array.IndexOf(mainButtons, button))
		{
			case 0:
				switch (currentPage)
				{
					case 0:
						return;
					default:
						currentPage--;
						break;
				}
				break;
			case 1:
				switch (currentPage)
				{
					default:
						currentPage++;
						break;
					case 0:
						FolderUpdate();
						goto default;
					case 1:

						goto default;
					case 3:
						if (usernameInput == username.GetUsername(Bomb) && passwordInput == passwordDigits.Join(""))
						{
							Log($"[Setup Wizard #{moduleId}] The username and password are correct. Moving over to the finish screen");
							currentPage++;
						}
						else
						{
							Log($"[Setup Wizard #{moduleId}] Either the username or password is invalid. Strike!");
							Module.HandleStrike();
						}
						break;
					case 4:
						currentPage++;
						canSolve = true;
						break;
					case 5:
						return;
				}
				break;
		}

		WindowUpdate();
	}

	void WindowUpdate()
	{
		var mainText = mainButtons[1].GetComponentInChildren<TextMesh>().text;

		mainButtons[1].GetComponentInChildren<TextMesh>().text = currentPos == 4 ? "Finish" : mainText;

		if (currentPage == 5)
		{
			foreach (KMSelectable mainButton in mainButtons)
				mainButton.gameObject.SetActive(false);
			reset.gameObject.SetActive(false);

			foreach (var obj in pages)
				obj.SetActive(false);

			windowIcon.material = windowIcons[1];
			submit.gameObject.SetActive(true);

			return;
		}

		

		for (int i = 0; i < pages.Length; i++)
			pages[i].SetActive(i == currentPage);
	}

	void FolderPress(KMSelectable folder)
	{
		folder.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("Click", transform);

		if (moduleSolved || !isActivated || currentPage != 1)
			return;

		var ix = Array.IndexOf(folderButtons, folder);

		currentPos = shuffledFolders[currentPos].Directories == null ? shuffledFolders[currentPos].SingleDirectory.Value : shuffledFolders[currentPos].Directories[ix];

		FolderUpdate();
	}

	void FolderUpdate()
	{

		if (shuffledFolders[currentPos].Directories == null)
		{
			folderButtons[1].gameObject.SetActive(false);
			folderButtons[0].GetComponentInChildren<TextMesh>().text = shuffledFolders[folders[currentPos].SingleDirectory.Value].FolderName;
		}
		else
		{
			folderButtons[1].gameObject.SetActive(true);

			var folderNames = shuffledFolders[currentPos].Directories.Select(x => folders[x].FolderName).ToArray();

			for (int i = 0; i < 2; i++)
				folderButtons[i].GetComponentInChildren<TextMesh>().text = folderNames[i];
		}

	}

	void Page2Update()
	{
		for (int i = 0; i < 4; i++)
		{
			var ix = i + page2Ix;
			expressionDisplays[i].text = $"{"a),b),c),d),e),f)".Split(',')[ix]} {modifiedPuzzle[ix].NumIxA} {modifiedPuzzle[ix].EquationExpression} {modifiedPuzzle[ix].NumIxB} = {modifiedPuzzle[ix].Answer}";
        }
			
	}


	void KeyboardLetterPress(KMSelectable letter)
	{
		letter.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("KeyPress", transform);

		if (moduleSolved || !isActivated || !canSubmit && !canTypeUser)
			return;

		if (usernameInput.Length < 10)
		{
            usernameInput += letter.GetComponentInChildren<TextMesh>().text;
			accountPrompts[0].GetComponentInChildren<TextMesh>().text = usernameInput;
        }
			
	}

	void KeyboardNumberPress(KMSelectable number)
	{
		number.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("KeyPress", transform);

		if (moduleSolved || !isActivated || !canSubmit && !canTypePassword)
			return;

		if (passwordInput.Length < 6)
		{
            passwordInput += number.GetComponentInChildren<TextMesh>().text;
			passwordAsterisk += '*';
			accountPrompts[1].GetComponentInChildren<TextMesh>().text = passwordAsterisk;
        }
			
	}

	void ShiftPress()
	{
		shift.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("KeyPress", transform);

		if (moduleSolved || !isActivated || !canSubmit)
			return;

		shiftedLetters = !shiftedLetters;

		var letters = "QWERTYYUIOPASDFGHJKLZXCVBNM";

		for (int i = 0; i < 26; i++)
			keyboardLetters[i].GetComponentInChildren<TextMesh>().text = shiftedLetters ? letters[i].ToString().ToUpperInvariant() : letters[i].ToString().ToLowerInvariant();
	}

	void SubmitPress()
	{
		submit.AddInteractionPunch(0.4f);
		Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);

		if (moduleSolved || !isActivated || !canSolve)
			return;

		Audio.PlaySoundAtTransform("Solve", transform);

		Log($"[Setup Wizard #{moduleSolved}] The button has been pressed. Solved!");

		moduleSolved = true;
		Module.HandlePass();
	}

	void ResetPress()
	{
		reset.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("Click", transform);

		if (moduleSolved || !isActivated || currentPage == 0)
			return;

		currentPage = 0;
		WindowUpdate();
	}

	void BackSpacePress()
	{
		backSpace.AddInteractionPunch(0.4f);
		Audio.PlaySoundAtTransform("KeyPress", transform);

		if (moduleSolved || !isActivated || !canSubmit)
			return;

		if (canTypeUser)
			if (usernameInput.Length > 0)
			{
				usernameInput.Remove(usernameInput.Length - 1);
                accountPrompts[0].GetComponentInChildren<TextMesh>().text = passwordAsterisk;
            }
		
		else if (canTypePassword)
				if (passwordInput.Length > 0)
				{
					passwordInput.Remove(passwordInput.Length - 1);
					passwordAsterisk.Remove(passwordAsterisk.Length - 1);
                    accountPrompts[1].GetComponentInChildren<TextMesh>().text = passwordAsterisk;
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





