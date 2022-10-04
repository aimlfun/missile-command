using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MissileDefence.Controllers;

/// <summary>
/// Object to track a store with the initials of the person who achieved it.
/// </summary>
internal class HighScore
{
    /// <summary>
    /// The score for this "high score"
    /// </summary>
    internal int Score;
    
    /// <summary>
    /// Who scored this "high score".
    /// </summary>
    internal string Initials;

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="score"></param>
    /// <param name="initials"></param>
    internal HighScore(int score, string initials)
    {
        Score = score;
        Initials = initials;
    }
}

/// <summary>
/// Simple tracker of high-scores.
/// </summary>
internal class HighScoreManager
{
    /// <summary>
    /// The highest score in the highscore table
    /// </summary>
    internal int TopScore = 0;

    /// <summary>
    /// Contains a list of scores.
    /// </summary>
    internal List<HighScore> hiscores = new();

    /// <summary>
    /// This is where the scores are saved to / loaded from.
    /// </summary>
    private readonly string highScoreFilePath = Path.Combine(Program.aiFolder, $"high-scores.txt");

    /// <summary>
    /// Constructor. Loads the scores.
    /// </summary>
    internal HighScoreManager()
    {
        Load();
    }

    /// <summary>
    /// Loads the high scores from disk.
    /// </summary>
    internal void Load()
    {
        hiscores.Clear();

        // if we have high scores, load them
        if (File.Exists(highScoreFilePath))
        {
            // load hiscores
            string[] lines = File.ReadAllLines(highScoreFilePath);

            foreach (string line in lines)
            {
                string[] tokens = line.Split(",");

                hiscores.Add(new(int.Parse(tokens[0]), tokens[1]));
            }
        }
        else
        {
            // we don't have hiscores, so use the original defaults
            hiscores.Add(new(29952, "DFT"));
            hiscores.Add(new(29845, "DLS"));
            hiscores.Add(new(29488, "SRC"));
            hiscores.Add(new(29264, "RDA"));
            hiscores.Add(new(29184, "MJP"));
            hiscores.Add(new(29008, "JED"));
            hiscores.Add(new(28677, "DEW"));
            hiscores.Add(new(26960, "GJL"));

            /*
             ; Default high scores, from lowest to highest.  Initials are ASCII, scores are
             ; 3-byte BCD.  For example, the highest default score is DFT with 7500 points.
             ; 
             ; cf. https://arcadeblogger.com/2021/01/31/anatomy-of-arcade-high-score-tables/
             ; 
             :def_score_names
            74f3: 47 4a 4c 44+                 .dstr   ‘GJLDEWJEDMJPRDASRCDLSDFT’
            750b: 50 69 00     :def_scores     .dd3    $006950           ;GJL - Gerry Lichac
            750e: 05 70 00                     .dd3    $007005           ;DEW - Dave Wiebenson
            7511: 50 71 00                     .dd3    $007150           ;JED - Jed Margolin
            7514: 00 72 00                     .dd3    $007200           ;MJP - Mary Pepper
            7517: 50 72 00                     .dd3    $007250           ;RDA - Rich D. Adam
            751a: 30 73 00                     .dd3    $007330           ;SRC - Steve Calfee
            751d: 95 74 00                     .dd3    $007495           ;DLS - Dave Sherman
            7520: 00 75 00                     .dd3    $007500           ;DFT - Dave F. Theurer
            */
        }

        TopScore = hiscores.ToArray()[0].Score; // highest score
        Save();
    }

    /// <summary>
    /// Saves the high-scores to disk.
    /// </summary>
    internal void Save()
    {
        StringBuilder sb = new();

        // write score,name 
        foreach (HighScore i in hiscores)
        {
            sb.AppendLine($"{i.Score},{i.Initials}");
        }

        File.WriteAllText(highScoreFilePath, sb.ToString());
    }

    /// <summary>
    /// Add to hiscore table.
    /// </summary>
    /// <param name="score"></param>
    /// <param name="initials"></param>
    internal void Add(int score, string initials)
    {
        bool found = false;

        // only allow ONE "cpu" entry, otherwise the user will unlikely
        // to have any entries given the CPU takes a long time to lose
        // despite noe xtra cities.
        if (initials == "CPU")
        {
            foreach (HighScore i in hiscores)
            {
                if (i.Initials == "CPU")
                {
                    found = true;
                    i.Score = score;
                }
            }
        }

        // we need to add this score
        if (!found) hiscores.Add(new(score, initials));

        // put them into order (highest) top. The alternative is for the "Add()"
        // to insert in the correct place, which is more messy.
        List<HighScore> newHighScores = hiscores.OrderByDescending(o => o.Score).ToList();

        // no more than 8 high scores
        while (newHighScores.Count > 8)
        {
            newHighScores.RemoveAt(8);
        }

        // swap the last scores with the new ones.
        hiscores = newHighScores;

        // find out the top one (it may have changed)
        TopScore = hiscores[0].Score; // highest score

        // persist to disk, so each time it runs we have them
        Save();
    }

    /// <summary>
    /// Is the players score eligible to go on the highscore table?
    /// </summary>
    /// <param name="player"></param>
    /// <returns></returns>
    internal bool IsHighScore(Player player)
    {
        return (player.score > hiscores[^1].Score);
    }
}
