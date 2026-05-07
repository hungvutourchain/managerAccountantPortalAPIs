using System;
using System.Collections.Generic;

namespace B2BAdmin.ApiDocument.Application.Response
{
    public class ItemsTotal<T>
    {
        public string? status { get; set; }
        public string? message { get; set; }
        public long Total { get; set; }
        public IList<T> Items { get; set; }
    }
    public class ItemWithStatus<T>
    {
        public string? status { get; set; }
        public T Item { get; set; }
    }
    public class Insertour<T>
    {
        public bool had { get; set; }
        public T Tour { get; set; }
    }
}
