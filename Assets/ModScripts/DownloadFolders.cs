using System;
using System.Collections.Generic;
using System.Linq;

public class Folder
{
    public string FolderName { get; private set; }
    public int[] Directories { get; private set; }


    public Folder(string folderName, int[] directories)
    {
        FolderName = folderName;
        Directories = directories;
    }
}

public class ObtainUsername
{
    private readonly Folder[] usedFolders, allFolders;
    private readonly Folder startingFolder;

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

    public string GetUsername(bool firstCond, bool secondCond)
    {
        var idxes = usedFolders.Select(x => Array.IndexOf(allFolders, x)).ToArray();

        var startingIx = Array.IndexOf(allFolders, startingFolder);

        var completedUsername = new List<string>();

        if (secondCond)
        {
            completedUsername.Add(tableA[idxes[firstCond ? 0 : 1], startingIx]);
            completedUsername.Add(tableB[idxes[firstCond ? 1 : 0], startingIx]);
        }
        else
        {
            completedUsername.Add(tableA[startingIx, idxes[firstCond ? 0 : 1]]);
            completedUsername.Add(tableB[startingIx, idxes[firstCond ? 1 : 0]]);
        }


        return completedUsername.Join("");
    }
}