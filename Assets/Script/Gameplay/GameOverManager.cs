using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;

public class GameOverManager : MonoBehaviour
{
    [Header("Game Over UI")]
    public CanvasGroup gameOverOverlay;     // Nền đen
    public Image gameOverImage;             // Ảnh GAME OVER
    public GameObject scorePanelRoot;       // Panel bên trái
    public GameObject leaderboardPanelRoot; // Panel bên phải
    public Text currentScoreText;           // Điểm hiện tại
    public GameObject bubblesParticlePrefab;

    [Header("Leaderboard")]
    public RankingEntry[] rankingEntries;   // 5 entries (1st-5th)

    [System.Serializable]
    public class RankingEntry
    {
        public GameObject root;
        public Image background;
        public Text rankText;
        public Text nameText;
        public Text scoreText;
    }

    private Leaderboard leaderboard;

    private void Awake()
    {
        leaderboard = new Leaderboard();
        
        // Ẩn game over UI ban đầu
        if (gameOverOverlay != null)
        {
            gameOverOverlay.alpha = 0;
            gameOverOverlay.gameObject.SetActive(false);
        }
    }

    public void ShowGameOver(int finalScore)
    {
        Debug.Log($"ShowGameOver called with score: {finalScore}");
        
        // DISABLE DROPPER (không cho thả nữa)
        DropperController dropper = FindObjectOfType<DropperController>();
        if (dropper != null)
        {
            dropper.enabled = false; // Tắt script
        }

        // Spawn particle
        if (bubblesParticlePrefab != null)
        {
            GameObject particle = Instantiate(bubblesParticlePrefab);
            particle.transform.position = new Vector3(0, -8.7f, -1);
        }

        // Hiện overlay với nền ĐEN
        if (gameOverOverlay != null)
        {
            gameOverOverlay.gameObject.SetActive(true);
            gameOverOverlay.alpha = 0;
            
            // Fade to alpha = 125/255 = 0.49
            gameOverOverlay.DOFade(0.49f, 0.5f);
            
            // Đảm bảo overlay block input
            gameOverOverlay.blocksRaycasts = true;
            gameOverOverlay.interactable = true;
        }

        // Animation GAME OVER image
        if (gameOverImage != null)
        {
            gameOverImage.transform.position = Vector3.zero;
            gameOverImage.transform.localScale = Vector3.one;
            
            // Thu nhỏ + di chuyển
            Sequence gameOverSeq = DOTween.Sequence();
            gameOverSeq.Append(gameOverImage.transform.DOScale(0.8f, 0.3f));
            gameOverSeq.Join(gameOverImage.transform.DOMove(new Vector3(-2.5f, 2.5f, 0), 0.3f));
            
            gameOverSeq.OnComplete(() =>
            {
                // Hiện score panel
                ShowScorePanel(finalScore);
                
                // Hiện leaderboard
                ShowLeaderboard(finalScore);
            });
        }
    }

    private void ShowScorePanel(int score)
    {
        if (scorePanelRoot != null)
        {
            scorePanelRoot.SetActive(true);
            CanvasGroup cg = scorePanelRoot.GetComponent<CanvasGroup>();
            if (cg == null) cg = scorePanelRoot.AddComponent<CanvasGroup>();
            
            cg.alpha = 0;
            cg.DOFade(1, 0.5f);
        }

        if (currentScoreText != null)
        {
            currentScoreText.text = score.ToString();
        }
    }

    private void ShowLeaderboard(int newScore)
    {
        if (leaderboardPanelRoot != null)
        {
            leaderboardPanelRoot.SetActive(true);
        }

        // Load leaderboard
        leaderboard.LoadFromPlayerPrefs();
        
        // Thêm điểm mới
        int newRank = leaderboard.AddScore("Player", newScore);
        
        // Lưu lại
        leaderboard.SaveToPlayerPrefs();

        // Hiển thị với animation
        if (newRank >= 0)
        {
            ShowLeaderboardWithAnimation(newRank);
        }
        else
        {
            ShowLeaderboardNormal();
        }
    }

    private void ShowLeaderboardNormal()
    {
        // Hiện tất cả entries không animation
        for (int i = 0; i < rankingEntries.Length && i < leaderboard.entries.Count; i++)
        {
            UpdateRankingEntry(i, leaderboard.entries[i]);
            
            if (rankingEntries[i].root != null)
            {
                rankingEntries[i].root.SetActive(true);
            }
        }
    }

