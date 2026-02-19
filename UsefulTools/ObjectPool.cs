using System;
using System.Collections.Generic;
using UnityEngine;


namespace UsefulTools.Pool
{
    public class ObjectPool<T> : IDisposable where T : class
    {
        Queue<T> m_pool = new Queue<T>();

        public readonly Func<T> _onCreate;          //创建函数
        public readonly Action<T> _onGet;           //获取时调用的函数
        public readonly Action<T> _onRelease;       // 释放时调用的函数
        public readonly Action<T> _OnDestory;       // 删除的时候调用的函数

        /// <summary>
        /// 最大数量
        /// </summary>
        protected int MaxSize;

        /// <summary>
        /// 当前池中总数量
        /// </summary>
        public int CountAll { get; private set; }

        /// <summary>
        /// 当前激活的对象数量
        /// </summary>
        public int CountActive => CountAll - CountInactive;

        /// <summary>
        /// 当前未激活的对象数量
        /// </summary>
        public int CountInactive => m_pool.Count;

        /// <summary>
        /// 从上次重置大小后同时间激活数量最大峰值
        /// </summary>
        public int MaxAcitve { get; private set; } = 0;



        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="onCreate"></param>
        /// <param name="onGet"></param>
        /// <param name="onRelease"></param>
        /// <param name="onDestroy"></param>
        /// <param name="count"></param>
        /// <param name="maxSize"></param>
        /// <exception cref="Exception"></exception>
        public ObjectPool(Func<T> onCreate, Action<T> onGet = null, Action<T> onRelease = null,
         Action<T> onDestroy = null, int count = 10, int maxSize = 1024)
        {
            if (onCreate == null)
            {
                throw new Exception("未设置创建函数");
            }
            if (maxSize <= 0)
            {
                throw new Exception("最大数量应该大于0");
            }
            if(count <0)
            {
                throw new Exception("初始数量不能为负数");
            }
            m_pool = new Queue<T>();
            _onCreate = onCreate;
            _onGet = onGet;
            _onRelease = onRelease;
            _OnDestory = onDestroy;
            MaxSize = maxSize;
            Instantiate(count);
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="onCreate"></param>
        /// <param name="count"></param>
        /// <param name="maxSize"></param>
        public ObjectPool(Func<T> onCreate, int count, int maxSize = 1024) : this(onCreate, null, null,
            null, count, maxSize)
        { }

        /// <summary>
        /// 获取对象
        /// </summary>
        /// <returns></returns>
        public T Get()
        {
            T val;
            if (m_pool.Count == 0)
            {
                Instantiate(1);
            }

            val = m_pool.Dequeue();
            _onGet?.Invoke(val);
            MaxAcitve = CountActive > MaxAcitve ? CountActive : MaxAcitve;
            return val;
        }

        /// <summary>
        /// 释放对象
        /// </summary>
        /// <param name="val"></param>
        public void Release(T val)
        {
            _onRelease?.Invoke(val);
            if (CountInactive < MaxSize)
            {
                m_pool.Enqueue(val);
            }
            else
            {
                _OnDestory?.Invoke(val);
                --CountAll;
            }
        }

        /// <summary>
        /// 删除容器中所有对象
        /// </summary>
        public void Clear()
        {
            if (m_pool != null)
            {
                for (int i = m_pool.Count - 1; i >= 0; --i)
                {
                    _OnDestory?.Invoke(m_pool.Dequeue());
                }
            }
            m_pool.Clear();
            CountAll = 0;
            MaxSize = 0;
        }

        /// <summary>
        /// 设置最大数量
        /// </summary>
        /// <param name="size"></param>
        public void SetMaxSize(int size)
        {
            MaxSize = size;
            MaxAcitve = 0;
        }



        protected void Instantiate(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T obj = _onCreate();
                m_pool.Enqueue(obj);
                ++CountAll;
            }
        }

        /// <summary>
        /// 适应大小
        /// </summary>
        public void AdaptSize()
        {
            MaxSize = MaxAcitve;
            MaxAcitve = 0;
        }




        public void Dispose()
        {
            Clear();
        }
    }
}
