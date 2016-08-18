﻿using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using Utils;

public class BaseResLoader: CachedMonoBehaviour
{
	protected static readonly string _cMainTex = "_MainTex";
	protected static readonly string _cMainMat = "_Mat_0";
	protected Dictionary<ResKey, ResValue> m_ResMap = new Dictionary<ResKey, ResValue>();

	protected string GetMatResName(int matIdx)
	{
		if (matIdx < 0)
			return string.Empty;
		string ret = string.Format("_Mat_{0:D}", matIdx);
		return ret;
	}

	protected struct ResKey
	{
		public int instanceId;
		public System.Type resType;
		public string resName;
	}

	protected struct ResValue
	{
		public UnityEngine.Object obj;
		public UnityEngine.Object[] objs;
	}

	protected static ResKey CreateKey(int instanceId, System.Type resType, string resName = "")
	{
		ResKey ret = new ResKey();
		ret.resName = resName;
		ret.resType = resType;
		ret.instanceId = instanceId;
		return ret;
	}

	protected ResValue CreateValue(UnityEngine.Object obj)
	{
		ResValue ret = new ResValue();
		ret.obj = obj;
		ret.objs = null;
		return ret;
	}

	protected ResValue CreateValue(UnityEngine.Object[] objs)
	{
		ResValue ret = new ResValue();
		ret.obj = null;
		ret.objs = objs;
		return ret;
	}

	protected virtual bool InternalDestroyResource(ResKey key, ResValue res)
	{
		if (key.resType == typeof(Sprite))
		{
			if (res.obj != null)
				Resources.UnloadAsset(res.obj);
		} else if (key.resType == typeof(Sprite[]))
		{
			if (res.objs != null)
			{
				for (int i = 0; i < res.objs.Length; ++i)
				{
					UnityEngine.Object obj = res.objs[i];
					if (obj != null)
						Resources.UnloadAsset(obj);
				}
			}
		}

		return true;
	}

	protected void DestroyResource(ResKey key)
	{
		ResValue res;
		if (m_ResMap.TryGetValue(key, out res))
		{
			if (InternalDestroyResource(key, res))
			{
				if (res.obj != null)
					ResourceMgr.Instance.DestroyObject(res.obj);

				if (res.objs != null)
					ResourceMgr.Instance.DestroyObjects(res.objs);
			}
			m_ResMap.Remove(key);
		}
	}

	protected void DestroyResource(int instanceId, System.Type resType, string resName = "")
	{
		ResKey key = CreateKey(instanceId, resType, resName);
		DestroyResource(key);
	}

	protected void SetResource(int instanceId, UnityEngine.Object res, System.Type resType, string resName = "")
	{
		ResKey key = CreateKey(instanceId, resType, resName);
		DestroyResource(key);
		if (res == null)
			return;
		ResValue value = CreateValue(res);
		m_ResMap.Add(key, value);
	}

	protected void SetResource(UnityEngine.Object target, UnityEngine.Object res, System.Type resType, string resName = "")
	{
		if (target == null)
			return;
		SetResource(target.GetInstanceID(), res, resType, resName);
	}

	protected void ClearResource<T>(UnityEngine.Object target, string resName = "") where T: UnityEngine.Object
	{
		System.Type resType = typeof(T);
		SetResource<T>(target, null, resName);
	}

	protected void ClearResource(UnityEngine.Object target, System.Type resType, string resName = "")
	{
		SetResource(target, null, resType, resName);
	}

	protected void SetResource<T>(UnityEngine.Object target, T res, string resName = "") where T: UnityEngine.Object
	{
		System.Type resType = typeof(T);
		SetResource(target, res, resType, resName);
	}

	protected void SetResources(int instanceId, UnityEngine.Object[] res, System.Type resType, string resName = "")
	{
		ResKey key = CreateKey(instanceId, resType, resName);
		DestroyResource(key);
		if (res == null)
			return;
		ResValue value = CreateValue(res);
		m_ResMap.Add(key, value);
	}

	protected void SetResources(UnityEngine.Object target, UnityEngine.Object[] res, System.Type resType, string resName = "")
	{
		if (target == null)
			return;
		SetResources(target.GetInstanceID(), res, resType, resName);
	}

	protected void ClearAllResources()
	{
		var iter = m_ResMap.GetEnumerator();
		while (iter.MoveNext())
		{
			if (InternalDestroyResource(iter.Current.Key, iter.Current.Value))
			{
				if (iter.Current.Value.obj != null)
					ResourceMgr.Instance.DestroyObject(iter.Current.Value.obj);

				if (iter.Current.Value.objs != null)
					ResourceMgr.Instance.DestroyObjects(iter.Current.Value.objs);
			}
		}
		iter.Dispose();
		m_ResMap.Clear();
	}

	protected virtual void OnDestroy()
	{
		ClearAllResources();
	}

	protected void DestroySprite(Sprite sp)
	{
		if (sp == null)
			return;
		Resources.UnloadAsset(sp);
		ResourceMgr.Instance.DestroyObject(sp);
	}
}
