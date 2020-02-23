﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EFwarehouse.Data;
using EFwarehouse.Models;
using Microsoft.AspNetCore.Authorization;

namespace EFwarehouse.Controllers
{
	[Authorize]
	public class ProductsController : Controller
	{
		private readonly ApplicationDbContext _context;

		public ProductsController(ApplicationDbContext context)
		{
			_context = context;
		}

		public async Task<IActionResult> Sale()
		{
			try
			{
				if (string.IsNullOrEmpty(Request.Cookies["OrderID"]))
				{
					int lastOrderID = 0;
					if (await _context.Order.AnyAsync())
					{
						lastOrderID = await _context.Order.MaxAsync(o => o.OrderId);
					}
					Response.Cookies.Append("OrderID", (lastOrderID + 1).ToString());
					ViewData["OrderId"] = lastOrderID + 1;
				} else
				{
					ViewData["OrderId"] = Request.Cookies["OrderID"];
				}
			}
			catch (Exception)
			{
				throw;
			}
			ViewData["Success"] = true;
			ViewData["Products"] = await _context.Products.ToListAsync();
			return View();
		}

		[HttpPost]
		public async Task<IActionResult> Sale(SaleViewModel model)
		{
			try
			{
				var selectedProduct = await _context.Products.FirstOrDefaultAsync(p => p.ProductId == model.ProductId);
				selectedProduct.Quantity_P -= model.Amount;
				var newOrder = new Order()
				{
					OrderId = model.OrderId,
					ProductId = model.ProductId,
					Quantity_O = model.Amount,
					Date = DateTime.Now,
					Price = selectedProduct.Price,
					Total_Price = selectedProduct.Price * model.Amount
				};
				await _context.Order.AddAsync(newOrder);
				await _context.SaveChangesAsync();
				return RedirectToAction("Bill");
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
			ViewData["Products"] = await _context.Products.ToListAsync();
			return View();
		}
		public async Task<IActionResult> Bill()
		{
			var OrderID = int.Parse(Request.Cookies["OrderID"]);
			var order = await _context.Order.FirstOrDefaultAsync(o => o.OrderId == OrderID);
			Response.Cookies.Delete("OrderID");
			return View(order);
		}
		public async Task<IActionResult> Order()
		{
			return View(await _context.Products.ToListAsync());
		}

		public async Task<IActionResult> Summary()
		{
			return View();
		}
		// GET: Products
		/*public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Products.Include(p => p.ProductType);
            return View(await applicationDbContext.ToListAsync());
        }*/

		//GET: Products Search
		public async Task<IActionResult> Index(string searchstring)
		{
			var searchPro = _context.Products.Include(p => p.ProductType).AsQueryable();
			if (!String.IsNullOrEmpty(searchstring))
			{
				searchPro = searchPro.Where(s => s.Product_Name.Contains(searchstring));
			}
			return View(await searchPro.ToListAsync());
		}

		//GET: Warning
		public async Task<IActionResult> Warning()
		{
			return View(await _context.Products.ToListAsync());
		}

		// GET: Products/Details/5
		public async Task<IActionResult> Details(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var product = await _context.Products
				.Include(p => p.ProductType)
				.FirstOrDefaultAsync(m => m.ProductId == id);
			if (product == null)
			{
				return NotFound();
			}

			return View(product);
		}

		// GET: Products/Create
		public IActionResult Create()
		{
			ViewData["TypeId"] = new SelectList(_context.Set<ProductType>(), "TypeId", "TypeId");
			return View();
		}

		// POST: Products/Create
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create([Bind("ProductId,Product_Name,TypeId,Price,Quantity_P")] Product product)
		{
			if (ModelState.IsValid)
			{
				_context.Add(product);
				await _context.SaveChangesAsync();
				return RedirectToAction(nameof(Index));
			}
			ViewData["TypeId"] = new SelectList(_context.Set<ProductType>(), "TypeId", "TypeId", product.TypeId);
			return View(product);
		}

		// GET: Products/Edit/5
		public async Task<IActionResult> Edit(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var product = await _context.Products.FindAsync(id);
			if (product == null)
			{
				return NotFound();
			}
			ViewData["TypeId"] = new SelectList(_context.Set<ProductType>(), "TypeId", "TypeId", product.TypeId);
			return View(product);
		}
		// POST: Products/Edit/5
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(int id, [Bind("ProductId,Product_Name,TypeId,Price,Quantity_P")] Product product)
		{
			if (id != product.ProductId)
			{
				return NotFound();
			}

			if (ModelState.IsValid)
			{
				try
				{
					_context.Update(product);
					await _context.SaveChangesAsync();
				}
				catch (DbUpdateConcurrencyException)
				{
					if (!ProductExists(product.ProductId))
					{
						return NotFound();
					}
					else
					{
						throw;
					}
				}
				return RedirectToAction(nameof(Index));
			}
			ViewData["TypeId"] = new SelectList(_context.Set<ProductType>(), "TypeId", "TypeId", product.TypeId);
			return View(product);
		}
		// GET: Products/Delete/5
		public async Task<IActionResult> Delete(int? id)
		{
			if (id == null)
			{
				return NotFound();
			}

			var product = await _context.Products
				.Include(p => p.ProductType)
				.FirstOrDefaultAsync(m => m.ProductId == id);
			if (product == null)
			{
				return NotFound();
			}

			return View(product);
		}

		// POST: Products/Delete/5
		[HttpPost, ActionName("Delete")]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> DeleteConfirmed(int id)
		{
			var product = await _context.Products.FindAsync(id);
			_context.Products.Remove(product);
			await _context.SaveChangesAsync();
			return RedirectToAction(nameof(Index));
		}

		private bool ProductExists(int id)
		{
			return _context.Products.Any(e => e.ProductId == id);
		}
	}
}