using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

/// <summary>
/// Таблица содержит расчитанные показатели по методикам для банка на дату
/// </summary>
public partial class MethodsResult
{
    /// <summary>
    /// Id строки
    /// </summary>
    public int IdRes { get; set; }

    /// <summary>
    /// Id шапки, т.е., для какого банка и на какую дату
    /// </summary>
    public int? IdInfo { get; set; }

    /// <summary>
    /// Id показателя
    /// </summary>
    public int? IdArg { get; set; }

    /// <summary>
    /// Значение показателя
    /// </summary>
    public decimal? Val { get; set; }

    public virtual Argument? IdArgNavigation { get; set; }

    public virtual FormInfo? IdInfoNavigation { get; set; }
}
