﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel;

namespace EFwarehouse.Models
{
	public class ProductType
	{
		[DisplayName("รหัสชนิดสินค้า")]
		public int TypeId { get; set; }
		[DisplayName("ชื่อชนิดสินค้า")]
		public string Name { get; set; }
		// ทำให้เราเห็น products ทั้งหมดใน ประเภทนี้ได้โดยที่ไม่ต้อง query
		public IList<Product> Products { get; set; }
	}
}
