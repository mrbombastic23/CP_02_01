using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class Enemy : MonoBehaviour
{
    public string letter;
    public string word;

    public TextMeshProUGUI letterText;
    public TextMeshProUGUI wordText;

    private Transform player;
    private float speed = 0.1f;

    public void Init(string letter, string word, Transform player)
    {
        this.letter = letter;
        this.word = word;
        this.player = player;

        letterText.text = letter;
        wordText.text = word;

        gameObject.tag = "Enemy";
    }


    private void Update()
    {
        if (player == null) return;

        transform.position = Vector2.MoveTowards(transform.position, player.position, speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, player.position) < 0.3f)
        {
            GameManager.Instance.EnemyReachedPlayer();
            Destroy(gameObject);
        }
    }
}
