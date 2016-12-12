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

namespace BrightBit
{

namespace EventSystem
{

/// <summary>
/// the event director is a hub facilitating delegate based messaging
/// between components inside a gameobject hierarchy. components
/// may register or unregister with the event handler to gain
/// access to its delegates (which are declared in a derived class).
/// registered objects can execute event handler delegate fields
///	and add their own methods to the invocation lists.
/// </summary>
public abstract class EventDirector : MonoBehaviour
{
    protected List<object> pendingObservers                                        = new List<object>();
    protected Dictionary<string, BrightBit.EventSystem.Event> methodNameToEventMap = new Dictionary<string, BrightBit.EventSystem.Event>();

    protected bool initialized = false;

    /// <summary>
    /// Instantiates all Event fields and subscribes all objects that
    /// might have tried to subscribe while this instance wasn't awake.
    /// </summary>
    protected virtual void Awake()
    {
        InstantiateAllEventFields();

        initialized = true;

        foreach (object observer in pendingObservers)
        {
            Subscribe(observer);
        }

        pendingObservers.Clear();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="observer"> </param>
    public void Subscribe(object observer)
    {
        if (observer == null)
        {
            Debug.LogError(this + "::" + MethodBase.GetCurrentMethod().Name + "() : Argument is null!");
            return;
        }

        if (!initialized)
        {
            pendingObservers.Add(observer);
            return;
        }

        List<MethodInfo> methods = GetMethods(observer.GetType());

        if (methods == null)
        {
            Debug.LogError(this + "::" + MethodBase.GetCurrentMethod().Name + "() : Could not find any method in '" + observer.GetType() + "'!");
            return;
        }

        Unsubscribe(observer); // prevents duplicate subscriptions

        BrightBit.EventSystem.Event currentEvent = null;

        foreach (MethodInfo method in methods)
        {
            if (!(methodNameToEventMap.TryGetValue(method.Name, out currentEvent))) continue;

            currentEvent.Subscribe(observer, method);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="observer"> </param>
    public void Unsubscribe(object observer)
    {
        if (observer == null)
        {
            Debug.LogError(this + "::" + MethodBase.GetCurrentMethod().Name + "() : Argument is null!");
            return;
        }

        if (!initialized)
        {
            pendingObservers.Remove(observer);
            Debug.LogWarning(this + "::" + MethodBase.GetCurrentMethod().Name + "() : EventDirector instance is not even initialized yet!");
            return;
        }

        List<MethodInfo> methods = GetMethods(observer.GetType());

        if (methods == null)
        {
            Debug.LogError(this + "::" + MethodBase.GetCurrentMethod().Name + "() : Could not find any method in '" + observer.GetType() + "'!");
            return;
        }

        BrightBit.EventSystem.Event currentEvent = null;

        foreach (MethodInfo method in methods)
        {
            if (!(methodNameToEventMap.TryGetValue(method.Name, out currentEvent))) continue;

            currentEvent.Unsubscribe(observer, method);
        }
    }

    /// <summary>
    /// Searches and defines, i.e. instantiates, all events declared in all
    /// sub classes of the EventDirector instance that is calling this method.
    /// Each created Event instance supports at least one event delegate. All
    /// possible delegate/method names will be stored in a Dictionary for
    /// later reference.
    /// </summary>
    void InstantiateAllEventFields()
    {
        List<FieldInfo> eventFields = FindAllEventFields();

        if (eventFields == null || eventFields.Count == 0) return;

        foreach (FieldInfo field in eventFields)
        {
            BrightBit.EventSystem.Event currentEvent = null;

            try
            {
                currentEvent = Activator.CreateInstance(field.FieldType, field.Name) as BrightBit.EventSystem.Event;
            }
            catch
            {
                Debug.LogError(this + "::" + MethodBase.GetCurrentMethod().Name + "() : Could not instantiate '" + field.Name + "' of type '" + field.DeclaringType + "'!");
                continue;
            }

            if (currentEvent == null) continue;

            field.SetValue(this, currentEvent);

            currentEvent.AddSupportedMethodNames(ref methodNameToEventMap); // adds all method names supported by the current event to 'methodNameToEventMap'
        }
    }

    /// <summary>
    /// Searches all events declared in all sub classes of the EventDirector
    /// instance that is calling this method.
    /// </summary>
    List<FieldInfo> FindAllEventFields()
    {
        List<FieldInfo> result = new List<FieldInfo>();
        Type currentType       = this.GetType();

        // look for all instance members declared at the level of the supplied type's hierarchy, i.e. inherited members are not considered
        BindingFlags bindingMask = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        while (currentType != null && currentType != typeof(EventDirector))
        {
            foreach (FieldInfo field in currentType.GetFields(bindingMask))
            {
                if (field.FieldType.IsSubclassOf(typeof(BrightBit.EventSystem.Event))) result.Add(field);
            }

            currentType = currentType.BaseType;
        }

        if (result == null || result.Count == 0) Debug.LogWarning(this + "::" + MethodBase.GetCurrentMethod().Name + "() : Could not find any field of type 'Event'!");

        return result;
    }

    List<MethodInfo> GetMethods(Type currentType)
    {
        List<MethodInfo> result   = new List<MethodInfo>();
        List<string> alreadyFound = new List<string>();

        // look for all instance members declared at the level of the supplied type's hierarchy, i.e. inherited members are not considered
        BindingFlags bindingMask = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        while (currentType != null)
        {
            foreach (MethodInfo method in currentType.GetMethods(bindingMask))
            {
                if (alreadyFound.Contains(method.Name)) continue;               // sub class methods hide base methods with equal names
                if (!methodNameToEventMap.ContainsKey(method.Name)) continue;   // the method name has to match to one of the supported method names

                alreadyFound.Add(method.Name);
                result.Add(method);
            }

            currentType = currentType.BaseType;
        }

        return result;
    }
}

} // of namespace EventSystem

} // of namespace BrightBit
