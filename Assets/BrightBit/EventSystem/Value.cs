////////////////////////////////////////////////////////////////
//                                                            //
//  This file is part of BrightBit's Event Director package.  //
//                                                            //
//	Copyright (c) 2016 by BrightBit                           //
//                                                            //
//  This software may be modified and distributed under       //
//  the terms of the MIT license. See the LICENSE file        //
//  for details.                                              //
//                                                            //
////////////////////////////////////////////////////////////////

using System;
using System.Reflection;
using System.Collections.Generic;

namespace BrightBit
{

namespace EventSystem
{

/// <summary>
/// This class defines an event that will only fire when
/// the underlying value changes due to calls to Set<T>().
/// </summary>
/// <remarks>
/// Supported Method Signatures:
/// void On<EventName>Changed(<GenericTypeParameter> newValue)
/// </remarks>
public class Value<T> : Event where T : IEquatable<T>
{
	protected static void DoNothing<V>(V value) { }

	public delegate void Callback<V>(V v);

    Callback<T> changeCallbacks;

    public T CurrentValue  { get; private set; }
    public T PreviousValue { get; private set; }

	public Value(string name) : base(name)
	{
		Initialize(); // should be safe to be called in a constructor because we won't use initializers
	}

    protected override void Initialize()
    {
        fields = new List<FieldInfo>()
        {
            this.GetType().GetField("changeCallbacks", BindingFlags.NonPublic | BindingFlags.Instance)
        };

		supportedMethodNames = new List<string>()
        {
            "On" + Name + "Changed"
        };

		supportedMethodTypes = new List<Type>()
        {
            typeof(Value<T>.Callback<T>)
        };

		changeCallbacks = DoNothing<T>;
	}

    public T Get()
    {
        return CurrentValue;
    }

    public void Set(T value)
    {
        PreviousValue = CurrentValue;
        CurrentValue  = value;

        if (PreviousValue.Equals(value)) return;

        changeCallbacks.Invoke(CurrentValue);
    }
}

} // of namespace EventSystem

} // of namespace BrightBit
