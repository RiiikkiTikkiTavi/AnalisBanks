using System;
using System.Collections.Generic;

namespace BlazorApp1.Models;

/// <summary>
/// Таблица содержит шапку данных форм
/// </summary>
public partial class FormInfo
{
    /// <summary>
    /// Id строки
    /// </summary>
    public int IdInfo { get; set; }

    /// <summary>
    /// Рег. номер банка, для которого расчитывается форма
    /// </summary>
    public int Regnum { get; set; }

    /// <summary>
    /// Дата, на которую расчитывается форма
    /// </summary>
    public DateOnly? Dt { get; set; }

    public virtual ICollection<Data101> Data101s { get; set; } = new List<Data101>();

    public virtual ICollection<DataNor> DataNors { get; set; } = new List<DataNor>();

    public virtual ICollection<MethodsResult> MethodsResults { get; set; } = new List<MethodsResult>();

    public virtual Bank RegnumNavigation { get; set; } = null!;
}
