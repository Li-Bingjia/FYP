using System;
using UnityEngine;

/// <summary>
/// 全局游戏暂停控制器。只暂停游戏内容，不影响UI和对话等交互。
/// 通过事件通知游戏内容暂停/恢复，便于后续扩展和维护。
/// </summary>
public static class PauseController
{
    /// <summary>
    /// 当前游戏是否处于暂停状态
    /// </summary>
    public static bool IsGamePaused { get; private set; } = false;

    /// <summary>
    /// 游戏暂停状态变更事件。参数为当前是否暂停。
    /// 游戏内容脚本可订阅此事件以响应暂停/恢复。
    /// </summary>
    public static event Action<bool> OnPauseStateChanged;

    /// <summary>
    /// 设置游戏暂停或恢复
    /// </summary>
    /// <param name="pause">true=暂停，false=恢复</param>
    public static void SetPause(bool pause)
    {
        if (IsGamePaused == pause) return;
        IsGamePaused = pause;
        OnPauseStateChanged?.Invoke(IsGamePaused);
    }
}