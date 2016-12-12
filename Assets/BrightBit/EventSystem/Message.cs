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
/// This class defines an event that will fire as soon
/// as its Send() method is called.
/// </summary>
/// <remarks>
/// Supported Method Signatures:
/// void On<EventName>()
/// </remarks>
public class Message : Event
{
	public delegate void Sender();
	public Sender Send;

	public Message(string name) : base(name)
    {
        Initialize(); // should be safe to be called in a constructor because we won't use initializers
    }

    protected void CommonInitialisation()
    {
        fields = new List<FieldInfo>()
        {
            this.GetType().GetField("Send")
        };

		supportedMethodNames = new List<string>()
        {
            "On" + Name
        };
    }

	protected override void Initialize()
	{
		CommonInitialisation();

		supportedMethodTypes = new List<Type>()
        {
            typeof(Message.Sender)
        };

		Send = DoNothing;
	}
}

/// <summary>
/// This class defines an event that will fire as soon
/// as its Send(T message) method is called.
/// </summary>
/// <remarks>
/// Supported Method Signatures:
/// void On<EventName>(<GenericTypeParameter> message)
/// </remarks>
public class Message<T> : Message
{
	protected static void DoNothing<V>(V value) { }

	public delegate void Sender<V>(V value);
	public new Sender<T> Send;

	public Message(string name) : base(name) { }

	protected override void Initialize()
	{
		CommonInitialisation();

		supportedMethodTypes = new List<Type>()
        {
            typeof(Message<T>.Sender<T>)
        };

		Send = DoNothing<T>;
	}
}

} // of namespace EventSystem

} // of namespace BrightBit
