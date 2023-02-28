﻿using MAgicVilla_VillaAPI.Data;
using MAgicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MAgicVilla_VillaAPI.Controllers
{
	[Route("api/VillaAPI")]
	[ApiController]
	public class VillaAPIController : ControllerBase
	{
		private readonly ILogger<VillaAPIController> _logger;

		public VillaAPIController(ILogger<VillaAPIController> logger)
		{
			_logger = logger;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public ActionResult<IEnumerable<VillaDTO>> GetVillas()
		{
			_logger.LogInformation("GettingAllVillas");
			return Ok(VillaStore.VillaList);
		}

		[HttpGet("{id:int}", Name ="GetVilla")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<VillaDTO> GetVilla(int id)
		{
			if (id == 0) 
			{
				_logger.LogError($"Get Villa Error with Id: {id}");
				return BadRequest(); 
			}
			var villa = VillaStore.VillaList.FirstOrDefault(item => item.Id == id);
			if (villa == null) 
			{ 
				return NotFound(); 
			}
			return Ok(villa);
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public ActionResult<VillaDTO> CreateVilla(VillaDTO villaDTO)
		{
			if (VillaStore.VillaList.
				    FirstOrDefault(item => item.Name.ToLower() == villaDTO.Name.ToLower()) != null)
			{
				ModelState.AddModelError("CustomError", $"Villa with name {villaDTO.Name} already exist!");
				return BadRequest(ModelState);
			}

			if (villaDTO == null)
			{
				return BadRequest(villaDTO);
			}
			if(villaDTO.Id > 0)
			{
				return StatusCode(StatusCodes.Status500InternalServerError);
			}
			villaDTO.Id = VillaStore.VillaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
			VillaStore.VillaList.Add(villaDTO);

			return CreatedAtRoute("GetVilla", new {id = villaDTO.Id}, villaDTO);
		}

		[HttpDelete("{id:int}", Name = "DeleteVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public IActionResult DeleteVilla(int id)
		{
			if (id == 0)
			{
				return BadRequest();
			}
			var villa = VillaStore.VillaList.FirstOrDefault(u => u.Id == id);
			if (villa == null)
			{
				return NotFound();
			}
			VillaStore.VillaList.Remove(villa);
			return NoContent();
		}

		[HttpPut("{id:int}", Name = "UpdateVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult UpdateVilla(int id, [FromBody]VillaDTO villaDTO)
		{
			if (villaDTO == null || id != villaDTO.Id)
			{
				return BadRequest();
			}
			var villa = VillaStore.VillaList.FirstOrDefault(u => u.Id == id);

			if (villa == null)
			{
				return NotFound();
			}
			villa.Name = villaDTO.Name;
			villa.Occupancy = villaDTO.Occupancy;
			villa.Sqft = villaDTO.Sqft;

			return NoContent();
		}

		[HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
		[ProducesResponseType(StatusCodes.Status204NoContent)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchDTO)
		{
			if (patchDTO == null || id == 0)
			{
				return BadRequest();
			}
			var villa = VillaStore.VillaList.FirstOrDefault(u => u.Id == id);
			if (villa == null)
			{
				return BadRequest();
			}
			patchDTO.ApplyTo(villa, ModelState);

			if (!ModelState.IsValid)
			{
				BadRequest(ModelState);
			}

			return NoContent();
		}
	}
}