using System;
using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;

    [SerializeField] private float fireworkTime = 5f;
    [SerializeField] private int fireworkCount = 50;
    
    [FormerlySerializedAs("_castleRoot")] [SerializeField]
    private GameObject castleRoot;
    
    public static Action onStartGameWin;
    public static Action onStartGameLose;
    
    public static Action onGameWin;
    public static Action onGameLose;
    public static bool IsGameOver = false;

    private void Awake()
    {
        instance = this;
        IsGameOver = false;
        
    }
    
    public static void LoseGame()
    {
        IsGameOver = true;
        if (instance)
            instance.PlayLoseSequence();
    }

    private Coroutine _loseSequence = null;
    private void PlayLoseSequence()
    {
        _loseSequence ??= StartCoroutine(LoseSequence());
    }
    
    private IEnumerator LoseSequence()
    {
        onStartGameLose?.Invoke();
        
        yield return new WaitForSeconds(_preEndgameHoldSeconds);
        FadeScreen.instance.FadeOut();
        yield return new WaitForSeconds(FadeScreen.instance.fadeDuration + _EndGameCleanUpSeconds);

        TeleportPlayerToEndGame();
        EnemyManager.HideEnemies();

        FadeScreen.instance.FadeIn();
        yield return new WaitForSeconds(FadeScreen.instance.fadeDuration/2);
        
        _DestroyCastle();
        onGameLose?.Invoke();

        _loseSequence = null;
    }

    private void TeleportPlayerToEndGame()
    {
        PlayerStateController.TeleportPlayerToPoint(CastleController.GetEndgamePoint());
    }

    public static void WinGame()
    {
        IsGameOver = true;
        if (instance)
            instance.PlayWinSequence();
    }

    private Coroutine _winSequence = null;
    private float _preEndgameHoldSeconds = 4f;
    private float _EndGameCleanUpSeconds = 1f;

    private void PlayWinSequence()
    {
        _winSequence ??= StartCoroutine(WinSequence());
    }

    private IEnumerator WinSequence()
    {
        onStartGameWin?.Invoke();
        
        yield return new WaitForSeconds(_preEndgameHoldSeconds);
        FadeScreen.instance.FadeOut();
        yield return new WaitForSeconds(FadeScreen.instance.fadeDuration + _EndGameCleanUpSeconds);

        TeleportPlayerToEndGame();
        EnemyManager.HideEnemies();

        FadeScreen.instance.FadeIn();
        yield return new WaitForSeconds(FadeScreen.instance.fadeDuration);
        
        instance.StartCoroutine(_LaunchFireworks());
        SoundtrackManager.PlayMenu();
        onGameWin?.Invoke();

        _winSequence = null;
    }

    private static IEnumerator _LaunchFireworks()
    {
        var t = 0f;
        var rate = instance.fireworkTime / instance.fireworkCount;
        var i = 0f;
        while (t <= instance.fireworkTime)
        {
            yield return null;
            if (XRPauseMenu.IsPaused == false)
            { 
                t += Time.deltaTime;
                i += Time.deltaTime;
            }
            if (i >= rate)
            {
                Firework.SpawnFirework();
                i = 0f;
            }
        }
    }

    private void _DestroyCastle()
    {
        var anim = castleRoot.GetComponent<PlayableDirector>();
        anim.Play();
    }
    
    public void ReturnToMenu(float t = 0.5f)
    {
        FadeScreen.Fade_Out(t);
        StartCoroutine(_DelayReturnToMenu(t));
    }

    private IEnumerator _DelayReturnToMenu(float t)
    {
        yield return new WaitForSeconds(t);
        SceneLoaderAsync.LoadScene("MainMenu");
    }
    /*public void _StartEndgame(GameObject panel)
    {
        StartCoroutine(_endgameLogic(panel));
        SoundtrackManager.PlayMenu();
    }
    IEnumerator _endgameLogic(GameObject panel)
    {
        panel.SetActive(true);
        yield return new WaitForSeconds(waitBeforeEndingTime);
        ReturnToMenu();
    }*/
    /*public void _DetonateCastle()
    {
        return;
        StartCoroutine(_CastleDetonationRoutine());
        StartCoroutine(_MoveExplodingCastle());
    }
    public List<MeshRenderer> _castleRenderers = new List<MeshRenderer>();
    IEnumerator _CastleDetonationRoutine()
    {
        float timeBetween = _timeToDestroyCastle / (float)_castleRenderers.Count;
        float t = 0f;
        foreach (var renderer in _castleRenderers)
        {
            try { 
            renderer.enabled = false;
            foreach (var col in renderer.GetComponents<Collider>())
                col.enabled = false;
            }
            catch (MissingReferenceException e) { continue; }
            yield return new WaitForSeconds(timeBetween);
            _SpawnExplosionAt(renderer.transform.position);

        }
        //_castleRoot.gameObject.SetActive(false);
    }
    IEnumerator _MoveExplodingCastle()
    {
        yield return new WaitForSeconds(_timeToDestroyCastle / 2f);
        Vector3 startRot = _castleRoot.transform.localEulerAngles;
        Vector3 targetRot = startRot + localRotateTarget;
        Vector3 startPos = _castleRoot.transform.localPosition;
        Vector3 targetPos = startPos + localPosTarget;
        float time = 0f;
        while (time <= _timeToDestroyCastle)
        {
            float t = time / _timeToDestroyCastle;
            _castleRoot.transform.localEulerAngles = Vector3.Slerp(startRot, targetRot, t);
            _castleRoot.transform.localPosition = Vector3.Slerp(startPos, targetPos, t);
            yield return null;
            time += Time.deltaTime;
        }
        //yield break;
    }
    void _SpawnExplosionAt(Vector3 pos)
    {
        GameObject explosion = Instantiate(Resources.Load<GameObject>("Prefabs/explosion"));
        explosion.DestroyAfter(5f);
        explosion.transform.position = pos;
    }*/

    public void Quit()
    {
#if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        return;
#endif
        Application.Quit();
    }
}
