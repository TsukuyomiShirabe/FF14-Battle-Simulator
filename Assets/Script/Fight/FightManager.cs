﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using FFSimulator_client;

public class FightManager : MonoBehaviour
{

    public static FightManager Instance = null;

    public Text countDownText;
    public GameObject playerPrefab;

    public Dictionary<string, PlayerController> players = new Dictionary<string, PlayerController>();

    TimeLine timeLine = null;

    private void Awake()
    {
        players.Clear();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        timeLine = gameObject.AddComponent<TimeLine>();
    }

    private void Update()
    {
        if (FNet.Instance.connectionState != ConnectionState.OFFLINE)
        {
            CheckPlayerChanged();
            if (FNet.Instance.connectionState == ConnectionState.PLAYING && timeLine.isPlaying == false)
            {   // 接收到服务器开始执行时间轴的命令
                StartCoroutine(CountDown());
                InitTimeLine(60, 3);
                timeLine.isPlaying = true;
            }
            if (Input.GetKeyDown(KeyCode.F1))
            {   // 告知服务器即将开始时间轴
                FNet.Instance.StartPlaying();
            }
        }  
    }

    private void OnDestroy()
    {
        FNet.Instance.Quit();
    }

    // 倒计时
    IEnumerator CountDown()
    {
        // 倒计时文本
        countDownText.text = "3";
        yield return new WaitForSeconds(1);
        countDownText.text = "2";
        yield return new WaitForSeconds(1);
        countDownText.text = "1";
        yield return new WaitForSeconds(1);
        countDownText.text = "";
    }

    private void InitTimeLine(int durationSecond, int delaySecond)
    {
        // 时间轴内容
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        for (int i = 1 + delaySecond; i < durationSecond + delaySecond; i++)
        {
            foreach (PlayerController player in players)
                timeLine.AddDamage(i, DamageManager.Instance.CreateCircleTouchDamage(Vector3.zero, 2, 0.8f, 20000, player.gameObject));
            if (i % 3 == 0)
                timeLine.AddDamage(i, DamageManager.Instance.CreateCircleTouchDamage(Vector3.zero, 5, 2.5f, 50000));
            if (i % 6 == 0)
            {
                timeLine.AddDamage(i, DamageManager.Instance.CreateCircleTouchDamage(new Vector3(2, 0, 2), 4, 2f, 50000));
                timeLine.AddDamage(i, DamageManager.Instance.CreateCircleTouchDamage(new Vector3(2, 0, -2), 4, 2f, 50000));
                timeLine.AddDamage(i, DamageManager.Instance.CreateCircleTouchDamage(new Vector3(-2, 0, 2), 4, 2f, 50000));
                timeLine.AddDamage(i, DamageManager.Instance.CreateCircleTouchDamage(new Vector3(-2, 0, -2), 4, 2f, 50000));
            }
            else if (i % 12 == 0)
            {
                timeLine.AddDamage(i, DamageManager.Instance.CreateCircleTouchDamage(new Vector3(2, 0, 0), 4, 2f, 50000));
                timeLine.AddDamage(i, DamageManager.Instance.CreateCircleTouchDamage(new Vector3(0, 0, -2), 4, 2f, 50000));
                timeLine.AddDamage(i, DamageManager.Instance.CreateCircleTouchDamage(new Vector3(-2, 0, 0), 4, 2f, 50000));
                timeLine.AddDamage(i, DamageManager.Instance.CreateCircleTouchDamage(new Vector3(0, 0, 2), 4, 2f, 50000));
            }
        }
    }

    public void CheckPlayerChanged()
    {
        foreach (string name in FNet.Instance.playerNicknames.Keys)
        {
            if (!players.ContainsKey(name))
            {
                PlayerController player = Instantiate(playerPrefab, GenerateRespawnPos(),Quaternion.identity).GetComponent<PlayerController>();
                players.Add(name, player);
                player.Init(name);
                ChatManager.Instance.AddMessage("欢迎玩家 " + name + " 进入房间！", true);
            }
        }
        foreach (var player in players.ToList())
        {
            if (!FNet.Instance.playerNicknames.ContainsKey(player.Key))
            {
                ChatManager.Instance.AddMessage("玩家 " + player.Key + " 退出房间！", true);
                Destroy(player.Value.gameObject);
                players.Remove(player.Key);
            }
        }
    }

    public Vector3 GenerateRespawnPos()
    {
        return new Vector3(-4, 0.3f, 0) + Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360), Vector3.up) * Vector3.forward * 0.5f;
    }
}
