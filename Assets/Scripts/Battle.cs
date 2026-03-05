using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Battle
{
  public List<Enemy> Enemies { get; private set; }

  public CardStack CStack { get; private set; }

  public int Energy { get; private set; }

  public enum States
  {
    Main,
    Win,
    Lose,
  }

  public enum Turns
  {
    Player,
    Enemy,
  }

  public States State { get; private set; }

  public Turns Turn { get; private set; }

  public List<Card> Hand { get; private set; }

  public Character MainCharacter { get; private set; }

  public Card SpecialCard { get; private set; }

  public Battle(Character mainCharacter, EnemySource[] enemySources, int minEnemyNumber, int maxEnemyNumber, Deck deck)
  {
    State = States.Main;
    MainCharacter = mainCharacter;
    Hand = new List<Card>();
    Enemies = new List<Enemy>();
    int enemyNumber = Random.Range(minEnemyNumber, maxEnemyNumber + 1);
    for (int i = 0; i < enemyNumber; ++i)
    {
      EnemySource enemySource = enemySources[Random.Range(0, enemySources.Length)];
      Enemy enemy = new Enemy(enemySource);
      Enemies.Add(enemy);
    }

    CStack = new CardStack(deck);
    CStack.Shuffle();

    Energy = 0;

    SetTurn(Turns.Player);

    AudioManager.Instance.PlayBgm("battle_intro");
    AudioManager.Instance.SetLoop(false);
    _ = BGMTask(new CancellationTokenSource().Token);
  }

  private async UniTask BGMTask(CancellationToken ct)
  {
    AudioSource audio = AudioManager.Instance.CurrenBgmSource;
    await UniTask.WaitUntil(() => audio.clip.length - audio.time < 0.5f, cancellationToken: ct);
    AudioManager.Instance.SetLoop(true);
    AudioManager.Instance.PlayBgm("battle", 0.5f, AudioManager.Instance.DefaultVolumeBgm);
  }

  public void SetEnergy(int value)
  {
    Energy = value;
  }

  public Card DealCard()
  {
    if (CStack.ShowTop() == null) return null;
    Card dealed = CStack.PickUpTop();
    Hand.Add(dealed);
    return dealed;
  }

  public void InsertCard(Card card)
  {
    Hand.Remove(card);
    CStack.InsertBottom(card);
  }

  public void SetNextEnemyActions()
  {
    foreach (Enemy enemy in Enemies)
    {
      enemy.SetNextActionData(this);
    }
  }

  public void UpdateStatus()
  {
    List<Enemy> nextEnemies = new List<Enemy>();
    foreach (Enemy enemy in Enemies)
    {
      if (enemy.Hp > 0)
      {
        nextEnemies.Add(enemy);
      }
    }
    Enemies = nextEnemies;
  }

  public void EvaluateState()
  {
    if (MainCharacter.Hp == 0)
    {
      State = States.Lose;
    }
    else if (Enemies.Count == 0)
    {
      State = States.Win;
    }
  }

  public void SetTurn(Turns turn)
  {
    Turn = turn;
  }

  public void EnemyTurnStart()
  {
    foreach (Enemy enemy in Enemies)
    {
      enemy.AdvanceTurn();
    }
  }

  public void ReturnStackToDeck()
  {
    CStack.ReturnToDeck();
  }

  public void CardToSpecial(Card card)
  {
    Hand.Remove(card);
    SpecialCard = card;
  }

  public void DestroySpecialCard()
  {
    SpecialCard = null;
  }
}
