﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorApp1.Models;

/// <summary>
/// Таблица содержит список методик
/// </summary>
public partial class Method
{
	/// <summary>
	/// Id методики
	/// </summary>
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int IdMethods { get; set; }

    /// <summary>
    /// Наименование методики
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Описание методики, ее история, основные принципы
    /// </summary>
    public string? Descr { get; set; }

    public virtual ICollection<Argument> Arguments { get; set; } = new List<Argument>();


}
