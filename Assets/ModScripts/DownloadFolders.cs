using System;
using System.Collections.Generic;
using System.Linq;
using KModkit;

public class Folder
{
    public string FolderName { get; private set; }
    public int[] Directories { get; private set; }
    public int? SingleDirectory {  get; private set; }


    public Folder(string folderName, int[] directories, int? singleDirectory = null)
    {
        FolderName = folderName;
        Directories = directories;
        SingleDirectory = singleDirectory;
    }
}

public class ObtainUsername
{
    private Folder[] usedFolders, allFolders;
    private Folder startingFolder;

    public ObtainUsername(Folder[] usedFolders, Folder[] allFolders, Folder startingFolder)
    {
        this.usedFolders = usedFolders;
        this.allFolders = allFolders;
        this.startingFolder = startingFolder;
    }

    private static readonly string[,] tableA =
    {
        { "Moist", "Awesome", "Random", "Mega", "Lord", "Scary" },
        { "Rainbow", "Based", "Colonel", "Crazy", "Red", "Powerful" },
        { "Tainted", "Not", "Doctor", "Funky", "Holy", "Creepy" },
        { "Cringe", "Unhinged", "Epic", "Creamy", "Mister", "Sloppy" },
        { "Cool", "Wacky", "Super", "King", "Grunkle", "French" },
        { "Secret", "Broken", "Robo", "Average", "Itz", "Funny" }
    }, tableB =
    {
        { "Cake", "Potato", "Balls", "Killer", "Meister", "Thing" },
        { "Bomb", "Orb", "Agony", "Blan", "Ish", "Cucumber" },
        { "Dude", "Master", "Disease", "Gamer", "Kitten", "Foot" },
        { "Birthday", "Salad", "Expert", "Finger", "Troll", "Guy" },
        { "Licker", "Mommy", "Username", "Death", "Dongle", "Hole" },
        { "Monkey", "Mate", "Pie", "Tapeworm", "Berg", "Spork" }
    };

    public string GetUsername(KMBombInfo bomb)
    {
        var idxes = usedFolders.OrderBy(x => x.FolderName).Select(x => Array.IndexOf(allFolders, x)).ToArray();
        var startingIx = Array.IndexOf(allFolders, startingFolder);

        var completedUsername = new List<string>();
        var obtainIxes = ("SETUPWIZARD".Contains(bomb.GetSerialNumberLetters().First()) ? "01" : "10").Select(x => int.Parse(x.ToString())).ToArray();

        if ("COMPUTERLAB".Contains(bomb.GetSerialNumberLetters().Last()))
        {
            completedUsername.Add(tableA[idxes[obtainIxes[0]], startingIx]);
            completedUsername.Add(tableB[idxes[obtainIxes[1]], startingIx]);
        }
        else
        {
            completedUsername.Add(tableA[startingIx, idxes[obtainIxes[0]]]);
            completedUsername.Add(tableB[startingIx, idxes[obtainIxes[1]]]);
        }


        return completedUsername.Join("");
    }
}