    private void ShowLeaderboardWithAnimation(int newRank)
    {
        Sequence seq = DOTween.Sequence();

        // Fade out các hàng cần update (từ newRank đến cuối)
        for (int i = newRank; i < rankingEntries.Length; i++)
        {
            if (rankingEntries[i].root != null)
            {
                CanvasGroup cg = rankingEntries[i].root.GetComponent<CanvasGroup>();
                if (cg == null) cg = rankingEntries[i].root.AddComponent<CanvasGroup>();
                
                seq.Append(cg.DOFade(0, 0.2f));
            }
        }

        // Update data
        seq.AppendCallback(() =>
        {
            for (int i = 0; i < rankingEntries.Length && i < leaderboard.entries.Count; i++)
            {
                UpdateRankingEntry(i, leaderboard.entries[i]);
            }
        });

        // Fade in các hàng (trừ hàng mới)
        for (int i = newRank; i < rankingEntries.Length; i++)
        {
            if (i == newRank) continue; // Bỏ qua hàng mới

            if (rankingEntries[i].root != null)
            {
                CanvasGroup cg = rankingEntries[i].root.GetComponent<CanvasGroup>();
                seq.Append(cg.DOFade(1, 0.2f));
            }
        }

        // Hiện hàng mới với highlight
        seq.AppendCallback(() =>
        {
            HighlightNewRank(newRank);
        });
    }

    private void HighlightNewRank(int rank)
    {
        if (rank < 0 || rank >= rankingEntries.Length) return;

        RankingEntry entry = rankingEntries[rank];
        
        // Fade in
        CanvasGroup cg = entry.root.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = 0;
            cg.DOFade(1, 0.3f);
        }

        // Flash background (chỉ nếu có background)
        if (entry.background != null)
        {
            Color originalColor = entry.background.color;
            Color brightColor = originalColor * 2f;
            
            entry.background.color = brightColor;
            entry.background.DOColor(originalColor, 1f).SetEase(Ease.OutQuad);
        }
        else
        {
            // Nếu không có background, flash text thay thế
            if (entry.nameText != null)
            {
                Color originalColor = entry.nameText.color;
                entry.nameText.color = Color.yellow;
                entry.nameText.DOColor(originalColor, 1f);
            }
        }
    }

    private void UpdateRankingEntry(int index, Leaderboard.LeaderboardEntry data)
    {
        if (index >= rankingEntries.Length) return;

        RankingEntry entry = rankingEntries[index];
        
        if (entry.rankText != null)
        {
            string[] ranks = { "1st", "2nd", "3rd", "4th", "5th" };
            entry.rankText.text = index < ranks.Length ? ranks[index] : $"{index + 1}th";
        }

        if (entry.nameText != null)
        {
            entry.nameText.text = data.playerName;
        }

        if (entry.scoreText != null)
        {
            entry.scoreText.text = data.score.ToString();
        }
    }
}

// Leaderboard data structure
public class Leaderboard
{
    [System.Serializable]
    public class LeaderboardEntry
    {
        public string playerName;
        public int score;

        public LeaderboardEntry(string name, int score)
        {
            this.playerName = name;
            this.score = score;
        }
    }

    public List<LeaderboardEntry> entries = new List<LeaderboardEntry>();

    public void LoadFromPlayerPrefs()
    {
        entries.Clear();
        
        for (int i = 0; i < 5; i++)
        {
            string name = PlayerPrefs.GetString($"Rank{i}_Name", "---");
            int score = PlayerPrefs.GetInt($"Rank{i}_Score", 0);
            
            entries.Add(new LeaderboardEntry(name, score));
        }
    }

    public void SaveToPlayerPrefs()
    {
        for (int i = 0; i < entries.Count && i < 5; i++)
        {
            PlayerPrefs.SetString($"Rank{i}_Name", entries[i].playerName);
            PlayerPrefs.SetInt($"Rank{i}_Score", entries[i].score);
        }
        PlayerPrefs.Save();
    }

    // Thêm score mới, trả về rank (-1 nếu không lọt top 5)
    public int AddScore(string playerName, int score)
    {
        // Tìm vị trí insert
        int insertIndex = -1;
        
        for (int i = 0; i < entries.Count; i++)
        {
            if (score > entries[i].score)
            {
                insertIndex = i;
                break;
            }
        }

        // Không lọt top 5
        if (insertIndex == -1 && entries.Count >= 5)
        {
            return -1;
        }

        // Insert vào vị trí phù hợp
        if (insertIndex >= 0)
        {
            entries.Insert(insertIndex, new LeaderboardEntry(playerName, score));
        }
        else
        {
            entries.Add(new LeaderboardEntry(playerName, score));
        }

        // Giữ tối đa 5 entries
        while (entries.Count > 5)
        {
            entries.RemoveAt(entries.Count - 1);
        }

        return insertIndex >= 0 ? insertIndex : entries.Count - 1;
    }
}