using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UIFruitLuckyControl : MonoBehaviour
{
    public List<UIFruitLuckyResult> fruitList;

    private enum ControlMode
    {
        RandomSelectMode,
        SequentialSelectMode,
        AllSelectMode
    }

    private ControlMode currentMode;
    private ControlMode lastMode;

    private bool isControlEnabled = false;  // 控制开关
    private Coroutine controlLoopCoroutine = null;  // 存储控制协程

    private void Start()
    {
        // 初始化时禁用控制模式
        SetAllSelected(false);  // 初始时所有对象都设置为 false
        lastMode = ControlMode.AllSelectMode; // 假设不会一开始就重复
    }

    // 启动或停止控制
    public void ToggleControl(bool enable)
    {
        if (enable && !isControlEnabled)
        {
            // 启动控制
            isControlEnabled = true;
            SwitchControlMode();
            controlLoopCoroutine = StartCoroutine(ControlLoop());  // 启动控制协程
        }
        else if (!enable && isControlEnabled)
        {
            // 停止控制
            isControlEnabled = false;
            StopCoroutine(controlLoopCoroutine);  // 停止控制协程
            SetAllSelected(false);  // 所有水果对象设置为 false
        }
    }

    private void SetAllSelected(bool selected)
    {
        foreach (var fruit in fruitList)
        {
            fruit.SetSelected(selected);
        }
    }

    private void SwitchControlMode()
    {
        // 随机选择不同的控制模式，确保和上一个控制模式不同
        ControlMode[] modes = (ControlMode[])System.Enum.GetValues(typeof(ControlMode));
        List<ControlMode> validModes = new List<ControlMode>();

        foreach (var mode in modes)
        {
            if (mode != lastMode)
                validModes.Add(mode);
        }

        currentMode = validModes[Random.Range(0, validModes.Count)];
        lastMode = currentMode; // 更新为当前模式，以备下次选择
    }

    private IEnumerator ControlLoop()
    {
        while (isControlEnabled)
        {
            switch (currentMode)
            {
                case ControlMode.RandomSelectMode:
                    yield return StartCoroutine(RandomSelectMode());
                    break;
                case ControlMode.SequentialSelectMode:
                    yield return StartCoroutine(SequentialSelectMode());
                    break;
                case ControlMode.AllSelectMode:
                    yield return StartCoroutine(AllSelectMode());
                    break;
            }

            // 切换控制模式
            SwitchControlMode();
        }
    }

    private IEnumerator RandomSelectMode()
    {
        foreach(int index in Enumerable.Range(0,5))
        {
            // 随机选择 SetSelected 状态
            foreach (var fruit in fruitList)
            {
                bool randomState = Random.Range(0, 2) == 0;
                fruit.SetSelected(randomState);
            }

            // 等待 1 秒
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator SequentialSelectMode()
    {
        foreach(int index in Enumerable.Range(0, fruitList.Count * 2))
        {
            // 依次设置 SetSelected 为 true，其余为 false
            for (int i = 0; i < fruitList.Count; i++)
            {
                fruitList[i].SetSelected(i == index % fruitList.Count);
            }

            // 每 0.5 秒刷新一次
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator AllSelectMode()
    {
        foreach(int index in Enumerable.Range(0, 2))
        {
            // 所有对象 SetSelected 为 true
            foreach (var fruit in fruitList)
            {
                fruit.SetSelected(true);
            }

            // 等待 1 秒
            yield return new WaitForSeconds(1f);

            // 所有对象 SetSelected 为 false
            foreach (var fruit in fruitList)
            {
                fruit.SetSelected(false);
            }

            // 等待 1 秒
            yield return new WaitForSeconds(1f);
        }
    }
}
