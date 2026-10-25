using UnityEngine;

public class PlayerController : MonoSingleton<PlayerController>
{
    [Header("移动速度")]
    public float moveSpeed = 5.0f;

    [Header("视角设置")]
    public float mouseSensitivity = 2.0f;
    [Tooltip("摄像机垂直向上看的最大角度")]
    public float maxLookAngle = 80.0f;
    [Tooltip("摄像机垂直向下看的最大角度")]
    public float minLookAngle = -80.0f;

    [Header("组件引用")]
    [Tooltip("需要被控制的摄像机的 Transform 组件")]
    public Transform cameraTransform;

    private PlayerController playerController;
    private float cameraPitch = 0.0f;
    
    void Start()
    {
        // 检查是否已在 Inspector 窗口中分配摄像机
        if (cameraTransform == null)
        {
            Debug.LogError("错误：摄像机未分配！请将主摄像机的 Transform 组件拖拽到脚本的'Camera Transform'字段中。");
            // 禁用此脚本以防止后续Update中出现空引用错误
            this.enabled = false; 
            return;
        }

        cameraTransform.position = transform.position;

        // 锁定鼠标光标到游戏窗口中央并隐藏它
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    /// <summary>
    /// Update 每帧被调用一次。
    /// </summary>
    void Update()
    {
        // --- 默认的玩家输入处理 ---
        // 在每一帧都检查并处理来自玩家的键盘和鼠标输入。
        ProcessPlayerInput();
        ProcessMouseInput();
    }


    private void ProcessPlayerInput()
    {
        // 获取键盘的水平和垂直输入值（范围在-1到1之间）
        float horizontal = Input.GetAxis("Horizontal"); // A/D 或左右箭头
        float vertical = Input.GetAxis("Vertical");     // W/S 或上下箭头
        
        // 调用移动API，将输入传递给它
        Move(new Vector2(horizontal, vertical));
    }

    /// <summary>
    /// 读取并处理玩家的鼠标视角输入。
    /// </summary>
    private void ProcessMouseInput()
    {
        // 获取鼠标在X和Y轴上的移动量
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        
        // 调用视角API，将输入传递给它
        Look(new Vector2(mouseX, mouseY));
    }


    public void Move(Vector2 moveInput)
    {
        Vector2 direction = moveInput.normalized;
        

        if (direction.magnitude > 1)
        {            
            direction.Normalize();
        }

        transform.Translate(new Vector3(direction.x * moveSpeed * Time.deltaTime, 0, direction.y * moveSpeed * Time.deltaTime));
        cameraTransform.position = transform.position;
    }

    /// <summary>
    /// 控制玩家视角的公共方法。
    /// </summary>
    /// <param name="lookInput">一个Vector2，其中X是水平视角移动量，Y是垂直视角移动量。</param>
    public void Look(Vector2 lookInput)
    {
        if (lookInput.x != 0)
        {
            transform.Rotate(Vector3.up * lookInput.x * mouseSensitivity);

        }// 左右旋转 (偏航角): 直接旋转玩家游戏对象本身（绕Y轴）

        // 上下旋转 (俯仰角): 只旋转摄像机（绕X轴）
        cameraPitch -= lookInput.y * mouseSensitivity;

        // 使用 Mathf.Clamp 方法将俯仰角限制在预设的最大和最小值之间，防止视角翻转
        cameraPitch = Mathf.Clamp(cameraPitch, minLookAngle, maxLookAngle);

        // 应用计算好的俯仰角到摄像机的 localRotation。
        // 使用 localRotation 可以确保摄像机是相对于父对象（玩家）进行旋转的。
        cameraTransform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
    }
}
