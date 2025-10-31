using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace KTX.Entities;

public partial class SinhVienKtxContext : DbContext
{
    public SinhVienKtxContext()
    {
    }

    public SinhVienKtxContext(DbContextOptions<SinhVienKtxContext> options)
        : base(options)
    {
    }

    public virtual DbSet<BaiDang> BaiDangs { get; set; }

    public virtual DbSet<DanhGia> DanhGia { get; set; }

    public virtual DbSet<HopDongPhong> HopDongPhongs { get; set; }

    public virtual DbSet<Phong> Phongs { get; set; }

    public virtual DbSet<SinhVien> SinhViens { get; set; }

    public virtual DbSet<ThanNhan> ThanNhans { get; set; }

    public virtual DbSet<ThongBao> ThongBaos { get; set; }

    public virtual DbSet<TienDienNuoc> TienDienNuocs { get; set; }

    public virtual DbSet<TienPhong> TienPhongs { get; set; }

    public virtual DbSet<TraLoi> TraLois { get; set; }

    public virtual DbSet<YeuCau> YeuCaus { get; set; }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//        => optionsBuilder.UseSqlServer("Server=DESKTOP-U195TOE\\SQLEXPRESS;Database=SinhVienKTX;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaiDang>(entity =>
        {
            entity.HasKey(e => e.MaBd);

            entity.ToTable("BaiDang");

            entity.Property(e => e.MaBd)
                .ValueGeneratedNever()
                .HasColumnName("MaBD");
            entity.Property(e => e.Msv).HasColumnName("MSV");
            entity.Property(e => e.NoiDungBd).HasColumnName("NoiDungBD");

            entity.HasOne(d => d.MsvNavigation).WithMany(p => p.BaiDangs)
                .HasForeignKey(d => d.Msv)
                .HasConstraintName("FK_BaiDang_SinhVien");
        });

        modelBuilder.Entity<DanhGia>(entity =>
        {
            entity.HasKey(e => e.MaDg);

            entity.Property(e => e.MaDg)
                .ValueGeneratedNever()
                .HasColumnName("MaDG");
            entity.Property(e => e.DiemDg)
                .HasMaxLength(20)
                .HasColumnName("DiemDG");
            entity.Property(e => e.MaYc).HasColumnName("MaYC");
            entity.Property(e => e.NgayGuiDg).HasColumnName("NgayGuiDG");
            entity.Property(e => e.NoiDungDg).HasColumnName("NoiDungDG");

            entity.HasOne(d => d.MaYcNavigation).WithMany(p => p.DanhGia)
                .HasForeignKey(d => d.MaYc)
                .HasConstraintName("FK_DanhGia_YeuCau");
        });

        modelBuilder.Entity<HopDongPhong>(entity =>
        {
            entity.HasKey(e => e.MaHd);

            entity.ToTable("HopDongPhong");

            entity.Property(e => e.MaHd)
                .ValueGeneratedNever()
                .HasColumnName("MaHD");
            entity.Property(e => e.LoaiP).HasMaxLength(50);
            entity.Property(e => e.Msv).HasColumnName("MSV");
            entity.Property(e => e.TienCoc).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TienP).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TrangThaiHd)
                .HasMaxLength(20)
                .HasColumnName("TrangThaiHD");

            entity.HasOne(d => d.MaPNavigation).WithMany(p => p.HopDongPhongs)
                .HasForeignKey(d => d.MaP)
                .HasConstraintName("FK_HopDongPhong_Phong");

            entity.HasOne(d => d.MsvNavigation).WithMany(p => p.HopDongPhongs)
                .HasForeignKey(d => d.Msv)
                .HasConstraintName("FK_HopDongPhong_SinhVien");
        });

        modelBuilder.Entity<Phong>(entity =>
        {
            entity.HasKey(e => e.MaP);

            entity.ToTable("Phong");

            entity.Property(e => e.MaP).ValueGeneratedNever();
            entity.Property(e => e.TinhTrang).HasMaxLength(50);
        });

        modelBuilder.Entity<SinhVien>(entity =>
        {
            entity.HasKey(e => e.Msv);

            entity.ToTable("SinhVien");

            entity.HasIndex(e => e.TenDn, "UQ__SinhVien__4CF96558A89D32B9").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__SinhVien__A9D105347870BF62").IsUnique();

            entity.Property(e => e.Msv)
                .ValueGeneratedNever()
                .HasColumnName("MSV");
            entity.Property(e => e.Avatar).HasMaxLength(255);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.GioiTinh).HasMaxLength(10);
            entity.Property(e => e.HoTen).HasMaxLength(100);
            entity.Property(e => e.Khoa).HasMaxLength(50);
            entity.Property(e => e.MatKhau).HasMaxLength(25);
            entity.Property(e => e.Sdt)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SDT");
            entity.Property(e => e.TenDn)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("TenDN");
        });

        modelBuilder.Entity<ThanNhan>(entity =>
        {
            entity.HasKey(e => e.MaPh);

            entity.ToTable("ThanNhan");

            entity.Property(e => e.MaPh)
                .ValueGeneratedNever()
                .HasColumnName("MaPH");
            entity.Property(e => e.HoTen).HasMaxLength(50);
            entity.Property(e => e.Msv).HasColumnName("MSV");
            entity.Property(e => e.QuanHe).HasMaxLength(10);
            entity.Property(e => e.Sdt)
                .HasMaxLength(10)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("SDT");

            entity.HasOne(d => d.MsvNavigation).WithMany(p => p.ThanNhans)
                .HasForeignKey(d => d.Msv)
                .HasConstraintName("FK_ThanNhan_SinhVien");
        });

        modelBuilder.Entity<ThongBao>(entity =>
        {
            entity.HasKey(e => e.MaTb);

            entity.ToTable("ThongBao");

            entity.Property(e => e.MaTb)
                .ValueGeneratedNever()
                .HasColumnName("MaTB");
            entity.Property(e => e.Msv).HasColumnName("MSV");
            entity.Property(e => e.NgayTb).HasColumnName("NgayTB");
            entity.Property(e => e.TieuDe).HasMaxLength(100);

            entity.HasOne(d => d.MsvNavigation).WithMany(p => p.ThongBaos)
                .HasForeignKey(d => d.Msv)
                .HasConstraintName("FK_ThongBao_SinhVien");
        });

        modelBuilder.Entity<TienDienNuoc>(entity =>
        {
            entity.HasKey(e => e.MaHddn);

            entity.ToTable("TienDienNuoc");

            entity.Property(e => e.MaHddn)
                .ValueGeneratedNever()
                .HasColumnName("MaHDDN");
            entity.Property(e => e.DotTtdn).HasColumnName("DotTTDN");
            entity.Property(e => e.Httdn).HasColumnName("HTTDN");
            entity.Property(e => e.NgayTtdn).HasColumnName("NgayTTDN");
            entity.Property(e => e.TienDien).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TienNuoc).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TongTienDn)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("TongTienDN");
            entity.Property(e => e.TrangThaiTtdn)
                .HasMaxLength(20)
                .HasColumnName("TrangThaiTTDN");

            entity.HasOne(d => d.MaPNavigation).WithMany(p => p.TienDienNuocs)
                .HasForeignKey(d => d.MaP)
                .HasConstraintName("FK_TienDienNuoc_Phong");
        });

        modelBuilder.Entity<TienPhong>(entity =>
        {
            entity.HasKey(e => e.MaHdp);

            entity.ToTable("TienPhong");

            entity.Property(e => e.MaHdp)
                .ValueGeneratedNever()
                .HasColumnName("MaHDP");
            entity.Property(e => e.HanTtp).HasColumnName("HanTTP");
            entity.Property(e => e.MaHd).HasColumnName("MaHD");
            entity.Property(e => e.NgayTtp).HasColumnName("NgayTTP");
            entity.Property(e => e.TongTienP).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TrangThaiTtp)
                .HasMaxLength(20)
                .HasColumnName("TrangThaiTTP");

            entity.HasOne(d => d.MaHdNavigation).WithMany(p => p.TienPhongs)
                .HasForeignKey(d => d.MaHd)
                .HasConstraintName("FK_TienPhong_HopDongPhong");
        });

        modelBuilder.Entity<TraLoi>(entity =>
        {
            entity.HasKey(e => e.MaTl);

            entity.ToTable("TraLoi");

            entity.Property(e => e.MaTl)
                .ValueGeneratedNever()
                .HasColumnName("MaTL");
            entity.Property(e => e.MaBd).HasColumnName("MaBD");
            entity.Property(e => e.Msv).HasColumnName("MSV");
            entity.Property(e => e.NgayTl).HasColumnName("NgayTL");
            entity.Property(e => e.NoiDungTl).HasColumnName("NoiDungTL");

            entity.HasOne(d => d.MaBdNavigation).WithMany(p => p.TraLois)
                .HasForeignKey(d => d.MaBd)
                .HasConstraintName("FK_TraLoi_BaiDang");
        });

        modelBuilder.Entity<YeuCau>(entity =>
        {
            entity.HasKey(e => e.MaYc);

            entity.ToTable("YeuCau");

            entity.Property(e => e.MaYc)
                .ValueGeneratedNever()
                .HasColumnName("MaYC");
            entity.Property(e => e.LoaiYc)
                .HasMaxLength(100)
                .HasColumnName("LoaiYC");
            entity.Property(e => e.Msv).HasColumnName("MSV");
            entity.Property(e => e.NgayGuiYc).HasColumnName("NgayGuiYC");
            entity.Property(e => e.NoiDungYc).HasColumnName("NoiDungYC");
            entity.Property(e => e.TrangThaiYc)
                .HasMaxLength(20)
                .HasColumnName("TrangThaiYC");

            entity.HasOne(d => d.MsvNavigation).WithMany(p => p.YeuCaus)
                .HasForeignKey(d => d.Msv)
                .HasConstraintName("FK_YeuCau_SinhVien");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    


    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
