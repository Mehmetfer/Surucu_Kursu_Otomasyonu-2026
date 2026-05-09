using Kolera.Mebbis.Models;

namespace Kolera_Mtsk.Sayfalar
{
    public sealed class MebbisOgrenimOnayForm : MebbisSaglikOnayForm
    {
        public MebbisOgrenimOnayForm() : this(new MebbisKursiyerModel(), null) { }

        public MebbisOgrenimOnayForm(MebbisKursiyerModel kursiyer, byte[] evrakResim)
            : base(kursiyer, "Ogrenim", evrakResim)
        {
        }
    }

    public sealed class MebbisSabikaOnayForm : MebbisSaglikOnayForm
    {
        public MebbisSabikaOnayForm() : this(new MebbisKursiyerModel(), null) { }

        public MebbisSabikaOnayForm(MebbisKursiyerModel kursiyer, byte[] evrakResim)
            : base(kursiyer, "Sabika", evrakResim) { }
    }

    public sealed class MebbisImzaOnayForm : MebbisSaglikOnayForm
    {
        public MebbisImzaOnayForm() : this(new MebbisKursiyerModel(), null) { }

        public MebbisImzaOnayForm(MebbisKursiyerModel kursiyer, byte[] evrakResim)
            : base(kursiyer, "Imza", evrakResim) { }
    }

    public sealed class MebbisSozlesmeOnOnayForm : MebbisSaglikOnayForm
    {
        public MebbisSozlesmeOnOnayForm() : this(new MebbisKursiyerModel(), null) { }

        public MebbisSozlesmeOnOnayForm(MebbisKursiyerModel kursiyer, byte[] evrakResim)
            : base(kursiyer, "Sozlesme On", evrakResim) { }
    }

    public sealed class MebbisSozlesmeArkaOnayForm : MebbisSaglikOnayForm
    {
        public MebbisSozlesmeArkaOnayForm() : this(new MebbisKursiyerModel(), null) { }

        public MebbisSozlesmeArkaOnayForm(MebbisKursiyerModel kursiyer, byte[] evrakResim)
            : base(kursiyer, "Sozlesme Arka", evrakResim) { }
    }

    public sealed class MebbisAdresOnayForm : MebbisSaglikOnayForm
    {
        public MebbisAdresOnayForm() : this(new MebbisKursiyerModel(), null) { }

        public MebbisAdresOnayForm(MebbisKursiyerModel kursiyer, byte[] evrakResim)
            : base(kursiyer, "Adres", evrakResim) { }
    }

    public sealed class MebbisFaturaOnayForm : MebbisSaglikOnayForm
    {
        public MebbisFaturaOnayForm() : this(new MebbisKursiyerModel(), null) { }

        public MebbisFaturaOnayForm(MebbisKursiyerModel kursiyer, byte[] evrakResim)
            : base(kursiyer, "Fatura", evrakResim) { }
    }
}
