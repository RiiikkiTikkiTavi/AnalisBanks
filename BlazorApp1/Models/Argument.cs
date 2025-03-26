using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

/// <summary>
/// Таблица содержит список показателей, входящих в методики
/// </summary>
public partial class Argument
{
    /// <summary>
    /// Id показателя
    /// </summary>
    public int IdArg { get; set; }

    /// <summary>
    /// Название показателя
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Краткий код, короткое название показателя
    /// </summary>
    public string ShortName { get; set; } = null!;

    /// <summary>
    /// Описание показателя, принцип расчета, на основе чего строится
    /// </summary>
    public string? Descr { get; set; }

    /// <summary>
    /// Формула для расчета показателя
    /// </summary>
    public string? Expression { get; set; }

    /// <summary>
    /// Из каких параметров отчетных форм состоит
    /// </summary>
    public string? Consist { get; set; }

    /// <summary>
    /// Id методики, в которую включен показатель
    /// </summary>
    public int? IdMethods { get; set; }

    public virtual ICollection<ArgConsist> ArgConsists { get; set; } = new List<ArgConsist>();

    public virtual Method? IdMethodsNavigation { get; set; }

    public virtual ICollection<MethodsResult> MethodsResults { get; set; } = new List<MethodsResult>();
}
