using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace DahuaUICamera.SDKTvt
{
    using System.Net;
    using System;
    using POINTERHANDLE = Int32;
    using System.Runtime.InteropServices.ComTypes;

    #region 委托
    // Set Message Callback Function 设置消息回调函数
    public delegate void fMessageCallback(UInt32 lLoginID, UInt32 lMsgType, IntPtr szBuf, UInt32 nLen, IntPtr pUser);
    //回调音视频数据、对讲音频,
    public delegate void fRealStreamCallback(ulong lStreamID, /*PlatFrameHeader* */IntPtr pHeader, IntPtr szBuf, UInt32 nLen, IntPtr pUser);
    //画图叠加回调,  实时流句柄、DC句柄，用户数据
    public delegate void fDrawCallback(ulong lStreamID,/*HDC*/IntPtr hDC, IntPtr pUser);//windows才支持
                                                                                        //回放数据回调， 句柄，设备标识，数据，
    public delegate void fPlayStreamCallback(ulong lStreamID, /*PlatFrameHeader * */IntPtr pHeader, IntPtr szBuf, UInt32 nLen, IntPtr pUser);
    //回调报警数据
    public delegate void fAlarmCallback(UInt32 lLoginID, IntPtr szBuf, UInt32 nLen, IntPtr pUser);
    public delegate void fAlarmCallbackEx(UInt32 lLoginID, IntPtr szBuf, UInt32 nLen, UInt32 nType, IntPtr pUser);
    // New alarm callback function, compatible with the above two methods.
    // Newly added SDC camera, whose alarm type is not fixed, and the translation needs to be returned together.
    // szBuf is the new structure Plat_NewAlarmInfo.
    public delegate void fAlarmCallback_V2(UInt32 lLoginID, IntPtr szBuf, UInt32 nLen, IntPtr pUser);
    //电视墙获取通道id
    public delegate void fKeyboardMessageCallback(UInt32 lLoginID, int lMon, int lWin, Guid nodeId, int lstate, IntPtr pUser);
    #endregion 委托

    #region 结构体，枚举定义

    /****PTZ相关操作****/
    public enum enPlat_PTZ_Control
    {
        PTZ_CONTROL_TOP = 1,			////向上走
        PTZ_CONTROL_BOTTOM,				////向下走
        PTZ_CONTROL_LEFT,				////向左
        PTZ_CONTROL_RIGHT,				////向右
        PTZ_CONTROL_LEFT_TOP,			//左上
        PTZ_CONTROL_LEFT_BOTTOM,		//左下
        PTZ_CONTROL_RIGHT_TOP,			//右上
        PTZ_CONTROL_RIGHT_BOTTOM,		//右下
        PTZ_CONTROL_FOCUSNEAR,			////调焦近
        PTZ_CONTROL_FOCUSFAR,			////调焦远
        PTZ_CONTROL_ZOOMIN,				///放大
        PTZ_CONTROL_ZOOMOUT,			////缩小
        PTZ_CONTROL_IRISOPEN,			////聚焦
        PTZ_CONTROL_IRISCLOSE,			////聚焦

        PTZ_CONTROL_STOP = 100,    /////停止
        PTZ_CONTROL_PRESETADD,	   //增加预置点
        PTZ_CONTROL_PRESETGET,		//获取预置点
        PTZ_CONTROL_PRESETDEL,		//删除预置点
        PTZ_CONTROL_PRESETMODIFY,		////设置某一预置点，只是修改预置点位置
        PTZ_CONTROL_PRESETGO,      ////调用预置点

        PTZ_CONTROL_CRUISEADD,      ////添加巡航线
        PTZ_CONTROL_CRUISEDEL,    //删除巡航线
        PTZ_CONTROL_CRUISEMODIFY, //修改巡航线
        PTZ_CONTROL_CRUISEGET,    //获取巡航线
        PTZ_CONTROL_CRUISEGO,      ////到某一巡航线
        PTZ_CONTROL_CRUISESTOP,    //停止巡航
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_Preset_Item
    {
        public Int32 nStructSize;
        public Int32 nIndex;                // 预置点索引
        public byte bEnable;			// 是否启用预置点
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
        public byte[] byReserve;            //预留
        public Int32 nSpeed;             //速度，1－8；
        public Int32 nlen;				//名称长度
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] szName;			//预置点名称
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_Preset_Info
    {
        public Int32 nStructSize;
        public Int32 nMaxNum;           // 最多可添加的预置点值
        public Int32 nMaxNameLen;       // 名称最大字节数
        public byte bFixedCount;		// 是否固定预置点个数，固定模式，预置点没有清除功能
        public byte bNameReadonly;		// 预置点名称只读
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 2)]
        public byte[] byReserve;            //预留
        public Int32 listCount;       // 多少个有效的Plat_Preset_Item[]
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256)]
        public Plat_Preset_Item[] pPresetList; //
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_CruisePoint
    {
        public Int32 nIndex;			// 预置点索引
        public Int32 nDwellSpeed;	// 轮询速度
        public Int32 nDwellTime;		// 轮询时间	
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_CruiseItem
    {
        public Int32 nStructSize;               //新增 结构体字节大小；包含了pCruisePointList；
        public Int32 nIndex;						// 巡航线索引
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] szName;               // 巡航线名称,修改
        public Int32 listCount;				// 多少个有效Plat_CruisePoint[]
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 128)]
        public Plat_CruisePoint[] pCruisePointList;	// 巡航点列表数组
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_CruiseInfo
    {
        public Int32 nStructSize;               //新增 结构体字节大小；包含了pCruiseItem；
        public byte bFixedCount;                // 固定巡航线数量
        public byte bNameReadonly;				// 巡航线名称是否只读
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 2)]
        public byte[] byReserve;            //预留
        public Int32 nCruisePresetMaxNum;       // 最多可添加的巡航点数量
        public Int32 nChCruiseMaxNum;           // 最多可添加的巡航线数量
        public Int32 nMaxNameLen;               // 巡航线名称最大字节数
        public Int32 nItemCount;	//有效巡航线条数
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public Plat_CruiseItem[] pCruiseItem; //巡航线数组
    };

    /*错误码*/
    public enum PLAT_ERROR_ID
    {
        PLAT_SUCCESS = 0,                    // 0-操作成功
        PLAT_ERROR_UNKNOWN,                  // 1-未知错误 ..
        PLAT_ERROR_NOINIT,                   // 2- 未初始化, Not Initliaze SDK API
        PLAT_ERROR_NO_LOGIN,                 // 3-未登录, Not Login Platform
        PLAT_ERROR_PASSWORD,                 // 4-用户名密码不匹配 , Password Error,
        PLAT_ERROR_NOENOUGHPRI,              // 5-权限不足 ..	Insufficient authority
        PLAT_ERROR_FAIL_CONNECT,             // 6-连接服务器失败, Net Connect Platformat Server Failed
        PLAT_ERROR_NO_USER,                  // 7-用户不存在 ,   user don't exist for Platform
        PLAT_ERROR_USER_LOCKED,              // 8-用户锁定，不能使用, User locked, don't login
        PLAT_ERROR_USER_ALREADY_LOGIN,       // 9-用户已登录,  User Already Logined;
        PLAT_ERROR_DISK_SPACE_NO_ENOUGH,     // 10-磁盘空间不足,
        PLAT_ERROR_NODE_NET_DISCONNECT,      // 11-NODE网络断开, Net Disconnect
        PLAT_ERROR_NODE_NET_OFFLINE,         // 12-NODE不在线
        PLAT_ERROR_INVALID_PARAM,            // 13-无效参数  // 参数不对
        PLAT_ERROR_NO_SUPPORT,               // 14-该功能不支持，无该能力
        PLAT_ERROR_DEVICE_BUSY,              // 15-设备忙,
        PLAT_ERROR_DEV_PLAYSTREAM,           // 16-
        PLAT_ERROR_NO_RECORDDATA,            // 17-无录像数据, don't Record File Data
        PLAT_ERROR_UNSUPPORTED_CMD,          // 18-不支持命令， UnSupported Command
        PLAT_ERROR_IA_ADD_ERR_NO_IMAGE,      // 19-没有图片
        PLAT_ERROR_IA_ADD_ERR_NO_FACE,       // 20-没有人脸
        PLAT_ERROR_IA_ADD_ERR_FACE_TOO_LARGE,// 21-人脸太大
        PLAT_ERROR_IA_ADD_ERR_INVALID_IP,    // 22-ip地址错误
        PLAT_ERROR_IA_ADD_ERR_SYS_ERROR,     // 23-盒子处理异常
    };

    /**Message ID*/
    public enum Plat_MsgType
    {
        MSGTYPE_CONNECT_NTF = 1,        //登录、连接相关， &SatusID			
        MSGTYPE_RESLIST_NTF,            //Device Node Information, 
        MSGTYPE_LIVEPLAY,               //实时播放,  对讲  结果
        MSGTYPE_PRESETINFO,             //预置点
        MSGTYPE_CRUISEINFO,             //巡航线
        MSGTYPE_CONFIG,                 //配置
        MSGTYPE_PLAYBACK_NTF,           //回放, 下载
        MSGTYPE_LABEL_NTF,              //录像标签
        MSGTYPE_FACE_MATCH,             //人脸比对
        MSGTYPE_AISERVER,               //智能分析服务器
        MSGTYPE_NETKEYBOARD_NTF,        //网络键盘
        MSGTYPE_SEARCHPIC_NTF,//搜索IPC图片
        MSGTYPE_UPGRADEDEV_NTF,//升级设备
        MSGTYPE_STORAGESERVER,//存储服务器
        MSGTYPE_CHLUNDERSTORAGE,//存储服务器下的设备信息
        MSGTYPE_ATTEND_INFO,//人脸考勤信息
        MSGTYPE_CPC_INFO,//人数统计信息
        MSGTYPE_STORAGE_INFO,//硬盘容量信息
        MSGTYPE_ALARMHOST,//报警主机信息
        MSGTYPE_ALARMZONE,//报警防区信息
        MSGTYPE_LICENSE_PLATE_PICTURE,//车牌信息
        MSGTYPE_ACCESS_STATE_INFO,//门禁状态信息
        MSGTYPE_ORG_INFO,       //组织信息
        MSGTYPE_TARGET_INFO,    //目标信息
        MSGTYPE_MATCH_UPDATE_PERSION_DEV,//下发人员到配置
        MSGTYPE_MATCH_GET_PERSION_DEV,//获取人员下发配置
        MSGTYPE_MATCH_GET_PERSION_BY_DEV,//通过设备获取人员权限列表
        MSGTYPE_MATCH_GET_VERIFIEDTARGETLIST,//获取人脸比对人脸信息列表
        MSGTYPE_MATCH_GET_VERIFIEDTARGETDETAIL,//获取人脸比对人脸信息详情
        MSGTYPE_CMD_RESULT_CB,    //一些异步命令的简单结果通知
        MSGTYPE_CMD_FACE_DETECION,  //人脸检测通知
        MSGTYPE_CMD_VECHLE_INFO,	//停车场相关信息
        MSGTYPE_CMD_PASSRECORD_INFO,	//通行记录查询返回
    };

    public enum enPlat_ConnectState
    {
        PLAT_NOLOGIN = 2,           ////没有启动登陆
        PLAT_CONNECTING = 3,        ////正在连接		
        PLAT_CONNECT_FAIL,      //////连接失败
        PLAT_CONNECT_SUCCESS,   //////连接成功
        PLAT_LOGINING,          //////////正在登陆
        PLAT_LOGIN_FAIL,        ////////登陆失败
        PLAT_LOGIN_SUCCESS,     ///////登陆成功
        PLAT_DISCONNECT,        //与管理服务器连接断开
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Login_MsgResult
    {
        public Int32 nLoginID;          // Login ID， login() return Value
        public Int32 nConnectState;     //enPlat_ConnectState值
        public UInt32 nErrorID;  //错误码
    };

    public enum enPlat_StreamType
    {
        PLAT_STREAM_NULL = 0,
        PLAT_LIVE_MAIN_STREAM,
        PLAT_LIVE_SUB_STREAM,
    };

    public enum enPlat_PalyMsgType
    {
        PLAT_PLAYOPT_NULL = 0,
        PLAT_PLAYOPT_REALSTREAM,    //start realstream
        PLAT_PLAYOPT_TALK,          //start talk
        PLAT_PLAYOPT_STREAM_EXCEPTION_CLOSE, //流异常关闭；包括对讲流或实时流
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct StreamPlay_MsgResult
    {
        public UInt32 StreamObject;   //流对象 
        public UInt32 ulStreamID;   //流ID 
        public UInt32 ulNodeID;     //通道NodeID
        public Guid GuidNodeID;
        public Int32 nOptType;       //实时视频，对讲
        public Int32 nResult;             //结果值， 0：失败，1：成功;
        public UInt32 nErrorID;    //错误码
    };


    /*
     * Memory 
     * ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
     * + sizeof(stPreset_MsgResult) | pPresetInfo, memory length:nLen    |
     * +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
     */
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Preset_MsgResult
    {
        public UInt32 ulNodeID;     //Channel ID
        public Guid GuidNodeID;
        public Int32 nOptType;              //enPlat_PTZ_Control 值
        public Int32 nResult;
        public UInt32 nErrorID;
        public Int32 nLen;                  //新增， Plat_Preset_Info 字节长度
        public IntPtr/*Plat_Preset_Info*/ pPresetInfo;
    };

    /*
     * Memory 
     * +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
     * + sizeof(stCruise_MsgResult) | pCruiseInfo, memory length:nLen     |
     * +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
     */
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Cruise_MsgResult
    {
        public UInt32 ulNodeID;     // 节点ID
        public Guid GuidNodeID;
        public Int32 nOptType;              //enPlat_PTZ_Control 值
        public Int32 nResult;               //结果值，1：成功；0：失败
        public UInt32 nErrorID;     //错误码
        public Int32 nLen;                  //新增， pCruiseInfo 字节长度
        public IntPtr/*Plat_CruiseInfo*/ pCruiseInfo;
    };

    //节点类型
    public enum enPlat_NodeType
    {
        NODETYPE_NONE = 0,
        NODETYPE_AREA,
        NODETYPE_DEVICE,
        NODETYPE_CHANNEL,
        NODETYPE_SENSOR,	 //报警输入

    };

    public enum enPlat_DevType
    {
        DEVTYPE_NONE = 0,
        DEVTYPE_DVR,  //tvt
        DEVTYPE_MDVR,
        DEVTYPE_NVR,
        DEVTYPE_IPC,
        DEVTYPE_HIK,
        DEVTYPE_DAHUA,
        DEVTYPE_ONVIF,
    };

    //节点类型
    public enum enPlat_VecheAndCamType
    {
        VECHEANDCAMNODETYPE_NONE = 0,
        VECHEANDCAMNODETYPE_PARK,       //停车场
        VECHEANDCAMNODETYPE_GATEWAY,    //出入口
        VECHEANDCAMNODETYPE_LINE,       //车道
        VECHEANDCAMNODETYPE_CAM,	    //相机
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_ResNodeInfo
    {
        public UInt32 ulNodeID;
        public Guid GuidNodeID;
        public UInt32 ulParentID;			//如果是区域，则是上一级区域的id；如果是设备，则是所属区域的id；如果是通道，则是所属设备的id
        public Guid guidParentID;           //如果是区域，则是上一级区域的guid；如果是设备，则是所属区域的guid；如果是通道，则是所属设备的guid
                                            //sdk2.0 库中已经修改为256
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256)]
        public byte[] szName;           //
        public Int32 nNodeType;         //区域、设备、通道； enPlat_NodeType
        public Int32 nDevType;
        public Int32 nOnline;           // 1-在线； 0－离线,  设备、通道有效
        public Int32 nChlCount;         // 通道数
        public UInt16 usSensorInNum;        //报警输入个数
        public UInt16 usAlarmOutNum;        //报警输出个数
        public Int32 nChlNO;                //通道号	
        public byte bisSupportFaceMatch;    //是否支持人脸比对
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 32)]
        public byte[] szIp;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
        public byte[] byReserve;//保留
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_AnsiServerInfo
    {
        public UInt32 ulNodeID;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256)]
        public byte[] szName;
        public Guid guid;
    };

    public enum enPlat_NodeOptType
    {
        NODEOPTTYPE_CREATE = 0,  // Received New Node
        NODEOPTTYPE_DELETE,      // Received Deleted Node Information, ulNodeID是有效
        NODEOPTTYPE_CHANGENAME,  //节点被修改, szName 是有效，
        NODEOPTTYPE_UPDATESTATE, // 节点状态被更新
        NODEOPTTYPE_UPDATE_DEVAREA,  //修改设备所属的区域,stPlat_ResListMsg::lpNodeinfo,有效；

    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_ResListMsg
    {
        public Int32 nStructSize;
        public Int32 nOptType;              //enPlat_NodeOptType
        public byte bFinish;				//
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
        public byte[] byReserve;
        public UInt32 ulNodeID;
        public Int32 nConnState;                //NODEOPTTYPE_UPDATESTATE, 有效; 其他操作值，无效
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] szName;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 32)]
        public byte[] szIp;
        public Int32 nBufLen;               //lpNodeinfo的内存长度,用于判断lpNodeinfo是否存在，为NULL； 新增
        public /*Plat_ResNodeInfo* */IntPtr lpNodeinfo;            	// //NODEOPTTYPE_CREATE, 有效; 其他操作值，NULL
        public Guid guid;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_AnsiServerListMsg
    {
        public Int32 nStructSize;
        public Int32 nOptType;              //enPlat_NodeOptType
        public UInt32 ulNodeID;
        public Guid guid;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] szName;
        public Int32 nBufLen;               //lpNodeinfo的内存长度,用于判断lpNodeinfo是否存在，为NULL； 新增
        public byte bFinshed;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
        public byte[] byReserve;
        public /*LpPlat_AnsiServerInfo*/IntPtr lpNodeinfo;            	// //NODEOPTTYPE_CREATE, 有效; 其他操作值，NULL
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_ResVechleNodeInfo
    {
        public UInt32 ulNodeID;
        public Guid GuidNodeID;
        public UInt32 ulParentID;			//如果是区域，则是上一级区域的id；如果是设备，则是所属区域的id；如果是通道，则是所属设备的id
        public Guid guidParentID;           //如果是区域，则是上一级区域的guid；如果是设备，则是所属区域的guid；如果是通道，则是所属设备的guid
                                            //sdk2.0 库中已经修改为256
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256)]
        public byte[] szName;           //windows gb2312编码 其他是utf8
        public int nNodeType;          //停车场、出入口、车道，监控通道IPC

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
        public byte[] byReserve; ////保留
    };


    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_VechleResListMsg
    {
        public Guid guid;
        public int ulNodeId;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] szName;
        public int nStructSize;
        public int nOptType;               //enPlat_NodeOptType
        public int nBufLen;                //lpNodeinfo的内存长度,用于判断lpNodeinfo是否存在，为NULL； 新增
        public /*Plat_ResVechleNodeInfo* */IntPtr lpNodeinfo;

    }
    public enum enPlayBack_ControlCmd
    {
        PLAYBACK_CMD_PAUSE = 0,   //暂停
        PLAYBACK_CMD_RESUME,    //恢复
        PLAYBACK_CMD_FAST, // 快放，
        PLAYBACK_CMD_SLOW, // 慢放
        PLAYBACK_CMD_SETPOS, //设置时间点//lparam传入开始时间 格式同Plat_StartDownloadByTime的startTm 调用PLAYBACK_CMD_SETPOS成功后，需要注意播放速度变成正常速度
    };

    public enum enRecFile_OptType
    {
        RECOPT_SEARCHERFILE = 0,  //搜索 查找录像文件
        RECOPT_STARTPLAYBACK,     //请求文件流
        RECOPT_PLAYBACK_END,      //结束文件流
        RECOPT_STARTDOWNLOAD,     //开始下载	  
        RECOPT_DOWNLOAD_END,	  //下载结束
    };

    //录像数据类型
    public enum enPlat_Record_Type
    {
        PLATREC_TYPE_NULL = 0x00,           //空
        PLATREC_TYPE_MANUAL = 0x01,         //手动录像
        PLATREC_TYPE_SCHEDULE = 0x02,       //排程录像
        PLATREC_TYPE_MOTION = 0x04,         //移动侦测录像
        PLATREC_TYPE_SENSOR = 0x08,         //传感器录像
        PLATREC_TYPE_VLOSS = 0x10,          //视频丢失
        PLATREC_TYPE_OVERSPEED = 0x20,      //超速
        PLATREC_TYPE_OVERBOUND = 0x40,      //越界(车载地图)
        PLATREC_TYPE_OSC = 0x80,            //物品看护
        PLATREC_TYPE_AVD = 0x100,           //视频异常
        PLATREC_TYPE_PEA_TRIPWIRE = 0x200,  //越界侦测(智能)
        PLATREC_TYPE_PEA_PERIMETER = 0x400, //区域入侵
        PLATREC_TYPE_ALL = 0xFFFF,		/////所有的录像类型
    };

    /*
     * Memory 
     * +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
     * + sizeof(stPlayBack_MsgResult) | pBuf, memory length:nBufLen |
     * ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
     */
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PlayBack_MsgResult
    {
        public UInt32 ulNodeID;	//通道ID
        public Guid guid;
        public UInt32 ulStreamID;   //流ID
        public Int32 nOptType;          //查询，回放，下载，
        public Int32 nResult;
        public UInt32 nErrorID;
        public Int32 nBufLen;
        public IntPtr pBuf;		//stPlat_Rec_File_
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_Rec_File
    {
        public UInt32 ulNodeID;			// Channel Node ID
        public Guid guid;
        public UInt16 nRecType;				// 录像类型：enPlat_Record_Type
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 2)]
        public byte[] byReserve;
        public UInt64 ullstartTime;             // 开始时间   从1970 1 1号开始绝对时间的秒数
        public UInt64 ullendTime;				// 结束时间   从1970 1 1号开始绝对时间的秒数
    };

    /*参数设置相关结构体*/
    public enum enPlat_Config_Opt
    {
        CONFIG_GETOSD = 300,  //获取OSD信息
        CONFIG_SETOSD,        //设置OSD信息
        CONFIG_GETQUALITY,    //获取通道所有视频质量参数信息
        CONFIG_SETQUALITY	 // 设备通道所有视频质量参数信息

    };
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct dvrPos
    {
        public Int16 nHPos;
        public Int16 nVPos;
    }
    //IPC DVR 不一样；
    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct DisplayPos
    {
        //[FieldOffset(0)]
        //public Int16 nHPos;
        //[FieldOffset(2)]
        //public Int16 nVPos;
        [FieldOffset(0)]
        public dvrPos dpos;
        [FieldOffset(0)]
        public Int32 nSelPos;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_OSDPos
    {
        public byte bShow;  // 是否显示位置信息
        public byte bIsSelValid; //是IPC设备，nSelPos有效
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 2)]
        public byte[] byReserve;
        public DisplayPos displayPos;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_OSDInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] szchlName;
        public Plat_OSDPos Namepos;  //名称OSD位置
        public Plat_OSDPos Timepos;  //时间OSD位置
    }

    ///
    //分辨率、码率,码率类型， 帧率，
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_Resolution
    {
        public Int32 nStructSize;
        public Int32 nID;					// 分辨率ID
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 128)]
        public byte[] strResName;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 100)]
        public Int32[] pBitRatelist;		//码率列表, kbps
    };

    //各列表
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_Stream_QualityConfig
    {
        public Int32 nStructSize;
        public Int32 nStreamTypeID;         //流ID， 0：主流， 1：子流，2：第三码流
        public Int32 nResolution;               // 当前选择的分辨率值
        public Int32 nBitRate;              // 当前选择的码率值 支持码率自定义, kbps
        public Int32 nPicQuality;           // 当前选择的画质值：1:lowest 2=lower 3=medium 4=higher 5=highest
        public Int32 nBitrateType;          // 当前选择的码率类型值：固定码率cbr:2， 可变码率vbr:1
        public Int32 nVideoRate;                // 当前选择的帧率值;1-30,1-60

        public byte bBitRateEnable;			// 码率是否可修改，true: Write/Read; false: only read
        public byte bBitrateTypeEnable;		// 码率类型是否可修改,true: Write/Read; false: only read
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 2)]
        public Plat_Resolution[] byReserve; //保留
        public Int32 nResListCount;				 //pResolution 的有效个数
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 32)]
        public Plat_Resolution[] pResolution; //分辨率列表数组, 修改
    };

    /*
     * Memory 
     * +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
     * + sizeof(stConfig_MsgResult) | pBuffer,                       |
     * ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
     */
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Config_MsgResult
    {
        public UInt32 ulNodeID;      //Node ID
        public Guid guid;
        public Int32 nOptCmd;                  //操作命令enPlat_Config_Opt
        public Int32 nResult;                 //结果值，1：成功；0：失败
        public UInt32 nErrorID;     //Error Value
        public Int32 nSize;					//Plat_OSDInfo struct count  OR Plat_Stream_QualityConfig struct count ；
        public IntPtr pBuffer;				//Plat_OSDInfo*; OR Plat_Stream_QualityConfig*
    };

    // 其他信息详情紧跟着该结构体，内存上是连续的
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_MsgResult
    {
        public Int32 nOptCmd;           // 操作命令enPlat_Face_Match_Opt
        public Int32 nResult;           // 结果值，1：成功；0：失败
        public UInt32 nErrorID; // Error Value
        public UInt32 targetCount;	//目标数量
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct RecordLabel
    {
        public Int32 nStructSize;
        public long lChlNodeID;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256)]
        public byte[] szLabelName;  //名称
        public Int64 LabelTime;   //
        public UInt32 nLabelID; //标签ID, 新增17-06-28
    };

    /*
     * Memory 
     * +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
     * + sizeof(stConfig_MsgResult) | pBuffer, memory length: nBufSize |
     * +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
     */
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct RecordLabel_MsgResult
    {
        public Int32 nResult;                 //结果值，1：成功；0：失败
        public UInt32 nErrorID;     //错误码
        public Int32 nBufSize;              //pBuffer字节数
        public IntPtr pBuffer;				//RecordLabel Array；
    };

    //报警类型
    public enum enPlat_Alaram_Type
    {
        PLAT_ALARM_TYPE_BEGIN,
        PLAT_ALARM_TYPE_MOTION = 0x01,/////移动侦测报警输入
        PLAT_ALARM_TYPE_SENSOR,/////传感器报警输入
        PLAT_ALARM_TYPE_VLOSS,////视频丢失报警输入
        PLAT_ALARM_TYPE_FRONT_OFFLINE, //////前端设备掉线报警
        PLAT_ALARM_TYPE_OSC, ////物品看护侦测报警 
        PLAT_ALARM_TYPE_AVD, ////视频异常侦测报警 
        PLAT_ALARM_TYPE_PEA_TRIPWIRE, ////越界侦测报警 
        PLAT_ALARM_TYPE_PEA_PERIMETER, ////区域入侵侦测报警 
        PLAT_ALARM_TYPE_PEA, ////越界侦测与区域入侵侦测
        PLAT_ALARM_TYPE_VFD, ////人脸识别侦测报警 

        PLAT_ALARM_TYPE_CPC,//人数统计报警，笼统的（设备暂时上报的没有做区分，所以只能采用笼统这个）
        PLAT_ALARM_TYPE_ENTER_AREA_OVER_LINE,//人数统计： 1、进入人数超过指定阈值， EnterAreaOverLine
        PLAT_ALARM_TYPE_LEAVE_AREA_OVER_LINE,//人数统计：2、离开人数超过指定阈值LeaveAreaOverLine
        PLAT_ALARM_TYPE_STAY_AREA_OVER_LINE,//人数统计：3、滞留人数超过指定阈值， StayAreaOverLine
        PLAT_ALARM_TYPE_TARGET_IN_BLACKLIST,//人脸布控： 1、黑名单匹配，TargetInBlackList
        PLAT_ALARM_TYPE_TARGET_UNDEFINED,//人脸布控：2、白名单不匹配， IllegalTarget 或者 TargetUndefined（I开头容易和l混淆）
                                         // PLAT_ALARM_TYPE_TARGET_UNDEFINED 需要修改为 PLAT_ALARM_TYPE_TARGET_NOT_IN_WHITELIST
        PLAT_ALARM_TYPE_CDD,//人群密度检测报警
        PLAT_ALARM_TYPE_IPD,//人员入侵侦测报警
        PLAT_ALARM_TYPE_CHANNEL_OFFLINE,//监控点下线报警
        PLAT_ALARM_TYPE_FACE_MATCH, //人脸比对报警
        PLAT_ALARM_TYPE_BLACKLIST_CAR_PASS, //黑名单车辆通行报警   21
        PLAT_ALARM_TYPE_AOIENTRY,//进入区域
        PLAT_ALARM_TYPE_AOILEAVE,//离开区域
        PLAT_ALARM_TYPE_SENCE_CHANGE,   // 场景变更（视频遮挡）
        PLAT_ALARM_TYPE_CAR_PASS_NOBLACKLIST,//白名单车辆的通行报警
        PLAT_ALARM_TYPE_CAR_PASS_TEMPCAR,//临时车辆的通行报警
        PLAT_ALARM_TYPE_TEMP_CAR_PARKING_OVERTIME,//临时车辆超时停放报警
        PLAT_ALARM_TYPE_INDUSTRY_THERMALEX_HIGH_WARNING,//工业相机高温预警
        PLAT_ALARM_TYPE_INDUSTRY_THERMALEX_HIGH_ALARM,//工业相机高温报警
        PLAT_ALARM_TYPE_STRANGER_MATCHING,          // 人脸布控：陌生人
        PLAT_ALARM_TYPE_BLACKLIST_MATCHING,         // 人脸布控：黑名单匹配
        PLAT_ALARM_TYPE_INDUSTRY_THERMALEX_LOW_WARNING,//工业相机低温预警
        PLAT_ALARM_TYPE_INDUSTRY_THERMALEX_LOW_ALARM,//工业相机低温报警
        PLAT_ALARM_TYPE_REGION_STATISTICS,  //区域统计侦测告警
        PLAT_ALARM_TYPE_VISITOR_MATCHING,           // 人脸布控：访客匹配
        PLAT_ALARM_TYPE_VIP_MATCHING,               // 人脸布控：VIP匹配
        PLAT_ALARM_TYPE_WHITELIST_MATCHING,         // 人脸布控：白名单匹配
        PLAT_ALARM_TYPE_FALLING,                    //高空抛物
        PLAT_ALARM_TYPE_SMART_EA,                   //电梯电动车告警
        PLAT_ALARM_TYPE_TEMP_ALARM,                 //工业相机温度报警（新）
        PLAT_ALARM_TYPE_FIRE,                       //火点检测告警
        PLAT_ALARM_TYPE_BLACKLIST_CAR_VERITY,       //车辆布控：车辆比对-黑名单车辆报警
        PLAT_ALARM_TYPE_TAMPER,                     //门禁防拆报警
        PLAT_ALARM_TYPE_DOORCONTACT,                //门禁门磁报警
        PLAT_ALARM_TYPE_STOREGE_REQUESTSTREM_ERR,   //存储通道取视频流失败报警
        PLAT_ALARM_TYPE_VIDEO_COVER,                // 视频遮挡（Onvif+HK）
        PLAT_ALARM_TYPE_OBJECT_STAY,                // 物品滞留
        PLAT_ALARM_TYPE_OFFDUTY_OR_SLEEP,           // 脱岗睡岗
        PLAT_ALARM_TYPE_ILLEGAL_PARK,               // 车辆违停
        PLAT_ALARM_TYPE_GATHER,                     // 局部聚众
        PLAT_ALARM_TYPE_SMOKE,                      // 抽烟
        PLAT_ALARM_TYPE_NOT_HELMET,                 // 未带安全帽
        PLAT_ALARM_TYPE_PARKING_LOT_FULL,           // 停车场-满位报警
        PLAT_ALARM_TYPE_LOITER,                     // 人员徘徊
        PLAT_ALARM_TYPE_ASD,                        // 音频异常
        PLAT_ALARM_TYPE_LOCAL_SENSOR,               // 传感器本地报警输入
        PLAT_ALARM_TYPE_OVERMAN,                    //双目超员报警
        PLAT_ALARM_TYPE_REVERSE,                    //双目逆行报警
        PLAT_ALARM_TYPE_CHANNEL_END,

        //设备报警
        PLAT_ALARM_TYPE_DEVICE_BEGIN = 0x41,
        PLAT_ALARM_TYPE_EXCEPTION = PLAT_ALARM_TYPE_DEVICE_BEGIN,
        PLAT_ALARM_TYPE_IP_CONFLICT,   /////IP地址冲突
        PLAT_ALARM_TYPE_DISK_IO_ERROR, /////磁盘IO错误
        PLAT_ALARM_TYPE_DEV_DISK_FULL,     /////磁盘满
        PLAT_ALARM_TYPE_RAID_SUBHEALTH, //阵列亚健康
        PLAT_ALARM_TYPE_RAID_UNAVAILABLE, //阵列不可用 
        PLAT_ALARM_TYPE_ILLEIGAL_ACCESS,  /////非法访问
        PLAT_ALARM_TYPE_NET_DISCONNECT,  /////网络断开
        PLAT_ALARM_TYPE_DEV_NO_DISK,        ////盘组下没有磁盘
        PLAT_ALARM_TYPE_SIGNAL_SHELTER, //信号遮挡
        PLAT_ALARM_TYPE_HDD_PULL_OUT, //前面板硬盘拔出
        PLAT_ALARM_TYPE_DEVICE_OFFLINE,//设备下线报警
        PLAT_ALARM_TYPE_RAID_HOT_ERROR,//热备错误
        PLAT_ALARM_TYPE_DEV_RAID_EXCEPTION,//阵列异常
        PLAT_ALARM_TYPE_ANTI_DEMOLITION, //设备防拆报警
        PLAT_ALARM_TYPE_COMBINATION, //组合报警
        PLAT_ALARM_TYPE_DEVICE_END,

        PLAT_ALARM_TYPE_ALARM_OUT = 0x51,  /////报警输出的类型，报警输出也有状态需要管理

        //针对报警主机
        /////第三方告警服务器上报的告警，子系统：布防/撤防           防区：告警/消除告警， 旁路/旁路恢复
        /////第三方告警服务器反控操作，  子系统：布防/撤防/消除告警  防区：旁路，旁路恢复

        PLAT_ALARM_TYPE_SUBSYSTEM_BEGIN = 0x61,
        PLAT_ALARM_TYPE_ARM = PLAT_ALARM_TYPE_SUBSYSTEM_BEGIN, /////布防 子系统
        PLAT_ALARM_TYPE_DISARM,             /////撤防 子系统
        PLAT_ALARM_TYPE_CLEAR_ALARM,    /////消警 子系统
        PLAT_ALARM_TYPE_SUBSYSTEM_END = PLAT_ALARM_TYPE_CLEAR_ALARM,
        PLAT_ALARM_TYPE_ZONE_BEGIN,
        PLAT_ALARM_TYPE_BYPASS = PLAT_ALARM_TYPE_ZONE_BEGIN, /////旁路 防区
        PLAT_ALARM_TYPE_BYPASSRES,          /////旁路恢复 防区
        PLAT_ALARM_TYPE_ZONE_ALARM,         /////防区报警 防区
        PLAT_ALARM_TYPE_ZONE_ALARMRES,          /////防区报警恢复 防区
        PLAT_ALARM_TYPE_ALARMHOST_OFFLINE,          /////报警主机下线
        PLAT_ALARM_TYPE_ZONE_ARM,               //防区布防
        PLAT_ALARM_TYPE_ZONE_DISARM,                //防区撤防
        PLAT_ALARM_TYPE_ZONE_END,

        //针对门禁系统报警
        PLAT_ALARM_TYPE_ACSSYSTEM_BEGIN = 0x81,
        PLAT_ALARM_TYPE_ACSDOOR_ALARM,//门禁事件报警
        PLAT_ALARM_TYPE_ACSDEVICE_ATTEND_ALARM,//考勤事件报警
        PLAT_ALARM_TYPE_ACSSYSTEM_OFFLINE,          //门禁系统下线
        PLAT_ALARM_TYPE_ACSSYSTEM_END,

        //针对服务器报警
        PLAT_ALARM_TYPE_SERVER_BEGIN = 0x91,
        PLAT_ALARM_TYPE_SERVER_OFFLINE,//服务器下线报警
        PLAT_ALARM_TYPE_RAID_NOUSE,//无可用阵列
        PLAT_ALARM_TYPE_RAID_EXCEPTION,//阵列异常
        PLAT_ALARM_TYPE_RAID_REBUILD,//阵列重建
        PLAT_ALARM_TYPE_RAID_DEGRADE,//阵列降级Degrade
        PLAT_ALARM_TYPE_NO_DISK,//无存储磁盘
        PLAT_ALARM_TYPE_NO_PARTION,//无分区
        PLAT_ALARM_TYPE_OPENFILE_ERR,//打开文件失败
        PLAT_ALARM_TYPE_ROUTING_CHECK_OFF_LINE,//服务器巡检不在线
        PLAT_ALARM_TYPE_RAID_DISKBAD,//raid-磁盘坏
        PLAT_ALARM_TYPE_DISK_FULL, //磁盘满
        PLAT_ALARM_TYPE_DISK_WILL_FULL, //磁盘快满
        PLAT_ALARM_TYPE_SERVER_CPU_HIGH,//服务器设备cpu高
        PLAT_ALARM_TYPE_DISK_WORK_MODE, // 服务器切换到磁盘模式
        PLAT_ALARM_TYPE_RAID_WORK_MODE, // 服务器切换到阵列模式
        PLAT_ALARM_TYPE_SERVER_END,

        //针对解码设备报警
        PLAT_ALARM_TYPE_DECODER_BEGIN = 0xA1,
        PLAT_ALARM_TYPE_DECODER_OFFLINE,//解码设备下线报警

        //针对服务器报警2
        PLAT_ALARM_TYPE_SERVER_ANOTHER_BEGIN = 0xB1,
        PLAT_ALARM_TYPE_SERVER_DISK_IO_ERROR,   //存储服务 磁盘IO错误
        PLAT_ALARM_TYPE_GROUP_NO_PARTIONGROUP,  // 服务器-存储组失效
        PLAT_ALARM_TYPE_OVER_BINDWIDTH,        // 服务器-带宽超限
        PLAT_ALARM_TYPE_INTELLDISK_DOWN,        // 服务器-智能磁盘掉线
        PLAT_ALARM_TYPE_STORDISK_DOWN,        // 服务器-磁盘模式存储磁盘掉线
        PLAT_ALARM_TYPE_SERVER_TIMEJUMP,        // 存储服务 时间跳变
        PLAT_ALARM_TYPE_SERVER_ANOTHER_END,

        //针对智能告警（智能告警也属于监控点告警）
        PLAT_ALARM_TYPE_INTELLIGENT_BEGIN = 0xDD,
        PLAT_ALARM_TYPE_SUSPECTED_FEVER,             //疑似发热Suspected fever
        PLAT_ALARM_TYPE_UNMASK,             //未戴口罩
        PLAT_ALARM_TYPE_INDUSTRY_THERMALEX_ABNORMAL_RISE,//工业测温-温度增长异常
        PLAT_ALARM_TYPE_SUSPECTED_HYPOTHERMIA,      //疑似低温
        PLAT_ALARM_TYPE_INTELLIGENT_END,

        //PLAT_ALARM_TYPE_END = 0xFF,  ////不能超过这个值 
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_BEGIN = 0x100,  //泰科KANTECH门禁报警新增,对应ENUM字典从201-232之间的报警
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_SECOND_ENTRANCE,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_UNLOCK,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_GRANTED,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_CARD_UNKNOWN,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_BADCARD,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_CARD_LOSTSTOLEN,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_CARD_EXPIRED,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_CARD_PENDING,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_CARD_COUNT,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_SCHEDULE_EXCEPTION,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_BAD_ACCESS,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_SUPLEVEL_REQUIRED,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_PASSBACKBAD_LOCATION,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_DUAL_CUSTODY,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_NUMBER_AREA,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_ALARMSYSTEM_ARMED,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_AREA_NOTCLEAR,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_INTERLOCK_ACTIVE,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_DOORMANUALLY_DISABLED,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_NOTENOUGH_CARDS,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_CARD_BUSY,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_MINPASSBACK_DELAY,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_PASSBACK_BADLOCATION,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_AREA,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_WAIT_APPROVAL,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_READER_LOCKED,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_SECOND_CARD,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_DOOR_ARMED,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_ACCESS_DENIED,
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_DOOR_FORCED_OPEN,//门-强制打开
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_DOOR_FORCED_OPEN_RESTORED,//门-强制打开已恢复
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_PREALARM_DOOR_OPEN_TOOLONG,//门-预警门打开时间太长
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_DOOR_OPEN_TOOLONG,//门-门打开时间太长
                                                            //start 安森 大部分类型是门禁系统报警(即无法具体到门)
        PLAT_ALARM_TYPE_ANSON_ALARM_DOOR_OPEN,//Anson开门报警.2.1.3引入, 门报警
        PLAT_ALARM_TYPE_ANSON_RELAY,                 //继电器开关事件 门报警
        PLAT_ALARM_TYPE_ANSON_WING_TIMEOUT,    //门开超时报警 门报警
                                               //以下是门禁系统报警
        PLAT_ALARM_TYPE_ANSON_BREAK_INTO,          //破门而入（隐含门磁输入有信号不再重复发送）废弃-改用：门-强制打开
        PLAT_ALARM_TYPE_ANSON_LEGAL_CARD,    //合法卡
        PLAT_ALARM_TYPE_ANSON_ILLEGAL_CARD,    //非法卡
        PLAT_ALARM_TYPE_ANSON_LEGAL_PASSWORD,         //密码开门
        PLAT_ALARM_TYPE_ANSON_ILLEGAL_PASSWORD,       //非法密码
        PLAT_ALARM_TYPE_ANSON_SWITCH,                 //出门按钮
        PLAT_ALARM_TYPE_ANSON_PREVENT_BACK,          //卡被防潜回
        PLAT_ALARM_TYPE_ANSON_LEGAL_FP,              //合法指纹
        PLAT_ALARM_TYPE_ANSON_ILLEGAL_FP,            //非法指纹
        PLAT_ALARM_TYPE_ANSON_LEGAL_FACE,            //合法人脸
        PLAT_ALARM_TYPE_ANSON_ILLEGAL_FACE,          //非法人脸
        PLAT_ALARM_TYPE_ANSON_ABNORMAL_BT,           //非正常体温，相当于非法人脸，不过是因为温度不正常导致
        PLAT_ALARM_TYPE_ANSON_ALARM_INPUT,         //报警输入
        PLAT_ALARM_TYPE_ANSON_ALARM_INPUT_EX,      //扩展板报警输入
        PLAT_ALARM_TYPE_ANSON_TAMPER,              //一体机被拆除
                                                   //end 安森 门禁系统报警
                                                   //以下可作为通用门禁类型,门报警, 尽可能复用
        PLAT_ALARM_TYPE_DOOR_KEY,               //钥匙开门
        PLAT_ALARM_TYPE_DOOR_REMOTE,            //远程开门
        PLAT_ALARM_TYPE_DOOR_BLACKLIST,         //黑名单事件
        PLAT_ALARM_TYPE_DOOR_COERCION,          //胁迫开门
        PLAT_ALARM_TYPE_DOOR_NORMAL_OPEN,       //正常开门
        PLAT_ALARM_TYPE_DOOR_NORMAL_CLOSE,      //正常关门
        PLAT_ALARM_TYPE_DOOR_ABNORMAL_OPEN,     //异常开门
        PLAT_ALARM_TYPE_DOOR_ABNORMAL_CLOSE,    //异常关门
        PLAT_ALARM_TYPE_DOOR_LEGAL_QRCODE,      //合法二维码
        PLAT_ALARM_TYPE_DOOR_ILLEGAL_QRCODE,    //非法二维码
        PLAT_ALARM_TYPE_DOOR_LEGAL_PERSON_VERIFICTION,  //人证合法开门
        PLAT_ALARM_TYPE_DOOR_ILLEGAL_PERSON_VERIFICTION,//人证非法开门
        PLAT_ALARM_TYPE_DOOR_TAMPER,            //拆卸报警
        PLAT_ALARM_TYPE_DOOR_OFFLINE,           //门下线
        PLAT_ALARM_TYPE_DOOR_SWITCH,            //按钮开门
        PLAT_ALARM_TYPE_DOOR_LEGAL_CARD,        //合法刷卡
        PLAT_ALARM_TYPE_DOOR_LEGAL_FACE,        //合法人脸
        PLAT_ALARM_TYPE_DOOR_ILLEGAL_FACE,      //非法人脸
        PLAT_ALARM_TYPE_DOOR_LEGAL_PERSONNEL_NUM_AND_PWD,   //合法人员编号+密码
        PLAT_ALARM_TYPE_DOOR_ILLEGAL_PERSONNEL_NUM_AND_PWD, //非法人员编号+密码
        PLAT_ALARM_TYPE_DOOR_LEGAL_PASSWORD,        //密码开门
        PLAT_ALARM_TYPE_DOOR_ILLEGAL_PASSWORD,      //非法密码
        PLAT_ALARM_TYPE_DOOR_LEGAL_FP,              //合法指纹
        PLAT_ALARM_TYPE_DOOR_ILLEGAL_FP,            //非法指纹
        PLAT_ALARM_TYPE_DOOR_EXTERNAL_ALARM,        //外部报警（火警等）
                                                    //CEM新增
        PLAT_ALARM_TYPE_DOOR_CARD_VALID,            //有效卡
        PLAT_ALARM_TYPE_DOOR_CARD_VALID_UNUSED,     //有效卡未开门
        PLAT_ALARM_TYPE_DOOR_CARD_PARKED,           //卡已暂停
        PLAT_ALARM_TYPE_DOOR_CARD_NOT_IN_SYSTEM,    //卡不在系统
        PLAT_ALARM_TYPE_DOOR_CARD_WRONG_ZONE,       //卡错误通行区域
        PLAT_ALARM_TYPE_DOOR_CARD_PASSBACK_FAILURE, //卡返传失败
        PLAT_ALARM_TYPE_DOOR_CARD_WRONG_TIMEZONE,   //卡错误时间区域
        PLAT_ALARM_TYPE_DOOR_CARD_RETRY_PIN,        //卡重试密码
        PLAT_ALARM_TYPE_DOOR_CARD_EXPIRING,         //卡将过期
        PLAT_ALARM_TYPE_DOOR_CARD_SPECIAL_VALID,    //特殊卡有效
        PLAT_ALARM_TYPE_DOOR_HELD,                  //门保持打开
        PLAT_ALARM_TYPE_DOOR_PIN_ALARM,             //密码错误报警
        PLAT_ALARM_TYPE_KANTECHDOOR_ALARM_END,

        PLAT_ALARM_TYPE_ALARM_TASK_BEGIN = 0x200,
        PLAT_ALARM_TYPE_LEAVE_POST,//离岗报警
        PLAT_ALARM_TYPE_ALARM_TASK_ENTER_HIGH_COUNT,//阈值报警
        PLAT_ALARM_TYPE_ALARM_TASK_END,

        //消防语音广播系统
        PLAT_ALARM_TYPE_FIRE_BROADCAST_BEGIN = 0x230,
        PLAT_ALARM_TYPE_FIRE_BROADCAST_OFFLINE, //消防语音广播系统下线
        PLAT_ALARM_TYPE_FIRE_BROADCAST_END,

        //针对门禁设备告警
        PLAT_ALARM_TYPE_ACSDEVICE_BEGIN = 0x300,
        PLAT_ALARM_TYPE_ACSDEVICE_OFFLINE,      //门禁设备下线
        PLAT_ALARM_TYPE_ACSDEVICE_TAMPER,       //门禁设备-拆卸告警
        PLAT_ALARM_TYPE_ACSDEVICE_END,

        // 预留的定制算法报警类型
        PLAT_ALARM_TYPE_CUSTOM_AI_BEGIN = 0x400,
        PLAT_ALARM_TYPE_CUSTOM_AI_END = 0xC00,

        PLAT_ALARM_TYPE_RANGE_END
    };




    [StructLayout(LayoutKind.Explicit, Size = 4)]
    public struct Union
    {
        [FieldOffset(0)]
        public IntPtr pPicData;
        [FieldOffset(0)]
        public ulong FaultTolerance;
    }
    //人脸比对报警状态信息
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct _ALARM_STATE_FACE_MATCH
    {
        public FILETIME frameTime;   //帧时间
        public uint dwRealFaceID;  //抓拍人脸ID
        public uint dwGrpID;       //特征组ID
        public uint dwLibFaceID;   //特征库人脸ID
        public uint dwHandle;      //抓拍人脸数据图片长度
        public uint dwSimilar;     //相似度
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 32)]
        public byte[] byName; //姓名
        public Guid ChannelID;         //抓拍通道ID
        public byte bHasData;		//是否真有数据

        public Union un;
    }
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_AlarmInfo
    {
        public UInt32 ulNodeType;       //节点类型
        public UInt32 ulNodeID;			////节点ID
        public Guid guid;
        public short byAlarmType;		//报警类型
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 1)]
        public byte[] byReserve;        //保留
        public byte byisAlarm;      ///非0是报警状态，0是非报警状态
                                    ///
        public uint tTime;//转发或者接入接收到报警的utc时间
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 5)]
        public byte[] szCID;//接入服务器接入报警主机用到
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
        public byte[] szRes;//保留
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 2976)]
        public byte[] szAlarmContent;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_AlarmInfoEx
    {
        public Plat_AlarmInfo stAlarmInfo;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 200 * 1024)]
        public byte[] picData;//报警图片
        public UInt32 picRealLen;//实际长度	 
    };

    public enum Plat_LanguageType
    {
        LANGUAGE_BEGIN = 0,
        LANGUAGE_ENGLISH,                               // English
        LANGUAGE_CHINESE,                               // Chinese
        LANGUAGE_END
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct Plat_NewAlarmInfo_V2
    {
        public UInt32 ulNodeType;
        public UInt32 ulNodeID;
        public Guid guid;
        public short byAlarmType;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 1)]
        public byte[] byReserve;
        public byte byisAlarm;                      // A non-zero value indicates an alarm state, while 0 indicates a non-alarm state
        public uint tTime;                          // Alarm Time
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 5)]
        public byte[] szCID;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
        public byte[] szRes;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 2976)]
        public byte[] szAlarmContent;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 200 * 1024)]
        public byte[] picData;                      // Alarm picture
        public UInt32 picRealLen;                   // Picture data length

        // If there is no Chinese translation, should be filled with English.
        // If there is also no English translation, then alarmNameLength should be 0.
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256)]
        public byte[] alarmNameContent;             // Content of the translation
        public uint alarmNameLength;                // Length of the translation content
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1024)]
        public byte[] reserved;                     // Reserved field
    }

    //////////////////////////////////////////////////////////////////////////
    //stream

    public enum enPlat_PacketType
    {

        PACTET_TYPE_VIDEO = 1,
        PACTET_TYPE_AUDIO,
    };

    public enum enPlat_CodecType
    {
        CODEC_VIDEO_H264 = 1,    //标准H264
        CODEC_VIDEO_H265,      //标准H265
        CODEC_VIDEO_MJPEG,     //标准MJPEG
        CODEC_VIDEO_HIK,       //HIK流
        CODEC_VIDEO_DAHUA,     //大华流 

        CODEC_AUDIO_PCM = 100, //标准PCM
        CODEC_AUDIO_G711a,  //标准G711 A
        CODEC_AUDIO_G711u,  //标准G711 U
        CODEC_AUDIO_G722,   //标准G722
        CODEC_AUDIO_G729,   //标准G729
        CODEC_AUDIO_MP3,    //mp3
        CODEC_AUDIO_AAC,    //aac
        CODEC_AUDIO_HI_ADPCM, //海思Adpcm
        CODEC_AUDIO_HI_G711a, //海思G711a
        CODEC_AUDIO_HI_G711u, //海思G711U
    };

    /*Frame 包标识*/
    [StructLayout(LayoutKind.Explicit, Pack = 4)]
    public struct Video_Audio
    {
        [FieldOffset(0)]
        public bool isKeyFrame; //I P帧
        [FieldOffset(4)]
        public Int32 nWidth;
        [FieldOffset(8)]
        public Int32 nHeight;

        [FieldOffset(0)]
        public byte byChannels;//声道数  单声道或双（立体）声道
        [FieldOffset(4)]
        public Int32 FrameRate;
        [FieldOffset(8)]
        public Int32 nBand;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct PlatFrameHeader
    {
        public Int32 nStructSize;
        public UInt32 chlNodeID;  //通道ID
        public Int32 nPacketType; //音频、视频
        public Int32 nEncodeType; //编码类型， H264. G711、
        public Int64 ulTimestamp;  //时间戳，毫秒
        public UInt32 ulFrameIndex; //当前帧索引号
        public Video_Audio video_audio;
    };

    public enum enPlat_Face_Match_Opt
    {
        //目标组信息由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_UPDATE_ALBUM_LIST数据为FaceMatch_MsgResult+目标组1+目标组2+......
        FACE_MATCH_UPDATE_ALBUM_LIST = 0x01, // 目标组list刷新通知

        //lpInBuffer=智能分析服务器ID（MSGTYPE_AISERVER通知获取的）
        //dwInBufferSize=sizeof(Int32)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由FACE_MATCH_UPDATE_ALBUM_LIST返回
        FACE_MATCH_GET_ALBUM_LIST = 0x02, // 获取目标组list

        //lpInBuffer=FaceMatch_AlbumInfo
        //dwInBufferSize=sizeof(FaceMatch_AlbumInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_ADD_ALBUM，数据为FaceMatch_MsgResult
        FACE_MATCH_ADD_ALBUM = 0x03, // 增加目标组 

        //lpInBuffer=FaceMatch_AlbumInfo
        //dwInBufferSize=sizeof(FaceMatch_AlbumInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_SET_ALBUM，数据为FaceMatch_MsgResult
        FACE_MATCH_SET_ALBUM = 0x04, // 编辑目标组

        //lpInBuffer=FaceMatch_AlbumInfo
        //dwInBufferSize=sizeof(FaceMatch_AlbumInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_DEL_ALBUM，数据为FaceMatch_MsgResult
        FACE_MATCH_DEL_ALBUM = 0x05, // 删除目标组

        //目标信息由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_UPDATE_TARGET_LIST数据为FaceMatch_MsgResult+FaceMatch_TargetListPageInfo+目标1+目标2+......
        FACE_MATCH_UPDATE_TARGET_LIST = 0x06, // 目标list刷新通知

        //lpInBuffer=FaceMatch_TargetListPageInfo
        //dwInBufferSize=sizeof(FaceMatch_TargetListPageInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由FACE_MATCH_UPDATE_TARGET_LIST返回
        FACE_MATCH_GET_TARGET_LIST = 0x07, // 获取目标list

        //lpInBuffer=FaceMatch_TargetInfo
        //dwInBufferSize=sizeof(FaceMatch_TargetInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_ADD_TARGET，数据为FaceMatch_MsgResult
        FACE_MATCH_ADD_TARGET = 0x08, // 增加目标

        //lpInBuffer=FaceMatch_TargetInfo
        //dwInBufferSize=sizeof(FaceMatch_TargetInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_SET_TARGET，数据为FaceMatch_MsgResult
        FACE_MATCH_SET_TARGET = 0x09, // 编辑目标

        //lpInBuffer=FaceMatch_TargetInfo
        //dwInBufferSize=sizeof(FaceMatch_TargetInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_DEL_TARGET，数据为FaceMatch_MsgResult
        FACE_MATCH_DEL_TARGET = 0x0A, // 删除目标

        //lpInBuffer=FaceMatch_AlbumInfo
        //dwInBufferSize=sizeof(FaceMatch_AlbumInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_CLEAR_TARGET，数据为FaceMatch_MsgResult
        FACE_MATCH_CLEAR_TARGET = 0x0B, // 清空目标

        //lpInBuffer=FaceMatch_TargetInfo
        //dwInBufferSize=sizeof(FaceMatch_TargetInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_GET_TARGET_IMAGE，数据为FaceMatch_MsgResult+FaceMatch_TargetInfo
        FACE_MATCH_GET_TARGET_IMAGE = 0x0C, // 获取目标图片

        //lpInBuffer=FaceMatch_SearchImagePageInfo
        //dwInBufferSize=sizeof(FaceMatch_SearchImagePageInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_SEARCH_IMAGE_BY_IMAGE，
        //数据为FaceMatch_MsgResult+FaceMatch_SearchImagePageInfoResult+图片信息1(FaceMatch_MatchedImageInfo)+图片信息2+......
        FACE_MATCH_SEARCH_IMAGE_BY_IMAGE = 0x0D, // 以图搜图

        //lpInBuffer=FaceMatch_MatchedImageInfo
        //dwInBufferSize=sizeof(FaceMatch_MatchedImageInfo)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_GET_IMAGE，
        //数据为FaceMatch_MsgResult+图片信息(FaceMatch_MatchedImageInfo)
        FACE_MATCH_GET_IMAGE = 0x0E, // 根据以图搜图结果来获取一张图片

        //lpInBuffer=FaceMatch_AlarmNotifyData
        //dwInBufferSize=sizeof(FaceMatch_AlarmNotifyData)
        //lpOutBuffer=IntPtr.Zero
        //dwOutBufferSize=0
        //结果由fMessageCallback回调出来，类型MSGTYPE_FACE_MATCH，FaceMatch_MsgResult.nOptCmd==FACE_MATCH_ALARM_NOTIFY，
        //数据为FaceMatch_MsgResult+ FaceMatch_AlarmNotifyData + 抓图数据(FaceMatch_TargetImageData) + 样本图数据(FaceMatch_TargetImageData)
        FACE_MATCH_ALARM_NOTIFY = 0x0F, // 人脸比对报警通知
        FACE_MATCH_SEARCH_IMAGE_BY_IMAGE_ALLNVR = 0x10, // 以图搜图,搜索所有NVR节点

        GET_ATTENDANCE_RECORD_BRIEF = 0x11,//获取人脸考勤结果
        GET_BATCH_ATTENDANCE_RECORD_BRIEF = 0x12,//
        GET_ATTENDANCE_RECORD_DETAIL = 0x13,
        GET_CPC_PASSENGERS = 0x14,
        GET_UPDATE_PASSENGERS = 0x15,
        FACE_MATCH_ADD_ORG = 0x16,
        FACE_MATCH_DELETE_ORG = 0x17,
        FACE_MATCH_MODIFY_ORG = 0x18,
        FACE_MATCH_GET_ORGINFO = 0x19,
        FACE_MATCH_GET_PEOPLE_TARGET = 0x20,
        FACE_MATCH_GET_VIP_TARGET = 0x21,
        FACE_MATCH_GET_VISITOR_TARGET = 0x22,
        FACE_MATCH_GET_BLACK_TARGET = 0x23,
    };

    enum enPlat_Face_Match_Group_Type
    {
        Face_Match_WHITE_LIST = 0x01, // 白名单
        Face_Match_BLACK_LIST = 0x02, // 黑名单
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_AlbumInfo
    {
        public Int32 nAlbumType;         // 目标库类型 enPlat_Face_Match_Group_Type
        public Guid albumGuid;         // 目标库Guid
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 128)]
        public byte[] szAlbumName;  // 目标库名称
        public Int32 nNvrCount;          // 关联的NVR数量
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 32)]
        public Int32[] nvrId; // 关联的NVR ID
        public Int32 serverId;  // 智能分析服务ID
        public int devType;			// 设备类型，见 AlbumDevTypeEx
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 128)]
        public byte[] resv;//预留
    };
    // 目标库对应的设备类型
    enum AlbumDevTypeEx
    {
        NO_DEVICE_Ex = -1, // 没有设备， 当前目标库没有关联任何设备
        LOCAL_Ex = 0,	// 本地目标库
        HUA_AN_Ex,		// 华安目标库
        SENCE_TIME_Ex,	// 商汤目标库
        TVT_NVR_Ex,	// 同为NVR
        TVT_A2_IPC_Ex,//同为A2 IPC
        THINK_FORCE_Ex, // 熠知人脸平板
        DEVICE_END_Ex = 0xFF,
    };
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_AlbumOperateResult
    {
        public Guid albumGuid;         // 目标库Guid
        public UInt32 nFailedNvrCount;//// 失败的NVR数量
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 32)]
        public UInt32[] failedNvrId; // 失败的NVR ID
    };

    // 目标身份信息
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_TargetProperty
    {
        public byte gender;          // 0--男, 1--女         
        public byte idType;          // ID_TYPE
        public byte priority;        // 优先级， 0--一般  1--中  2--高 (useless)
        public byte spare;           // 补齐
        public ushort birthYear;       // 出生年
        public byte birthMonth;      // 出生月
        public byte birthDay;        // 出生日
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 128)]
        public byte[] name;       // 用户姓名
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] id;          // 证件编号
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] country;     // 国籍
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] province;    // 省份
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] city;        // 城市   
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] discription; // 备注
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
        public byte[] phoneNo; // 手机号码

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] staffNo; // 工号
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 128)]
        public byte[] department; // 部门
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 128)]
        public byte[] job; // 职位
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 100)]
        public byte[] subCompany; // 分公司
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 100)]
        public byte[] projectTeam; // 项目组
        public uint visitStartTime;// 访客有效时间
        public uint visitEndTime; // 访客失效时间
    };

    // 目标图像信息
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_TargetImageData
    {
        public Int32 type;           // 0--jpg（默认）, 1--yuv(useless)
        public UInt32 imgId;  // 图像id 向客户端推送数据时使用
        public Int32 width;          // 图片宽  40*40--8192*8192  useless
        public Int32 height;         // 图片宽  40*40--8192*8192  useless
        public Int32 dataLen;        // 图片长度
        public IntPtr data; // 图片数据指针， 指向目标图像数据
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_TargetInfo
    {
        public FaceMatch_AlbumInfo albumInfo;            // 所属目标库
        public Int32 targetId;                             // 添加目标时-1, 编辑时填值大于0
        public UInt32 nErrorID;	                  // 添加、修改目标失败时，给出的失败原因
        public FaceMatch_TargetProperty targetProperty; // 目标属性
        public FaceMatch_TargetImageData imageData;      // 目标图像
    };
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_TargetOperateResult
    {
        public int targetId;                                    // 目标id
        public FaceMatch_AlbumOperateResult albumOperateResult; // 包含各NVR操作结果
    }

    // 作为返回参数时，该结构体后紧跟着的FaceMatch_TargetInfo数组
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_TargetListPageInfo
    {
        public FaceMatch_AlbumInfo albumInfo;   // 目标库   //发起分页请求时仅需前三个数据
        public UInt32 pageSize;    // 分页大小
        public UInt32 pageIndex;   // 页索引

        public UInt32 totalCount;  // 目标库中目标的总数量
        public UInt32 pageCount;   // 目标库中目标页数， pageCount=totalCount/pageSize+1
        public UInt32 leftCount;   // 待处理目标数量
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_SearchImagePageInfo
    {
        public int pageSize;           // 分页大小
        public int pageIndex;          // 页索引
        public int srvId;              // 智能分析服务
        public int nvrId;
        public int startTime;          // 开始时间， 从抓拍库搜索有效
        public int endTime;            // 结束时间， 从抓拍库搜索有效
        public int threshold;          // 设定阈值
        public int resultCount;        // 返回值数量限制
        public int dataSource;		//0代表从本地服务器搜，1代表从设备端搜
        public FaceMatch_TargetImageData image; // 待搜索图像
    };

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_MatchedImageInfo
    {
        // 智能分析服务
        public UInt32 srvId;

        // 特征值的Guid
        public UInt32 nvrId;
        public Guid albumGuid;
        public uint targetId;

        // 人脸抓图的信息
        public UInt32 frameTime;
        public UInt32 frameUTime;
        public UInt32 imgId;
        public UInt32 similarity;
        public UInt32 grade;
        public Guid chnlGuid;
        public int dataSource;		//0代表从本地服务器搜，1代表从设备端搜
        public FaceMatch_TargetImageData image; // 搜索结果
    };

    // 该结构体后紧跟着FaceMatch_MatchedImageInfo数组
    public struct FaceMatch_SearchImagePageInfoResult
    {
        public byte hasNextPage; // 是否还有下一页  1-has next page;2-no next page
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
        public byte[] byReserve;       //  预留
    };
    public struct tm
    {
        public int tm_sec;     /* seconds after the minute - [0,59] */
        public int tm_min;     /* minutes after the hour - [0,59] */
        public int tm_hour;    /* hours since midnight - [0,23] */
        public int tm_mday;    /* day of the month - [1,31] */
        public int tm_mon;     /* months since January - [0,11] */
        public int tm_year;    /* years since 1900 */
        public int tm_wday;    /* days since Sunday - [0,6] */
        public int tm_yday;    /* days since January 1 - [0,365] */
        public int tm_isdst;   /* daylight savings time flag */
    }
    public struct FaceMatch_AlarmNotifyData
    {
        public UInt32 ulNodeID;//对应Plat_ResNodeInfo中的ulNodeID
        public Guid chnlGuid;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256)]
        public byte[] szChnName;       //  监控点名称 windows是GB2312编码,其他平台是utf8编码
        public int chnlTime;
        public int faceId;//人脸抓拍id
        public int similarity;//需要除10得到百分比
        public int targetId;//目标编号
        public FaceMatch_AlbumInfo albumInfo;//所属目标库
        public FaceMatch_TargetProperty targetProperty;//目标属性
        public FaceMatch_TargetImageData snapedImage;//抓图
        public FaceMatch_TargetImageData targetImage;//样本图

    }

    public struct UpdatePassengerInfo//人数统计
    {
        public Guid chalGuid;
        public uint time;
        public uint crossInCount;       // ipc上报总人数 
        public uint crossOutCount;      // ipc 上报总人数
        public uint addInPersonCount;   // 新进入人数
        public uint addOutPersonCount;  // 新离开人数

        public uint crossInCountCar;    // ipc上报总车浪数
        public uint crossOutCountCar;   // ipc上报总车辆数
        public uint addInCarCount;      // 新进入车辆数
        public uint addOutCarCount;     // 新离开车辆数

        public uint crossInCountMotor;  // ipc上报总非机动车数
        public uint crossOutCountMotor; // ipc上报总非机动车数
        public uint addInMotorCount;    // 新进入非机动车数
        public uint addOutMotorCount;   // 新离开非机动车数
    }
    public struct PassengerInfo//人数统计
    {
        public uint targetType; //1,person ; 2,car; 4,bike
        public uint time;
        public uint inCount;
        public uint outCount;
        public Guid chlGuid;
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FaceMatch_AttendInfo
    {
        public Guid albumGuid;		// 目标库GUID
        public int targetId;		// 目标id
        public int startTime;
        public int endTime;
        public Guid startChnlGuid;			// 最早时间GUID
        public Guid endChnlGuid;			// 最晚时间GUID
        public Guid serverGuid;//智能分析服务器guid

    }

    //人员类别
    public enum PersonInfoType
    {
        REGISTERED_PERSON = 1, //户籍人员
        FIELD_PERSON, //外地人员
        OVERSEAS_PERSON,//境外人员
    };

    public struct TargetPropertyInfo
    {
        public int gender;      // 0--男, 1--女
        public int idType;      // ID_TYPE
        public int priority;    // 优先级， 0--一般  1--中  2--高 (useless)
        public int spare;       // 补齐

        //
        public int birthYear; // 出生年
        public int birthMonth;// 出生月
        public int birthDay;  // 出生日

        // @warning 访客等级、办卡年-月-日 可用于迎宾及访客， 访客这些字段记录访客的预约时间， 没有对其 wtf
        public int VIPLevel;  // VIP等级
        public int vipYear;   // vip办卡年

        public int vipMonth;  // vip办卡月
        public int vipDay;    // vip办卡日

        // 用户的基本信息
        public byte[] name;       // 用户姓名
        public byte[] id;          // 证件编号
        public byte[] country;     // 国籍
        public byte[] province;    // 省份
        public byte[] city;        // 城市

        // @warning 考勤用以下字段，工号、分公司、部门、项目组这样的层级结构， 由于2.1版本中没有处理分公司、项目组，故从备用字段中抽取出部门和项目组
        public byte[] staffNo;      //工号
        public byte[] department;   // 部门
        public byte[] job;      // 职位
        public byte[] phoneNo;      // 电话号码
        public byte[] nCardNo;		//卡号
        // @warning 备注，用于添加一些备注信息， 不涉及具体的业务，仅作提示用
        public byte[] discription;
        // @warning 2019-10-30添加， 添加员工考勤分公司、项目组字段  2019-10-30添加
        // @ref department, 由于备用字段不足，故分公司、项目组设置为100字节
        public String subCompany;       // 分公司 2019-10-30添加
                                        // @ref department              // 部门   2019-10-30添加
        public String projectTeam;      // 项目组 2019-10-30添加

        // @warning 2019-10-30添加， 添加访客有效时间段字段， 访客预约时间可套用vip时间，
        public int visitStartTime;        // 访客有效时间  2019-10-30添加
        public int visitEndTime;          // 访客失效时间  2019-10-30添加
                                          // 补齐
        public int isAddByModify;
        public int isTargetAdded; // 2019-01-10添加

        // @warning 2020-11-13添加：智慧社区
        PersonInfoType personType;//人员类别
        public int credentialType;//证件类型
        public int nation;//民族
        public int origin;//籍贯
        public int domicile;//户籍地行政区划
        public int streetCode;//户籍所在街道
        public int domicileRoadCode;//户籍地路名
        public String domicileAddress;//户籍地详址
        public int residence;//居住地行政区划
        public int residenceStreetCode;//居住地街道代码
        public int residenceRoadCode;//居住地路名代码
        public String residenceAddress;//居住地址
        public int educationCode;//文化程度代码
        public int maritalStatusCode;//婚姻状况代码
        public String spouseName;//配偶姓名
        public int spouseType;//配偶证件类型
        public String spouseNO;//配偶证件号码
        public int nationalityCode;//国家代码
        public String entryTime;//入境时间
        public String surnameEng;//外文姓
        public String nameEng;//外文名
        public String phoneNoOne;//手机号码 1
        public String phoneNoOnePerson;//手机号码 1 归属人
        public int phoneNoOnePersonType;//手机号码1归属人证件类型
        public String phoneNoOnePersonID;//手机号码1归属人证件号码
        public String phoneNoTwo;//手机号码 2
        public String phoneNoTwoPerson;//手机号码 2 归属人
        public int phoneNoTwoPersonType;//手机号码2归属人证件类型
        public String phoneNoTwoPersonID;//手机号码2归属人证件号码
        public String phoneNoThree;//手机号码 3
        public String phoneNoThreePerson;//手机号码 3 归属人
        public int phoneNoThreePersonType;//手机号码3归属人证件类型
        public String phoneNoThreePersonID;//手机号码3归属人证件号码
        public String idCardPicUrl;//证件照
        public int source;//数据来源
        public String rowTime;//新增/更新时间
    }
    public enum _ORG_TYPE
    {
        ORG_PEOPLE = 0,//人员管理类型
        ORG_VIP,//VIP
        ORG_VISITOR,//访客
        ORG_BLACK,//黑名单
        ORG_OTHER//其他
    };
    public struct Plat_TargetInfo
    {
        //人脸属性
        public TargetPropertyInfo targetProperty;
        //组织ID
        public Guid orgGuid;
        //目标库ID
        public int targetId;
        //图片数量一般是1
        public uint imageCount;
        //人脸，暂时只一张
        public FaceMatch_TargetImageData Image;
    }

    public struct Plat_TargetPersinInfo
    {
        public Guid orgGuid;
        public int targetId;
    }
    public struct Plat_PermisinInfo
    {
        public int id;                     //权限ID，修改的时候用，
        public String m_strName;      //权限名称
        public int systemAndOperateRright; //勾选权限值的按位或赋值给他
                                           //以下四组map可以参考NVMS关于权限控制对应的check项，
                                           //暂时在sdk demo当中不对以下四组map进行demo上的展示，但是会提供对应的接口
                                           //对接的话可以自己做对应的UI界面

        //区域权限，可参考AccountManageData.h里的THI_AREA_PERMISSION_BEGIN至THI_AREA_PERMISSION_END
        public Dictionary<uint, Int64> areaRright;
        //电视墙权限，可参考AccountManageData.h里的THI_TVWALL_PERMISSION_BEGIN至THI_TVWALL_PERMISSION_END
        public Dictionary<uint, Int64> tvWallRight;
        //区域下通道权限，可参考AccountManageData.h里的THI_AREA_PERMISSION_BEGIN至THI_AREA_PERMISSION_END
        public Dictionary<uint, Int64> chlsInAreaAuthRight;
        //组织管理权限，可参考AccountManageData.h里的THI_ALBUM_PERMISSION_BEGIN至THI_ALBUM_PERMISSION_END
        public Dictionary<uint, Int64> albumRight;
        public String strAuthGroupName;			//添加和修改的时候不需要传递
    }
    public struct Plat_UserInfo
    {
        public bool bUse; //是否启用
        public bool bBindMacAddress; //是否绑定mac
        public String name;
        public String password;
        public String email;
        public String groupName;
        public String macAddress;
        public String remark;
        public int id;
    }

    public enum PlatAuthGroup
    {
        //下面权限是demo进行了例子对接的
        PLAT_SYSTEM_RIGHT_USERRIGHT_CONFIG = 0x0010,    //用户和权限管理配置
        PLAT_SYSTEM_RIGHT_TVWALL_CONFIG = 0x0040,       //电视墙管理
        PLAT_SYSTEM_RIGHT_PMS_CONFIG = 0x1000,          //停车场管理
        PLAT_SYSTEM_RIGHT_STATIC_CONFIG = 0x10000,      //过线统计
        PLAT_SYSTEM_RIGHT_ACCESS_CONFIG = 0x40000,      //门禁
        PLAT_SYSTEM_RIGHT_ORGANIZATION = 0x800000,      //组织管理	
                                                        //下面是demo不进行例子对接的，可自行根据情况对接
        PLAT_SYSTEM_RIGHT_RESOURCE_CONFIG = 0x01,       //资源管理配置
        PLAT_SYSTEM_RIGHT_SERVER_CONFIG = 0x02,
        PLAT_SYSTEM_RIGHT_REC_CONFIG = 0x04,
        PLAT_SYSTEM_RIGHT_ALARM_CONFIG = 0x08,
        PLAT_SYSTEM_RIGHT_EMAP_CONFIG = 0x0020,
        PLAT_SYSTEM_RIGHT_SYSTEM_CONFIG = 0x0080,
        PLAT_SYSTEM_RIGHT_SYSTEM_BACKUP_STORE = 0x0100,
        PLAT_SYSTEM_RIGHT_LOG = 0x0200,
        PLAT_SYSTEM_RIGHT_EMAP_OPERATE = 0x0400,
        PLAT_SYSTEM_RIGHT_TVWALL_OPERATE = 0x0800,
        PLAT_SYSTEM_RIGHT_FACEDEPLOY_CONFIG = 0x2000,
        PLAT_SYSTEM_RIGHT_FACEATTEND_CONFIG = 0x4000,
        PLAT_SYSTEM_RIGHT_GUEST_CONFIG = 0x8000,
        PLAT_SYSTEM_RIGHT_CPC_CONFIG = 0x10000,
        PLAT_SYSTEM_RIGHT_OATTEND_CONFIG = 0x20000,
        PLAT_SYSTEM_RIGHT_ALBUM_OPERATE = 0x80000,
        PLAT_SYSTEM_RIGHT_THERMAL_IMAGE = 0x100000,
        PLAT_SYSTEM_RIGHT_CHLGROUP_ADD_OPERATE = 0x200000,
        PLAT_SYSTEM_RIGHT_CHLGROUP_DEL_OPERATE = 0x400000,

        //区域权限（监控点相关）:云台控制、预览、回放、备份、录像、监控点配置
        PLAT_AREA_RIGHT_PTZ = 0x01,         //云台控制
        PLAT_AREA_RIGHT_LIVE = 0x02,            //预览
        PLAT_AREA_RIGHT_PLAYBACK = 0x04,        //回放
        PLAT_AREA_RIGHT_BACKUP = 0x08,      //备份
        PLAT_AREA_RIGHT_RECORD = 0x10,      //录像
        PLAT_AREA_RIGHT_CHANNEL_CONFIG = 0x20,      //监控点配置
                                                    //区域权限（设备相关）:与设备进行对讲、查看设备日志、设备配置
        PLAT_AREA_RIGHT_TALKBACK = 0x40,            //对讲
        PLAT_AREA_RIGHT_VIEW_LOG = 0x80,            //查看设备日志
        PLAT_AREA_RIGHT_DEVICE_CONFIG = 0x100,          //设备配置
        PLAT_AREA_RIGHT_SOUND = 0x200,          //视频声音
                                                //电视墙子项的权限配置
        PLAT_TVWALL_RIGHT_ITEM = 0x01,
        //目标库子项的权限配置
        PLAT_ALBUM_RIGHT_ADDTARGET = 0x01,
        PLAT_ALBUM_RIGHT_EDITTARGET = 0x02,
        PLAT_ALBUM_RIGHT_DELTARGET = 0x04,
        PLAT_ALBUM_RIGHT_QUERYTARGET = 0x08,
    }

    public enum PlatAuthType
    {
        PLAT_AUTH_SYSTEM = 0,
        PLAT_AUTH_AREA,
        PLAT_AUTH_TVWALL,
        PLAT_AUTH_ALBUM,
    }

    public enum PLAT_IA_DATA_SRC
    {
        PLAT_FROM_NULL = -1,
        PLAT_FROM_LOCAL,
        PLAT_FROM_DEV,
    }

    public struct stPlat_StreamGWUrlInfo
    {
        public byte[] channelName;
        public byte[] deviceName;
        public byte[] RTSPUrl;
        public byte[] RTMPUrl;
        public byte[] HLSUrl;
        public byte[] HTTPFLVUrl;
    }

    public struct Plat_StreamPlayBackParm
    {
        public byte[] szGuid;
        public byte[] szStartTime;         //"2019-05-9 11:02:00"
        public byte[] szEndTime;			//"2019-05-9 11:02:00"
    }

    public struct Plat_StreamPlayBackResponse
    {
        public byte[] szGuid;
        public byte[] RTSPUrl;
        public byte[] RTMPUrl;
        public byte[] HLSUrl;
        public byte[] HTTPFLVUrl;
        public byte[] szStartTime;         //"2019-05-9 11:02:00"
        public byte[] szEndTime;           //"2019-05-9 11:02:00"
        public uint nRecordId;
        public uint errCode;
    }
    public struct Plat_TaretToDeviceInfo
    {
        public int targetId;                   // 人员ID
        public Guid deviceGuid;                // 设备GUID
        public int targetToDeviceType;         // 人员下发状态
        public int targetToDeviceErrorcode;    //错误码// 人员下发错误码
        public int manDeleteType;              // 强制删除标志 1： 强制删除(只有删除人员与设备对应关系命令有效)
    }
    public struct Plat_ResOrgInfo
    {
        public uint ulNodeID;          //组织id  对接权限模块的时候有用
        public Guid guidNodeID;
        public Guid guidParentID;               //上级组织GUID
                                                //sdk2.0 库中已经修改为256
        public byte[] szName;       //windows gb2312编码 其他是utf8
        public uint orgType;           //组织类型
        public byte[] byReserve; ////保留
    }
    public struct Plat_CarInfo
    {
        Guid guid;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] licensePlate;			//车牌号
        public uint vehicleType;        //车辆类型， 1小型车 2大型车 3中型车 4其他  
        public int vehicleColor;                //颜色0-蓝色，1-黑色，2-黄色 3-白，4-绿，5-红 6-灰 7-紫 8-other
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] ownerName;            //姓名
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 32)]
        public byte[] ownerPhone;			//电话

        public uint startTime;         //有效期开始时间
        public uint endTime;           //有效期结束时间

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256)]
        public byte[] describe;         //描述

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] schedule;         //描述

        public int targetId;                   //targetID，暂时不用管
        public int groupParkType;              //参考PLAT_PARK_TYPE，0x1固定车，0x2黑名单车
        Guid chargeRuleGUID;         //规则GUID，对接收费规则时有用
        public Guid vehicleGroupGUID;       //车辆组GUID,黑名单不需要
    }

    public struct Plat_CarGroupInfo
    {
        public Guid guid;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] name;			//车辆组名称
        public int groupParkType;
        public Guid scheduleGuid;
        public Guid parentGuid;
    }
    public struct Plat_PassRecordQueryCondition
    {
        public uint nStartTime;
        public uint nEndTime;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 128)]
        public byte[] szPlateContation;			//车牌号

        public int nParkType;							//停车场类型
        public int vehilceType;                       //车辆类型
        public int nPassMode;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public Guid[] pLineGuid; //
        public int laneRealCount;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public Guid[] pGateWayGuid; //
        public int gateWayRealCount;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public Guid[] pParkGuid; //
        public int nParkCont;
        public int nPageIndex;                        //页索引
        public int nPageSize;                          //页大小
        public int nOprateType;
    }

    public struct Plat_PassRecordItem
    {
        public int logindex;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 48)]
        public byte[] szUserGuid;          //用户GUID

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 48)]
        public byte[] sChargeRuleGuid;            //收费规则GUID

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 48)]
        public byte[] szReduceRuleGuid;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 48)]
        public byte[] szParkGuid;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 48)]
        public byte[] szGateWayGuid;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 48)]
        public byte[] szLineGuid;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256)]
        public byte[] szMemo;

        public int enterTime;                   //进入时间
        public int outTime;                     //离开时间
        public int reduceSum;
        public int vehicleType;
        public int chargeType;
        public int chargeSum;
        public int abnormaPass;
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 128)]
        public byte[] szPlate; //车牌号
    }

    public struct PassRecord_MsgResult
    {
        public int nResult;                 //结果值，1：成功；0：失败
        public int nErrorID;                //错误码
        public int nBufSize;                //pBuffer字节数
    }

    public struct Plat_PmsLogPassRecord
    {
        public int nIndex;        //唯一标识
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] licensePlate;  //车牌号
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] licensePicPath;//图片路径 暂时填充的是监控点GUID
        public int passTime;      //通行utc时间
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] parkGUID;      //停车场GUID
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] gatewayGUID;   //出入口GUID
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] laneGUID;      //车道GUID
        public int laneType;       //车道类型 1入口 2免费出口 3收费出口 |2.1.3 PMS重构后:PmsCommonInfo::LANE_TYPE 1入口；2出口
        public int vehicleType;   //车辆类型 1小型车 2大型车 3其它	 |2.1.3 PMS重构后:PmsCommonInfo::VEHICLE_TYPE 1小型车 2大型车 3中型车 4其他  
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 64)]
        public byte[] userGUID;      //客户端用户GUID			   |2.1.3 PMS重构后:自动放行，未放行:管理服务用户GUID;手动放行:客户端用户GUID
        public int passMode;      //放行方式 1自动通行2手动放行3未通行 |2.1.3 PMS重构后:PmsCommonInfo::CAR_RELEASE_RULE(同)
        public int carColor;      //车牌颜色 0-蓝色 1-黑色 2-黄色 3-白色 4-绿色 5-红色 6-灰色 7-紫色 |2.1.3 PMS重构后:PmsCommonInfo::VEHICLE_COLOR(同)
        public int surveillance;   //停车类型 1,固定车，有效期内，2-公共黑名单车辆，4，纯粹临时车
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 168)]
        public byte[] szMemo;//备用字段
        public int promptInfo;        //给客户端提示信息，不存库	 PmsCommonInfo::PROMPT_INFO |2.1.3新增
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 80)]
        public byte[] passReason;    //放行理由 20个汉字 20*4=80个字节	|2.1.3新增
                                     //TODO：是否加收费金额，迭代3视情况定
        public int parkTime;      //停车时长（秒） 入场记录时，该字段无效；出场记录时，表示 在场停车时长
        public int outPark;       //入场记录时，1-有对应的出场记录|已离场,0-无对应记录|未离场；出场记录时，该字段无效 PmsCommonInfo::IS_OUT_PARK
        public int startTime;     //默认：0；	若为固定车，表示有效开始时间；若为临时车，无效		 不写库，只用于传输
        public int endTime;       //默认：0；	若为固定车，表示有效结束时间；若为临时车，无效		 不写库，只用于传输
        public int nLinkId;       //关联id，出场关联入场id，入场关联出场id  2021.11.10 |2.1.3新增
        public int mask;			//更新标识,标明更新哪些内容    2021.11.10 |2.1.3新增
    }

    public struct Plat_PmsLogPassRecordPic
    {
        public int passTime;      //秒
        public int nMilliseconds;//秒后面的毫秒数

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 256 * 1024)]
        public byte[] picData;

        public int m_bufLen;
    }

    public struct Plat_PmsPassData
    {
        Guid chGuid;                         //通道Guid
        public Plat_PmsLogPassRecord passData;     //通信记录相关信息数据
        public Plat_PmsLogPassRecord passHistoryData;
        public Plat_PmsLogPassRecordPic pictureData;	//图片相关数据
    }
    #endregion 结构体，枚举定义

    public class PlatformSDKHelperEx
    {
        #region 常量
        public const string PLAT_CLIENT_SDK = "\\PlatClientSDK.dll";
        //能力集
        //设备能力集
        public const UInt32 DEVICE_ABILITY_TALK = 0x00000001; //是否支持对讲
        //通道能力集
        public const UInt32 CHANNEL_ABILITY_AUDIO = 0x00000001; //是否支持音频
        public const UInt32 CHANNEL_ABILITY_PTZ = 0x00000002; //是否支持PTZ控制
        public const UInt32 CHANNEL_ABILITY_CURISELINE = 0x00000004; //是否支持巡航线配置
        public const UInt32 CHANNEL_ABILITY_PRESETPOINT = 0x00000008; //是否支持预置点配置
        public const UInt32 CHANNEL_ABILITY_ENCODEINFOCFG = 0x00000010; //是否支持编码器信息配置,视频质量
        public const UInt32 CHANNEL_ABILITY_COLORREGULATE = 0x00000020; //是否支持 色彩调节 亮度信息配置
        public const UInt32 CHANNEL_ABILITY_IMAGEDISPLAY = 0x00000040; // 是否支持图像显示  OSD配置
        public const UInt32 CHANNEL_ABILITY_STREAMTOFILE = 0x00000080; // 流保存成文件比如远程备份、AVI录像 


        #endregion 常量


        #region SDK对外接口
        ////////////////////////////////////////////////////////////////////////////////
        /********************************SDK接口函数声明*********************************/
        //SDK初始化，必须调用
        [DllImport(PLAT_CLIENT_SDK)]
        //public static extern bool Plat_Initialize();
        public static extern bool Plat_InitializeEx(IntPtr pExePath/*有权限写入配置的目录,手机端必传,其他平台传NULL(IntPtr.Zero)*/, Int32 nSpecificType = 0/*默认0，键盘调用传入1*/ );

        //SDK反初始化，必须调用
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_UnInitializeEx();

        /* 登录， 只支持登录一个平台；登录成功会自动通过消息回调函数 直接返回 区域、设备通道信息，并会更新设备状态；
	        * host     : ip地址 或 域名 
	        * port     : 
	        * user     :
	        * password
	        * Return   ： Success, Login Handle， Failed： -1;
	    **/
        //[DllImport(PLAT_CLIENT_SDK)]
        //public static extern int Plat_Login(IntPtr szHost,int nPort,string szUser, string szPassword );
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern int Plat_LoginEx(string szHost, int nPort, string szUser, string szPassword);
        //登出
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_LogOutEx(Int32 lLoginID);

        //设置消息回调函数
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SetMessageCBEx(fMessageCallback MsgCB, IntPtr pUser);

        /************************************************************************/
        /*实时预览相关 Real Stream Preview                                                     */
        /************************************************************************/
        /*Play Channel Real Stream
	    * loginhandle  : Platform Login Handle
	    * chnid        : Channel Node ID
	    * StreamType   : 流类型： 主子流
	    * playwnd      : 播放窗口句柄，NULL：不解码播放；
	    * fRealCB      : 流回调函数，  NULL - 不回调；  
	    * pUser        : UaseData
	    * 
	    * Return       ： Success，Real Stream Handle； Failed: NULL；
	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern ulong Plat_LivePlayEx(Int32 lLoginID, Guid guid, byte nStreamType, IntPtr PlayWnd, fRealStreamCallback fRealCB, IntPtr pUser);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_StopLivePlayEx(ulong lStreamID);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SetDrawFunCBEx(ulong lStreamID, fDrawCallback cbDraw, IntPtr pUser);

        //Talk, 
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern UInt32 Plat_StartTalkEx(Int32 lLoginID, Guid devGuid, fRealStreamCallback cbVoiceCallBack, IntPtr pUser, bool bLocalPlay);

        /*PCM格式流， 长度640字节，bLocalPlay为true时， */
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_TalkSendDataEx(UInt32 ulTalkHandle, ref byte pBuf, UInt32 iLen);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_StopTalkEx(UInt32 ulTalkHandle);

        /*本地录像
 	    *streamID: 实时流ID
 	    *fileName: 文件名称（包括路径）: 
 	    *返回值：ture，成功； false， 失败；
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_StartLocalRecordEx(ulong lStreamID, string szFileName);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_StopLocalRecordEx(ulong lStreamID);

        /* 截图BMP格式
 	    * streamID： 实时流/回放流ID， 
 	    * sPicFileName： 图片文件名；
 	    */
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_CapturePictureEx(ulong lStreamID, string szPicFileName);//bmp   

        /*
 	    * 打开声音
 	    * streamid: 实时流/回放流ID
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_OpenSoundEx(ulong lStreamID);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_CloseSoundEx(ulong lStreamID);


        /* 音量控制
 	    * streamID： 实时流/回放流ID
 	    * Volume   : 音量值， 范围：0－100
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SetVolumeEx(ulong lStreamID, Int32 nVolume);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetVolumeEx(ulong lStreamID, ref Int32 nVolume);

        /* PTZ控制
 	    * StreamID： 实时流ID
 	    * dwPTZCommand: ptz命令
 	    * dwSpeed: Speed, Range: 1－8；，
 	    * 
 	    * 返回值： true: Success； false: failed
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_PtzControlEx(ulong lStreamID, UInt32 dwPTZCommand, UInt32 dwSpeed);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_PtzControl_OtherEx(Int32 lLoginID, Guid guid, UInt32 dwPTZCommand, UInt32 dwSpeed);

        /*
	    *
	    * cmd :  enPlat_PTZ_Control值 
	    * pItem ： ；
	    * dwSpeed: Speed, Range: 1－8;
	    * 固定模式，预置点没有清除功能
	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_PTZPresetInfoEx(Int32 lLoginID, Guid guid, UInt32 nCommand, ref Plat_Preset_Item pItem);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GotoPTZPresetEx(Int32 lLoginID, Guid guid, UInt32 nIndex, Int32 nSpeed);

        /*
 	    * Cruise  
 	    * cmd: Add、Modify、Delete；查
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_PTZCruiseInfoOptEx(int lLoginID, Guid guid, UInt32 nCommand, ref Plat_CruiseItem pItem);
        //开始，停止
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_PTZCruiseEx(int lLoginID, Guid guid, UInt32 nCommand, UInt32 ulCruiseIndex, Int32 nSpeed);

        /************************************************************************/
        /* PlayBack                                                             */
        /************************************************************************/
        // 
        /*Search Record File
    	 *login:      Login Handle
 	     *lChannel:   Channel Node ID
 	    *startTm:    Start Time
 	    *endTm:      结束时间
 	    *Rectype:    录像类型，手动、排程、移动侦测报警，等enPlat_Record_Type;
 	    *isDevSource：   查询目标服务器， false－存储服务器， true－设备端录像
 	    *返回值：   true-success, false-failed
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_RecordSearch_Guid(Int32 lLoginID, Guid guid, Int32 nRectype, Int64 startTm, Int64 endTm, bool isDevSource);

        // 返回值： 非零－句柄， 0，false; 错误码：Plat_GetLastError获取
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern UInt32 Plat_RecordSearchEx_Guid(Int32 lLoginID, Guid guid, Int32 nRectype, Int64 startTm, Int64 endTm, bool isDevSource);

        /*按时间回放,异步接口
    	 * 返回值： 非零－播放句柄， 0，false;
 	     **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern ulong Plat_PlaybackByTimeEx(Int32 lLoginID, Guid guid, Int32 nRecType, Int64 startTm, Int64 endTm, bool isDevSource,
            /*HWND*/IntPtr hwnd, fPlayStreamCallback cbPlayStreamCB, IntPtr pUser);

        //WAITMODE
        //同步接口, 
        // 返回值： 非零－播放句柄， 0，false; 错误码：Plat_GetLastError获取
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern ulong Plat_PlaybackByTimeForWaitResultEx(Int32 lLoginID, Guid guid, Int32 nRecType, Int64 startTm, Int64 endTm, bool isDevSource,
            /*HWND*/IntPtr hwnd, fPlayStreamCallback cbPlayStreamCB, IntPtr pUser);

        /*停止回放
 	    * ulReplayID ： 回放句柄
 	    * Return： true-success, false-failed
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_StopPlaybackEx(ulong ulPlaybackID);

        /* 回放操作, 暂停、快进（ 1 2 4 8。16.32倍）、快退（-1 -2 -4 -8 -16 -32倍）－没有、恢复、 慢放, 
 	     * ulReplayHandle ： 回放句柄
 	    * Return： 
 	    * 在回入hwnd不为NULL时，
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_PlaybackControlEx(ulong ulPlaybackID, UInt32 nCommand, Int32 lParam);
        //回放的其它操作
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetPlaybackOsdTimeEx(ulong ulPlaybackID, ref UInt64 OsdTime/*单位毫秒*/);//获取录像回放时显示的OSD时间

        /* 下载录像
 	    *Return: >0 - success,下载句柄； 0 :Failed;
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern UInt32 Plat_StartDownloadByTimeEx(ulong lLoginID, Guid guid, Int32 nRecType, Int64 startTm, Int64 endTm, bool isDevSource, string szSavedFilePath);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_StopDownloadEx(ulong ulDownloadID);

        /* 获取当前下载进度l
 	    *Return: >0 - success,进度值0－100；
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern UInt32 Plat_GetDownloadPosEx(ulong ulDownloadID);

        //录像标签操作，标签保存在本地。
        //lChlNodeID: 通道NodeID
        // szLabelName: 标签名称,最长不能超256；
        // nLen: szLabelName的长度
        // labelTime:  时间值，1970-1-1到现在的秒数；
        // 返回值: false:失败；true:成功
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_AddRecordLabelEx(Int32 lLoginID, Guid guid, string szLabelName, Int32 nLen, Int64 labelTime);//同步
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_DelRecordLabelEx(Int32 lLoginID, Guid guid, string szLabelName, Int32 nLen);//同步
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetRecordLabelEx(Int32 lLoginID, Guid guid);   // chlnode 为 0， 获取所有标签，//异步


        // Remote 录像标签操作,标签保存在服务器上
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_AddRecordLabel_RemoteEx(Int32 lLoginID, Guid guid, string szLabelName, Int32 nLen, Int64 labelTime);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_DelRecordLabel_RemoteEx(Int32 lLoginID, Guid guid);//标签ID
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetRecordLabel_RemoteEx(Int32 lLoginID, Guid guid);

        /* 获取 亮度，饱和度、灰度、对比度
 	     * login: 登录句柄
 	    * lChannel: 通道ID
 	    * pBrightValue:   [out]亮度 0－100；
 	    * pContrastValue：[out]对比度 0－100；
 	    * pSaturationValue ：[out]饱和度指针，取值范围[0,100] 
 	    * pHueValue :        [out]色度指针，取值范围[0,100] 
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetVideoEffectEx(Int32 lLoginID, Guid guid, ref UInt32 nBrightValue, ref UInt32 nContrastValue,
            ref UInt32 nSaturationValue, ref UInt32 nHueValue);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SetVideoEffectEx(Int32 lLoginID, Guid guid, UInt32 nBrightValue, UInt32 nContrastValue,
            UInt32 nSaturationValue, UInt32 nHueValue);

        /* 设备参数配置获取      主子码流的 帧率、分辨率、码率、编码格式, (OSD参数获取,时间、通道名称、自定义OSD);
 	    * login: 登录句柄
 	    * dwCommand： 配置命令， 用于指定相关数据, enPlat_Config_Opt值，如：CONFIG_GETOSD， 
 	    * lChannel: 通道ID
 	    * lpInBuffer： 输入数据的缓冲指针
 	    * dwOutBufferSize ：缓冲长度
 	    **/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetDevConfigEx(Int32 lLoginID, Guid guid, UInt32 dwCommand);  // 
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SetDevConfigEx(Int32 lLoginID, Guid guid, UInt32 dwCommand, IntPtr lpInBuffer, UInt32 dwInBufferSize);// 


        //获取能力集
        // NodeID: [in] 节点ID号；
        // dwNodeType: [in]标识节点类型：值为：NODETYPE_DEVICE，NODETYPE_CHANNEL
        // Ability：[out]能力集值，SDKDefs.h有定义
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetDeviceAbilityEx(Int32 lLoginID, Guid guid, UInt32 dwNodeType, ref UInt32 Ability);// 

        // 订阅、取消订阅报警
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SubscribeAlarmEx(fAlarmCallback AlarmCB, IntPtr pUser);  // 
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_UnSubscribeAlarmEx();
        // 订阅、取消订阅报警(带图片)
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SubscribeAlarmExPic(fAlarmCallbackEx AlarmCB, IntPtr pUser);  // 
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_UnSubscribeAlarmExPic();

        // New subscription and unsubscription functions for alarms, compatible with the above two methods.
        // In the future, simply call this new subscription.
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SubscribeAlarm_V2(fAlarmCallback_V2 alarmCB, IntPtr pUser);  // 
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_UnSubscribeAlarm_V2();

        // Set the platform's return language. Before obtaining translations from the platform, this function should be called.
        // Otherwise, the default is English.
        // If a translation for that language does not exist, default to returning English. If no translation exists, return an empty string.
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SetPlatfromLanguage(Plat_LanguageType langType);

        // 人脸比对相关操作
        // lLoginID        :[in]  NET_SDK_Login()的返回值
        // dwCommand       :[in]  命令类型，enum enPlat_Face_Match_Opt
        // lpInBuffer      :[in]  发送数据的缓冲指针 
        // dwInBufferSize  :[in]  发送数据的缓冲长度(以字节为单位) 
        // lpOutBuffer     :[out] 接收数据的缓冲指针 
        // dwOutBufferSize :[in]  接收数据的缓冲长度(以字节为单位) 
        // lpBytesReturned :[out] 实际收到的数据长度指针，不能大于dwOutBufferSize 
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_FaceMatchOperateEx(int lLoginID, int dwCommand, IntPtr lpInBuffer, int dwInBufferSize, IntPtr lpOutBuffer, int dwOutBufferSize, ref int lpBytesReturned);

        [DllImport(PLAT_CLIENT_SDK)]
        public static extern void Plat_FaceMatchCreateAlbumEx(int lLoginID, ref FaceMatch_AlbumInfo pInfo, IntPtr outResult);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern void Plat_FaceMatchModifyAlbumEx(int lLoginID, ref FaceMatch_AlbumInfo pInfo, IntPtr outResult);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern void Plat_FaceMatchDeleteAlbumEx(int lLoginID, ref FaceMatch_AlbumInfo pInfo, IntPtr outResult);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern void Plat_FaceMatchClearTargetEx(int lLoginID, ref FaceMatch_AlbumInfo pInfo, IntPtr outResult);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern void Plat_FaceMatchCreateTargetEx(int lLoginID, ref FaceMatch_TargetInfo pInfo, IntPtr outResult);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern void Plat_FaceMatchModifyTargetEx(int lLoginID, ref FaceMatch_TargetInfo pInfo, IntPtr outResult);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern void Plat_FaceMatchDeleteTargetEx(int lLoginID, ref FaceMatch_TargetInfo pInfo, IntPtr outResult);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_FaceMatchGetTargetInfoByIDEx(int lLoginID, int serverId/*智能分析服务ID*/, Guid albumGUID/*目标库GUID*/, int nTargetID/*目标id*/ , IntPtr lpOutBuffer/*接收数据的缓冲指针*/ , int dwOutBufferSize/*接收数据的缓冲长度(以字节为单位) */, ref int lpBytesReturned/*实际收到的数据长度*//*FaceMatch_TargetInfo+图片*/);


        /************************************************************************/
        /* 辅助功能                                                     */
        /************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern UInt32 Plat_GetLastErrorEx();		// 
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern Int64 Plat_GetSDKVersionEx(); // 0x00010300 00060723
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_RequestKbTvWallOptionEx(string pReqType, string pReqXml, uint nReqLen, IntPtr lpOutBuffer, uint dwOutBufferSize, ref uint lpBytesReturned);

        [DllImport(PLAT_CLIENT_SDK)]//根据节点的GUID获取int型ID
        public static extern uint Plat_GetNodeIDByGUIDEx(int lLoginID, Guid nodeGUID);
        [DllImport(PLAT_CLIENT_SDK)]//根据节点的int型ID获取GUID
        public static extern Guid Plat_GetNodeGUIDByIDEx(int lLoginID, uint nodeId);
        [DllImport(PLAT_CLIENT_SDK)]//从字符串中提取出GUID
        public static extern Guid Plat_GetGuidFromStringEx(string pGUIDStr);
        [DllImport(PLAT_CLIENT_SDK)]//将GUID转成字符串
        public static extern bool Plat_GuidToStringEx(ref Guid guid, IntPtr pGUIDStr);

        [DllImport(PLAT_CLIENT_SDK)]//注册电视墙获取通道id
        public static extern bool Plat_SetKeyboardMessageCBEx(fKeyboardMessageCallback MsgCB, int lLoginID, int lMon, int lWin, IntPtr pUser);

        [DllImport(PLAT_CLIENT_SDK)]//创建人数统计任务（同步）
        public static extern bool Plat_CreateStatisticTaskEx(int lLoginID, Guid nodeGuid, Guid svrGuid/*智能分析服务guid*/);

        [DllImport(PLAT_CLIENT_SDK)]//获取实时人数统计（同步）
        public static extern bool Plat_RequestStatisticEx(int lLoginID, Guid nodeGuid, Guid svrGuid/*智能分析服务guid*/, IntPtr lpInBuffer, uint dwInBufferSize, ref uint lpBytesReturned);

        [DllImport(PLAT_CLIENT_SDK)]//删除人数统计任务(同步、异步均可)
        public static extern bool Plat_DeleteStatisticTaskEx(int lLoginID, Guid nodeGuid, Guid svrGuid/*智能分析服务guid*/);

        [DllImport(PLAT_CLIENT_SDK)]//创建人数统计任务(异步)
        public static extern bool Plat_CreateNewStatisticTaskEx(int lLoginID, Guid nodeGuid, Guid svrGuid/*智能分析服务guid*/);

        [DllImport(PLAT_CLIENT_SDK)]//获取人数统计（异步）
        public static extern bool Plat_RequestNewStatisticEx(int lLoginID, Guid nodeGuid, Guid svrGuid/*智能分析服务guid*/, uint startTime, uint endTime,
            uint pageType, uint reportType, uint crossType, uint targetType);


        //bSwitch 看守位开启或者关闭
        //strLocation;		看守位置类型：预置点"PRE",巡航"CRU"，轨迹"TRA"，随机扫描"RSC"，固定扫描"ASC"
        //nNumber;			编号 预置点70-89,巡航1-8，轨迹1-4，随机扫描1，固定扫描1
        //nWaitTime;			等待时间7-180秒
        [DllImport(PLAT_CLIENT_SDK)]//设置球机看守位
        public static extern bool Plat_PTZSetGuardEx(int lLoginID, Guid nodeGuid, bool bSwitch, string location, int nNumber, int waitTime);

        [DllImport(PLAT_CLIENT_SDK)]//不预览抓图
        public static extern bool Plat_CaptureJpgPictureDataEx(int lLoginID, Guid nodeGuid, IntPtr picData, int picLen, ref int lpBytesReturned);

        [DllImport(PLAT_CLIENT_SDK)]//开启设备通道手动录像
        public static extern bool Plat_StartManulRecordEx(int lLoginID, Guid chnlGuid);

        [DllImport(PLAT_CLIENT_SDK)]//关闭设备通道手动录像
        public static extern bool Plat_StopManulRecordEx(int lLoginID, Guid chnlGuid);

        [DllImport(PLAT_CLIENT_SDK)]//查询考勤记录表
        public static extern bool Plat_GetAttendanceLogEx(int lLoginID, int dwCommand, int m_startTime, int m_endTime, IntPtr data, int inBufferSize);

        [DllImport(PLAT_CLIENT_SDK)]//查询考勤详细信息
        public static extern bool Plat_GetAttendanceDetailEx(int lLoginID, Guid serverGuid, Guid chnlGuid, int m_startTime, int m_endTime, int targetId, Guid albumGuid, int pageIndex, int countPerPage);
        /**************************************************************************
        添加组织（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务的ID
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetOrgInfo(int lLoginID, int nSrvId);
        /**************************************************************************
        添加组织（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务的ID
            orgName			添加组织的名称						
            upperOrgGuid	上级组织的GUID		
            nType			类型，参考_ORG_TYPE
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_AddOrganization(int lLoginID, int nSrvId, IntPtr orgName, Guid upperOrgGuid, int orgType);
        /**************************************************************************
        删除组织（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务的ID
            upperOrgGuid	上级组织的GUID
            orgGUID			当前组织的GUID
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_DeleteOrganization(int lLoginID, int nSrvId, Guid upperOrgGuid, Guid orgGuid, int orgType);
        /**************************************************************************
        修改组织（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务的ID
            orgName			添加组织的名称
            upperOrgGuid	上级组织的GUID
            orgGUID			当前组织的GUID
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_ModifyOrganization(int lLoginID, int nSrvId, IntPtr orgName, Guid upperOrgGuid, Guid orgGuid, int orgType);
        //获取智能服务的ID，因为智能服务平台暂时只开放一个
        //---------------------------------------------------------------------------------------------------------------------
        /**************************************************************************
        获取智能服务id（同步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务的ID(接口返回)
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetIASrvId(int lLoginID, ref int nSrvId);
        /**************************************************************************
        获取目标列表（异步）
        参数：
        lLoginID		登录id
        nSrvId			智能服务的ID
        orgGuid			组织GUID
        showChild		是否显示子组织的目标
        beginIndex		开始索引
        endIndex		结束索引
        strTargetLibName 目标库的名称(接口返回)
        OrgType			组织类型(参考_ORG_TYPE)
        返回：
        成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetAITargetParamList(int ulLoginID, int nSrvId, Guid orgGuid, bool showChild, int beginIndex, int endIndex, ref String strTargetLibName, int OrgType);
        //目标管理相关
        //---------------------------------------------------------------------------------------------------------------------
        //
        /**************************************************************************
        添加目标（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务的ID
            targetInfo		targetInfo为目标库信息
            nType			类型，参考_ORG_TYPE
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_AddAlbumTargetList(int lLoginID, int nSrvId, ref List<Plat_TargetInfo> targetInfo);
        /**************************************************************************
        删除目标（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务的ID
            setTargetId		需要删除的targetId的集合
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_DeleteAlbumTargetList(int lLoginID, int nSrvId, ref List<int> lstTargetId);
        /**************************************************************************
        修改目标（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务的ID
            targetInfo		targetInfo为目标库信息
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_ModifyAlbumTargetList(int lLoginID, int nSrvId, ref List<Plat_TargetInfo> targetInfo);
        /**************************************************************************
        下发人员到设备（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务的ID
            targetInfo		targetInfo为人员信息信息
            addDevGuidList  添加的设备GUID
            deleteDevGuidList  删除的设备GUID
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_UpdateDevByPersion(int lLoginID, int nSrvId, ref List<Plat_TargetPersinInfo> targetInfo, ref List<Guid> addDevGuidList, ref List<Guid> delDevGuidList);
        /**************************************************************************
        获取下发配置（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务的ID
            targetInfo		targetInfo为人员信息信息
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetDevByPersionInfo(int lLoginID, int nSrvId, ref List<Plat_TargetPersinInfo> targetInfo, ref List<int> lstState);

        /**************************************************************************
        获取人员下发信息（异步）
        参数：
        lLoginID		登录id
            nSrvId			智能服务的ID
            devId			设备ID
            lstState		需要查询的几种状态集合，
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetPersionByDev(int lLoginID, int nSrvId, int devId, ref List<int> lstState);
        //权限/用户管理相关
        /**************************************************************************
        添加权限组（同步）
        参数：
            lLoginID		登录id
            permission		权限信息
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_CreatePermissionGroup(int lLoginID, ref Plat_PermisinInfo permission);
        /**************************************************************************
        修改权限组（同步）
        参数：
            lLoginID		登录id
            permission		权限信息
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_ModifyPermissionGroup(int lLoginID, ref Plat_PermisinInfo permission);
        /**************************************************************************
        修改权限组（同步）
        参数：
            lLoginID		登录id
            permission		权限信息
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_DeletePermissionGroup(int lLoginID, ref List<int> permission);
        /**************************************************************************
        添加用户（同步）
        参数：
            lLoginID		登录id
            userInfo		用户信息
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_CreateUser(int lLoginID, ref Plat_UserInfo userInfo);
        /**************************************************************************
        删除用户（同步）
        参数：
            lLoginID		登录id
            lstId			用户id集合
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_DeleteUser(int lLoginID, ref List<int> lstId);
        /**************************************************************************
        修改用户（同步）
        参数：
            lLoginID		登录id
            userInfo		用户信息
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_ModifyUser(int lLoginID, ref Plat_UserInfo userInfo);
        /**************************************************************************
        获取所有用户信息（同步）
        参数：
            lLoginID		登录id
            userInfo	返回的用户数据集合
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetAllUserInfo(int lLoginID, ref List<Plat_UserInfo> userInfo);
        /**************************************************************************
        获取所有权限组信息（同步）
        参数：
            lLoginID		登录id
            lstPermission	返回的用户数据集合
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetAllAuthGroupInfo(int lLoginID, ref List<Plat_PermisinInfo> lstPermission);

        /**************************************************************************
        获取对应用户的权限（同步）
        参数：
            lLoginID		登录id
            UserName		登录用户名
            nType			类型，参考 PlatAuthGroup
            nNodeID			节点id，某些操作权限需要传递，如区域的操作权限，通道的操作权限以及组织的操作权限等,非操作权限传0
            nTypeAuth		权限的种类
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_AuthGroup(int lLoginID, PlatAuthGroup nType, int nNodeID, PlatAuthType nTypeAuth);
        /**************************************************************************
        设置osd，支持预览与回放，（同步）
        参数：
            streamID		流ID
            nodeGuid		通道GUID
            bClose			开启osd（通道名称）或关闭
            bNeedTime		是否显示时间
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SetOsd(UInt32 streamID, Guid nodeGuid, bool bClose, bool bNeedTime);
        /**************************************************************************
        获取RTSP地址（只针对预览流）（同步）
        参数：
            strIp			流媒体网关服务IP
            nPort			流媒体网关http端口
            lstGWUrlInfo	返回的流媒体网关相关url信息
            errCode			错误码,成功:200，400:错误请求，参数错误404:未找到请求资源500:服务器内部错误10001:设备不存在
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetStreamUrl(ref String strIp, uint nPort, ref List<stPlat_StreamGWUrlInfo> lstGWUrlInfo, ref uint errCode);

        /**************************************************************************
        获取RTSP地址（只针对回放流）（同步）
        参数：
            strIp			流媒体网关服务IP
            nPort			流媒体网关http端口
            lstGWUrlInfo	返回的流媒体网关相关url信息
            param			请求的参数
            errCode			错误码,成功:200，400:错误请求，参数错误404:未找到请求资源500:服务器内部错误10001:设备不存在
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_PlayGetStreamUrl(ref String strIp, uint nPort, ref Plat_StreamPlayBackParm param, ref List<Plat_StreamPlayBackResponse> lstGWUrlInfo, ref uint errCode);

        /**************************************************************************
        通知平板开门（同步）
        参数：
            lLoginID		登录id
            nChannelID		通道ID
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_OpenDevFaceDoor(int lLoginID, uint nChannelID);

        /**************************************************************************
        通知平板开门（异步）
        参数：
            lLoginID		登录id
            nDevID			设备Id
        返回：
            成功返回true，失败返回false。
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_RebootDevice(int lLoginID, uint nDevID);
        /**************************************************************************
        获取对应时间内的人脸比对信息（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务ID
            nStartTime		开始时间
            nEndTime		结束时间
            cType			类型0x00, 比对成功的/ 0x01, 比对失败的;0x02, 所有比对结果。;
            nPageIndex		页面索引
            nCntPerPage		当前页面数量
            srcData			搜索源
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetTargetSlice(int lLoginID, int nSrvId, ref Guid chGuid, uint nStartTime, uint nEndTime, char cType, uint nPageIndex, uint nCntPerPage, PLAT_IA_DATA_SRC srcData);
        /**************************************************************************
        获取人脸比对信息详情（异步）
        参数：
            lLoginID		登录id
            nSrvId			智能服务ID
            nStartTime		开始时间
            nEndTime		结束时间
            cType			类型0x00, 比对成功的/ 0x01, 比对失败的;0x02, 所有比对结果。;
            nPageIndex		页面索引
            nCntPerPage		当前页面数量
            srcData			搜索源
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetFaceTargetDetal(int lLoginID, int nSrvId, ref Guid orgGuid, uint nTargetId, ref Guid chGuid, uint chnlTime, uint nFaceId);
        /**************************************************************************
        请求存储手动开始录像（异步）
        参数：
            lLoginID		登录id
            chnlID			通道id
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_ManualStoreStart(int lLoginID, int chnlID);
        /**************************************************************************
        请求存储手动停止录像（异步）
        参数：
            lLoginID		登录id
            chnlID			通道id
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_ManualStoreStop(int lLoginID, int chnlID);
        /**************************************************************************
        视频截图,仅支持回放（同步）
        参数：
            path			文件路径
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SnapShotVideo(IntPtr pPath);
        /**************************************************************************
        订阅人脸相关（同步）
        参数：
            lLoginId		登陆ID
            nSrvId			智能服务ID
            setGuid			智能服务GUID
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SubFacePic(int lLoginId, int nSrvId, ref List<Guid> setGuid);
        /**************************************************************************
        取消订阅人脸相关（同步）
        参数：
            lLoginId		登陆ID
            nSrvId			智能服务ID
            setGuid			智能服务GUID
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_UnSubFacePic(int lLoginId, int nSrvId, ref List<Guid> setGuid);
        /**************************************************************************
        设置是否需要推送人脸比对信息（同步）
        参数：
            lLoginId		登陆ID
            bNeed			true 需要推送，false，不需要
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_SetNeedFaceMatch(int lLoginId, bool bNeed);
        /**************************************************************************
        添加车辆/黑名单车（同步）
        参数：
            lLoginId		登陆ID
            platPassData	添加固定车辆信息
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_CreateFixedCar(int lLoginId, ref List<Plat_CarInfo> platPassData);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_CreateFixedCarSingle(int lLoginId, ref Plat_CarInfo platPassData);
        /**************************************************************************
        修改车辆/黑名单车（同步）
        参数：
            lLoginId		登陆ID
            platPassData	添加固定车辆信息
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_ModifyFixedCar(int lLoginId, ref List<Plat_CarInfo> platPassData);
        /**************************************************************************
        删除车辆/黑名单车（同步）
        参数：
            lLoginId		登陆ID
            info			删除的固定车辆GUID集合
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_DeleteFixedCar(int lLoginId, ref List<Guid> info);
        /**************************************************************************
        查询固定车辆（同步）
        参数：
            lLoginId		登陆ID
            guidCarGroup	车辆组GUID
            platPassData	查询出来的数据
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_QueryFixedCar(int lLoginId, ref Guid guidCarGroup, ref List<Plat_CarInfo> platPassData);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_QueryFixedCarExCount(int lLoginId, ref Guid guidCarGroup, ref int nCount);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_QueryFixedCarEx(int lLoginId, ref Guid guidCarGroup, IntPtr platPassData);
        /**************************************************************************
        添加车辆组（同步）
        参数：
            lLoginId		登陆ID
            lstCarGroup		添加车辆组信息
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_CreateCarGroup(int lLoginId, ref List<Plat_CarGroupInfo> lstCarGroup);
        /**************************************************************************
	    修改车辆组信息（同步）
	    参数：
		    lLoginId		登陆ID
		    lstCarGroup		修改的车辆组信息
	    返回：
		    成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
	    **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_ModifyCarGroup(int lLoginId, ref List<Plat_CarGroupInfo> lstCarGroup);
        /**************************************************************************
	    删除车辆组信息（同步）
	    参数：
		    lLoginId		登陆ID
		    lstCarGroup		删除的车辆组ID
	    返回：
		    成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
	    **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_DeleteCarGroup(int lLoginId, ref List<Guid> lstCarGroup);
        /**************************************************************************
	    查询车辆组（同步）
	    参数：
		    lLoginId		登陆ID
		    lstCarGroup		查询出来数据结果
	    返回：
		    成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
	    **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_QueryCarGroup(int lLoginId, ref List<Plat_CarGroupInfo> lstCarGroup);

        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_QueryCarGroupEx(int lLoginId, IntPtr lstCarGroup);
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_GetQueryCarGroupCount(int lLoginId, ref int nCount);
        /**************************************************************************
        查询黑名单车辆（同步）
        参数：
            lLoginId		登陆ID
            platPassData	查询出来的数据
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_QueryBlackListCar(int lLoginId, ref List<Plat_CarInfo> platPassData);
        /**************************************************************************
        查询通行记录（异步）
        参数：
            condition		查询条件
            pData	返回数据
            nCount  返回结果
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_QueryPassRecordHistory(ref Plat_PassRecordQueryCondition condition);
        /**************************************************************************
        修改通行记录+手动放行（异步）
        参数：
            curPassInfo		当前通行记录
            hisPassInfo	    历史通行记录
            ManualType      1为手动放行
        返回：
            成功返回true，失败返回false。返回false后通过Plat_GetLastErrorEx获取错误码
        **************************************************************************/
        [DllImport(PLAT_CLIENT_SDK)]
        public static extern bool Plat_PmsOperatePassRec(ref Plat_PassRecordItem curPassInfo, ref Plat_PassRecordItem hisPassInfo, int ManualType);
        #endregion
        public static string RemoveEmptyChar(string value)
        {
            return value.Replace('\0'.ToString(), string.Empty);
        }
        public static byte[] RemoveZero(byte[] src)
        {
            List<byte> resList = new List<byte>();
            for (int i = 0; i < src.Length; i++)
            {
                if (src[i] != 0)
                    resList.Add(src[i]);
                else
                    break;
            }
            return resList.ToArray();
        }
        public static string ByteToStr(byte[] src)
        {
            return Encoding.UTF8.GetString(RemoveZero(src));
        }
        public static DateTime GetTime(long TimeStamp, bool AccurateToMilliseconds = false)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)); // 当地时区
            if (AccurateToMilliseconds)
            {
                return startTime.AddTicks(TimeStamp * 10000);
            }
            else
            {
                return startTime.AddTicks(TimeStamp * 10000000);
            }
        }

    }
}
