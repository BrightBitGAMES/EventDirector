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

using UnityEngine;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

namespace BrightBit
{

namespace EventSystem
{

/// <summary>
/// The base class for all possible events supported by the EventDirector.
/// </summary>
public abstract class Event
{
    protected static void DoNothing() { }
    protected static bool Success()   { return true; }

    public string Name { get; protected set; }

    protected List<string>    supportedMethodNames;
    protected List<Type>      supportedMethodTypes;
    protected List<FieldInfo> fields;

    public Event(string name)
    {
        Name = name;
    }

    protected abstract void Initialize(); // shouldn't be called in constructors

    internal virtual void Subscribe(object observer, MethodInfo method)
	{
        int index = supportedMethodNames.IndexOf(method.Name);

		AddDelegate(fields[index], supportedMethodTypes[index], observer, method.Name);
	}

	internal virtual void Unsubscribe(object observer, MethodInfo method)
	{
        int index = supportedMethodNames.IndexOf(method.Name);

		RemoveDelegate(fields[index], observer);
	}

    internal void AddSupportedMethodNames(ref Dictionary<string, BrightBit.EventSystem.Event> map)
    {
        foreach (string supportedMethodName in supportedMethodNames)
        {
            map.Add(supportedMethodName, this);
        }
    }

    void AddDelegate(FieldInfo field, Type methodType, object targetInstance, string methodName)
    {
        Delegate addition = Delegate.CreateDelegate(methodType, targetInstance, methodName, false, false);

        if (addition == null)
        {
            Debug.LogError(this + "::" + MethodBase.GetCurrentMethod().Name + "() : Could not add " + targetInstance.GetType() + "." + methodName + "!");
            return;
        }

        Delegate combination = Delegate.Combine((Delegate)field.GetValue(this), addition);

        field.SetValue(this, combination);
    }

    void RemoveDelegate(FieldInfo field, object targetInstance)
    {
        List<Delegate> delegates = new List<Delegate>(((Delegate)field.GetValue(this)).GetInvocationList());

        if (delegates == null)
        {
            Debug.LogError(this + "::" + MethodBase.GetCurrentMethod().Name + "() : Could not remove delegate from '" + field.Name + "' for " + targetInstance.GetType() + "!");
            return;
        }

        delegates.RemoveAll(d => d.Target == targetInstance);

        if (delegates != null) field.SetValue(this, Delegate.Combine(delegates.ToArray()));
    }
}

} // of namespace EventSystem

} // of namespace BrightBit
