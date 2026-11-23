-- =============================================
-- Script nạp dữ liệu mẫu cho CSDL QuanLyMuaBanTraSua
-- Khớp chính xác danh sách 22 hình ảnh trong WebApplication4/Images
-- =============================================
USE [QuanLyMuaBanTraSua]
GO

-- 1. NẠP DANH MỤC (categories)
SET IDENTITY_INSERT [dbo].[categories] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[categories] WHERE [id] = 1)
    INSERT INTO [dbo].[categories] ([id], [name], [description], [status]) VALUES (1, N'Trà sữa', N'Các loại trà sữa thơm béo chuẩn vị', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[categories] WHERE [id] = 2)
    INSERT INTO [dbo].[categories] ([id], [name], [description], [status]) VALUES (2, N'Trà trái cây', N'Trà trái cây tươi mát giải nhiệt', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[categories] WHERE [id] = 3)
    INSERT INTO [dbo].[categories] ([id], [name], [description], [status]) VALUES (3, N'Cà phê', N'Các loại cà phê đậm đà', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[categories] WHERE [id] = 4)
    INSERT INTO [dbo].[categories] ([id], [name], [description], [status]) VALUES (4, N'Topping thêm', N'Topping đa dạng hấp dẫn', 1);
SET IDENTITY_INSERT [dbo].[categories] OFF;
GO

-- 2. NẠP SẢN PHẨM (products)
SET IDENTITY_INSERT [dbo].[products] ON;

-- Trà sữa (Category 1)
IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 1)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (1, 1, N'Trà sữa truyền thống', N'Trà sữa thơm béo chuẩn vị truyền thống.', 30000, N'TraSuaTruyenThong.jpg', 1, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 2)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (2, 1, N'Trà sữa Matcha', N'Matcha Nhật Bản thơm dịu ngọt mát.', 35000, N'TraSuaMatCha.jpg', 1, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 3)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (3, 1, N'Trà sữa Ô Long Nướng', N'Vị trà ô long nướng thơm lừng đậm đà.', 39000, N'TraSuaOLongNuong.jpg', 1, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 4)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (4, 1, N'Trà sữa trân châu đường đen', N'Trà sữa béo ngậy kèm đường đen ngọt ngào.', 38000, N'TraSuaTranChauDuongDen.jpg', 1, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 5)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (5, 1, N'Trà sữa khoai môn', N'Hương khoai môn bùi bùi thơm ngon khó cưỡng.', 35000, N'TraSuaKhoaiMon.jpg', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 6)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (6, 1, N'Trà sữa Thái xanh', N'Trà thái xanh thanh mát, béo thơm chuẩn vị.', 32000, N'TraSuaThaiXanh.jpg', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 7)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (7, 1, N'Trà sữa Socola', N'Socola đậm đà hòa quyện trà sữa thơm ngọt.', 36000, N'TraSuaSocola.jpg', 0, 1);

-- Trà trái cây (Category 2)
IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 8)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (8, 2, N'Trà đào cam sả', N'Trà đào tươi kết hợp cùng hương cam sả thơm nồng.', 32000, N'TraDaoCamSa.jpg', 1, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 9)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (9, 2, N'Trà vải thanh mát', N'Trà vải ngọt thanh dịu mát cho ngày hè.', 30000, N'TraVaiThanhMat.jpg', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 10)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (10, 2, N'Trà xoài nhiệt đới', N'Trà xoài thơm lừng hòa quyện vị thanh khiết.', 35000, N'TraXoai.jpg', 1, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 11)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (11, 2, N'Trà chanh dây tươi', N'Chua ngọt sảng khoái đánh tan cơn khát.', 30000, N'TraChanhDay.jpg', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 12)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (12, 2, N'Trà ổi hồng', N'Hương ổi hồng ngọt dịu, màu sắc bắt mắt.', 34000, N'TraOiHong.jpg', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 13)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (13, 2, N'Trà dâu tây tươi', N'Vị dâu tây chua chua ngọt ngọt thơm mát.', 35000, N'TraDau.jpg', 1, 1);

-- Cà phê (Category 3)
IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 14)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (14, 3, N'Cà phê sữa đá', N'Cà phê đậm vị hòa quyện sữa béo ngậy.', 28000, N'CaPheSuaDa.jpg', 1, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 15)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (15, 3, N'Cà phê đen đá', N'Đậm đà hương vị cà phê phin truyền thống.', 25000, N'CaPheDen.jpg', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 16)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (16, 3, N'Bạc xỉu 3 tầng', N'Vị ngọt béo từ sữa hòa quyện chút đắng cà phê.', 29000, N'BacXiu.jpg', 1, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 17)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (17, 3, N'Cà phê muối', N'Lớp kem muối béo mặn hòa tan cà phê đậm đà.', 32000, N'CaPheMuoi.jpg', 0, 1);

-- Topping (Category 4)
IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 18)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (18, 4, N'Trân châu đen dẻo', N'Trân châu truyền thống dai dai dẻo dẻo.', 8000, N'TranChauDen.jpg', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 19)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (19, 4, N'Trân châu trắng giòn', N'Trân châu trắng giòn sần sật vui miệng.', 10000, N'TranChauTrang.jpg', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 20)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (20, 4, N'Pudding trứng', N'Pudding mềm mịn, béo ngậy tan ngay đầu lưỡi.', 10000, N'puudingTrung.jpg', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 21)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (21, 4, N'Thạch trái cây', N'Thạch thanh mát nhiều hương vị trái cây.', 8000, N'ThachTraiCay.jpg', 0, 1);

IF NOT EXISTS (SELECT 1 FROM [dbo].[products] WHERE [id] = 22)
    INSERT INTO [dbo].[products] ([id], [category_id], [name], [description], [price], [image], [is_featured], [status])
    VALUES (22, 4, N'Kem Cheese Macchiato', N'Lớp kem phô mai béo mặn sánh mịn thơm lừng.', 12000, N'KemChesse.jpg', 0, 1);

SET IDENTITY_INSERT [dbo].[products] OFF;
GO

-- 3. NẠP BẢNG TOPPINGS RIÊNG (toppings)
SET IDENTITY_INSERT [dbo].[toppings] ON;
IF NOT EXISTS (SELECT 1 FROM [dbo].[toppings] WHERE [id] = 1)
    INSERT INTO [dbo].[toppings] ([id], [name], [price], [image], [status]) VALUES (1, N'Trân châu đen', 8000, N'TranChauDen.jpg', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[toppings] WHERE [id] = 2)
    INSERT INTO [dbo].[toppings] ([id], [name], [price], [image], [status]) VALUES (2, N'Trân châu trắng', 10000, N'TranChauTrang.jpg', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[toppings] WHERE [id] = 3)
    INSERT INTO [dbo].[toppings] ([id], [name], [price], [image], [status]) VALUES (3, N'Pudding trứng', 10000, N'puudingTrung.jpg', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[toppings] WHERE [id] = 4)
    INSERT INTO [dbo].[toppings] ([id], [name], [price], [image], [status]) VALUES (4, N'Thạch trái cây', 8000, N'ThachTraiCay.jpg', 1);
IF NOT EXISTS (SELECT 1 FROM [dbo].[toppings] WHERE [id] = 5)
    INSERT INTO [dbo].[toppings] ([id], [name], [price], [image], [status]) VALUES (5, N'Kem Cheese', 12000, N'KemChesse.jpg', 1);
SET IDENTITY_INSERT [dbo].[toppings] OFF;
GO
