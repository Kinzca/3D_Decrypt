//delegate=委托
//可以理解为拨号的类型转换

public delegate void CallBack();
//T为变量类型（int）arg为变量名
public delegate void CallBack<T>(T arg);
public delegate void CallBack<T, X>(T arg1, X arg2);
public delegate void CallBack<T, X, Y>(T arg1, X arg2, Y arg3);
public delegate void CallBack<T, X, Y, Z>(T arg1, X arg2, Y arg3, Z arg);