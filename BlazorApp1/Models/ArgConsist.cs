using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

/// <summary>
/// Таблица содержит состав показателей методик
/// </summary>
public partial class ArgConsist
{
    /// <summary>
    /// Id строки
    /// </summary>
    public int IdArgConsist { get; set; }

    /// <summary>
    /// Id показателя
    /// </summary>
    public int? IdArg { get; set; }

    /// <summary>
    /// Id строки из 101 формы (БС2)
    /// </summary>
    public int? IdT101 { get; set; }

    /// <summary>
    /// Id строки из 123/135 форм (код норматива)
    /// </summary>
    public int? IdTnor { get; set; }

    public virtual Argument? IdArgNavigation { get; set; }

    public virtual Templates101? IdT101Navigation { get; set; }

    public virtual TemplatesNor? IdTnorNavigation { get; set; }
}
