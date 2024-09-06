﻿using System;
using System.Collections.Generic;

namespace Autho_micro.Models;

public partial class PaymentMethod
{
    public int PaymentMethodId { get; set; }

    public string? MethodName { get; set; }

    public virtual ICollection<OrderPayment> OrderPayments { get; set; } = new List<OrderPayment>();
}
