﻿using System;
using UnityEngine;

/// <summary>
///     Since unity doesn't flag the Quaternion as serializable, we
///     need to create our own version. This one will automatically convert
///     between Quaternion and SerializableQuaternion
/// </summary>
[Serializable]
public struct SerializableQuaternion
{
    /// <summary>
    ///     x component
    /// </summary>
    public float x;
    /// <summary>
    ///     y component
    /// </summary>
    public float y;
    /// <summary>
    ///     z component
    /// </summary>
    public float z;
    /// <summary>
    ///     w component
    /// </summary>
    public float w;

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="rX"></param>
    /// <param name="rY"></param>
    /// <param name="rZ"></param>
    /// <param name="rW"></param>
    public SerializableQuaternion(float rX, float rY, float rZ, float rW)
    {
        x = rX;
        y = rY;
        z = rZ;
        w = rW;
    }

    /// <summary>
    ///     Automatic conversion from SerializableQuaternion to Quaternion
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator Quaternion(SerializableQuaternion rValue) =>
        new(rValue.x, rValue.y, rValue.z, rValue.w);

    /// <summary>
    ///     Automatic conversion from Quaternion to SerializableQuaternion
    /// </summary>
    /// <param name="rValue"></param>
    /// <returns></returns>
    public static implicit operator SerializableQuaternion(Quaternion rValue) =>
        new(rValue.x, rValue.y, rValue.z, rValue.w);

    /// <summary>
    ///     Returns a string representation of the object
    /// </summary>
    /// <returns></returns>
    public override string ToString() =>
        string.Format("[{0}, {1}, {2}, {3}]", x, y, z, w);
}