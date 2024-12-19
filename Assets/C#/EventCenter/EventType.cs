public enum EventType
{
    //事件的tag，需要有事件tag才能找到对应的事件
    //相当于电话号码，只需要添加新的电话号码就可以进行拨号
    
    ValidateCommand,//高亮显示事件
    PlayerMove,//玩家事件
    UpdateUI,ClickUI,OnDrag,EndDrag,//UI事件
    SetActiveTrue,SetActiveFalse//场景事件
}