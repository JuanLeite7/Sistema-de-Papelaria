USE papelaria_db;

CREATE TABLE Usuarios (
    id_usuario INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL,
    email VARCHAR(120) NOT NULL UNIQUE,
    senha_hash VARCHAR(255) NOT NULL,
    tipo ENUM('ADMIN', 'VENDEDOR', 'ESTOQUISTA') NOT NULL DEFAULT 'VENDEDOR',
    status BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE Clientes (
    id_cliente INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(120) NOT NULL,
    cpf VARCHAR(14) UNIQUE,
    telefone VARCHAR(20),
    email VARCHAR(120),
    endereco VARCHAR(200)
);

CREATE TABLE Categorias (
    id_categoria INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(100) NOT NULL UNIQUE,
    descricao VARCHAR(255)
);

CREATE TABLE Fornecedores (
    id_fornecedor INT AUTO_INCREMENT PRIMARY KEY,
    razao_social VARCHAR(150) NOT NULL,
    nome_fantasia VARCHAR(120),
    cnpj VARCHAR(18) NOT NULL UNIQUE,
    telefone VARCHAR(20),
    email VARCHAR(120),
    endereco VARCHAR(200)
);

CREATE TABLE Produtos (
    id_produto INT AUTO_INCREMENT PRIMARY KEY,
    id_categoria INT NOT NULL,
    nome VARCHAR(120) NOT NULL,
    descricao VARCHAR(255),
    codigo_barras VARCHAR(50) UNIQUE,
    preco_venda DECIMAL(10,2) NOT NULL,
    status BOOLEAN NOT NULL DEFAULT TRUE,
    CHECK (preco_venda >= 0),
    FOREIGN KEY (id_categoria) REFERENCES Categorias(id_categoria)
);

CREATE TABLE Estoque (
    id_estoque INT AUTO_INCREMENT PRIMARY KEY,
    id_produto INT NOT NULL UNIQUE,
    quantidade_atual INT NOT NULL DEFAULT 0,
    quantidade_minima INT NOT NULL DEFAULT 0,
    CHECK (quantidade_atual >= 0),
    CHECK (quantidade_minima >= 0),
    FOREIGN KEY (id_produto) REFERENCES Produtos(id_produto)
);

CREATE TABLE Vendas (
    id_venda INT AUTO_INCREMENT PRIMARY KEY,
    id_cliente INT,
    id_usuario INT NOT NULL,
    data_venda DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    valor_total DECIMAL(10,2) NOT NULL DEFAULT 0,
    forma_pagamento ENUM('DINHEIRO', 'PIX', 'CARTAO_CREDITO', 'CARTAO_DEBITO') NOT NULL,
    status ENUM('CONCLUIDA', 'CANCELADA') NOT NULL DEFAULT 'CONCLUIDA',
    CHECK (valor_total >= 0),
    FOREIGN KEY (id_cliente) REFERENCES Clientes(id_cliente) ON DELETE SET NULL,
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario)
);

CREATE TABLE ItensVenda (
    id_item_venda INT AUTO_INCREMENT PRIMARY KEY,
    id_venda INT NOT NULL,
    id_produto INT NOT NULL,
    quantidade INT NOT NULL,
    preco_unitario DECIMAL(10,2) NOT NULL,
    desconto DECIMAL(10,2) NOT NULL DEFAULT 0,
    CHECK (quantidade > 0),
    CHECK (preco_unitario >= 0),
    CHECK (desconto >= 0),
    FOREIGN KEY (id_venda) REFERENCES Vendas(id_venda),
    FOREIGN KEY (id_produto) REFERENCES Produtos(id_produto)
);

CREATE TABLE PedidosCompra (
    id_pedido INT AUTO_INCREMENT PRIMARY KEY,
    id_fornecedor INT NOT NULL,
    id_usuario INT NOT NULL,
    data_pedido DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    status ENUM('PENDENTE', 'RECEBIDO', 'CANCELADO') NOT NULL DEFAULT 'PENDENTE',
    valor_total DECIMAL(10,2) NOT NULL DEFAULT 0,
    FOREIGN KEY (id_fornecedor) REFERENCES Fornecedores(id_fornecedor),
    FOREIGN KEY (id_usuario) REFERENCES Usuarios(id_usuario)
);

CREATE TABLE ItensPedidoCompra (
    id_item_pedido INT AUTO_INCREMENT PRIMARY KEY,
    id_pedido INT NOT NULL,
    id_produto INT NOT NULL,
    quantidade INT NOT NULL,
    preco_unitario DECIMAL(10,2) NOT NULL,
    CHECK (quantidade > 0),
    CHECK (preco_unitario >= 0),
    FOREIGN KEY (id_pedido) REFERENCES PedidosCompra(id_pedido),
    FOREIGN KEY (id_produto) REFERENCES Produtos(id_produto)
);

CREATE TABLE Promocoes (
    id_promocao INT AUTO_INCREMENT PRIMARY KEY,
    nome VARCHAR(120) NOT NULL,
    descricao VARCHAR(255),
    percentual_desconto DECIMAL(5,2) NOT NULL,
    data_inicio DATE NOT NULL,
    data_fim DATE NOT NULL,
    status BOOLEAN NOT NULL DEFAULT TRUE,
    CHECK (percentual_desconto > 0 AND percentual_desconto <= 100),
    CHECK (data_fim >= data_inicio)
);

INSERT INTO Usuarios (nome, email, senha_hash, tipo) VALUES
('Administrador', 'admin@papelaria.com', 'hash_admin', 'ADMIN'),
('Guilherme Cosme', 'guilherme.cosme@papelaria.com', 'hash_guilherme_cosme', 'VENDEDOR'),
('Patricia Kaiane', 'patricia.kaiane@papelaria.com', 'hash_patricia_kaiane', 'ESTOQUISTA'),
('Guilherme Francisquini', 'guilherme.francisquini@papelaria.com', 'hash_guilherme_francisquini', 'VENDEDOR');

INSERT INTO Clientes (nome, cpf, telefone, email, endereco) VALUES
('Joao Silva', '12345678901', '11999999999', 'joao@email.com', 'Rua das Flores, 100'),
('Maria Santos', '98765432100', '11888888888', 'maria@email.com', 'Avenida Brasil, 250'),
('Pedro Costa', '11122233344', '11777777777', 'pedro@email.com', 'Rua Central, 50'),
('Juliana Lima', '55566677788', '11666666666', 'juliana@email.com', 'Rua das Palmeiras, 80'),
('Fernanda Alves', '44433322211', '11555555555', 'fernanda@email.com', 'Rua Primavera, 120');

INSERT INTO Categorias (nome, descricao) VALUES
('Material Escolar', 'Cadernos, lapis, canetas e borrachas'),
('Informatica', 'Acessorios e equipamentos de informatica'),
('Escritorio', 'Materiais para escritorio'),
('Livros', 'Livros e materiais didaticos'),
('Impressao', 'Impressoras e cartuchos');

INSERT INTO Fornecedores (razao_social, nome_fantasia, cnpj, telefone, email, endereco) VALUES
('Distribuidora Escolar LTDA', 'Distribuidora Escolar', '12345678000199', '1133334444', 'contato@escolar.com', 'Rua Comercial, 100'),
('Tech Informatica LTDA', 'Tech Informatica', '98765432000188', '1144445555', 'vendas@tech.com', 'Avenida Tecnologia, 200'),
('Papel e Cia LTDA', 'Papel e Cia', '11222333000177', '1155556666', 'contato@papelcia.com', 'Rua do Comercio, 300'),
('Livros Brasil LTDA', 'Livros Brasil', '22333444000166', '1166667777', 'vendas@livros.com', 'Rua Literaria, 400');

INSERT INTO Produtos (id_categoria, nome, descricao, codigo_barras, preco_venda) VALUES
(1, 'Caderno 10 Materias', 'Caderno universitario', '100001', 25.90),
(1, 'Caneta Azul', 'Caneta esferografica azul', '100002', 2.50),
(1, 'Lapis HB', 'Lapis preto HB', '100003', 1.50),
(1, 'Borracha Branca', 'Borracha escolar', '100004', 2.00),
(2, 'Mouse USB', 'Mouse optico USB', '100005', 35.00),
(2, 'Teclado USB', 'Teclado para computador', '100006', 65.90),
(3, 'Grampeador', 'Grampeador de mesa', '100007', 18.90),
(3, 'Papel A4', 'Resma com 500 folhas', '100008', 29.90),
(4, 'Livro de Matematica', 'Livro didatico', '100009', 45.00),
(5, 'Cartucho Preto', 'Cartucho de tinta preta', '100010', 79.90),
(5, 'Impressora Multifuncional', 'Impressora multifuncional', '100011', 699.90),
(1, 'Estojo Escolar', 'Estojo para lapis e canetas', '100012', 19.90);

INSERT INTO Estoque (id_produto, quantidade_atual, quantidade_minima) VALUES
(1, 50, 10),
(2, 200, 30),
(3, 150, 20),
(4, 100, 15),
(5, 25, 5),
(6, 20, 5),
(7, 30, 5),
(8, 80, 15),
(9, 15, 5),
(10, 12, 3),
(11, 5, 2),
(12, 40, 10);

INSERT INTO Vendas (id_cliente, id_usuario, data_venda, valor_total, forma_pagamento, status) VALUES
(1, 2, NOW(), 30.90, 'PIX', 'CONCLUIDA'),
(2, 2, NOW(), 40.00, 'DINHEIRO', 'CONCLUIDA'),
(3, 1, NOW(), 699.90, 'CARTAO_CREDITO', 'CONCLUIDA'),
(4, 4, NOW(), 85.00, 'CARTAO_DEBITO', 'CONCLUIDA');

INSERT INTO ItensVenda (id_venda, id_produto, quantidade, preco_unitario, desconto) VALUES
(1, 1, 1, 25.90, 0),
(1, 2, 2, 2.50, 0),
(2, 5, 1, 35.00, 0),
(2, 3, 2, 1.50, 0),
(3, 11, 1, 699.90, 0),
(4, 6, 1, 65.90, 0),
(4, 7, 1, 18.90, 0);

INSERT INTO PedidosCompra (id_fornecedor, id_usuario, data_pedido, status, valor_total) VALUES
(1, 3, NOW(), 'RECEBIDO', 400.00),
(2, 3, NOW(), 'PENDENTE', 670.00),
(3, 1, NOW(), 'RECEBIDO', 360.00),
(4, 1, NOW(), 'PENDENTE', 450.00);

INSERT INTO ItensPedidoCompra (id_pedido, id_produto, quantidade, preco_unitario) VALUES
(1, 1, 20, 15.00),
(1, 2, 100, 1.00),
(2, 5, 10, 22.00),
(2, 6, 10, 45.00),
(3, 8, 20, 18.00),
(4, 9, 10, 30.00);

INSERT INTO Promocoes (nome, descricao, percentual_desconto, data_inicio, data_fim, status) VALUES
('Volta as Aulas', 'Desconto em materiais escolares', 10.00, '2026-01-01', '2027-02-28', TRUE),
('Semana da Informatica', 'Desconto em produtos de informatica', 15.00, '2026-11-01', '2026-12-31', TRUE),
('Promocao de Livros', 'Desconto em livros selecionados', 20.00, '2026-01-01', '2026-12-31', TRUE);

SELECT * FROM Usuarios;
SELECT * FROM Clientes;
SELECT * FROM Categorias;
SELECT * FROM Fornecedores;
SELECT * FROM Produtos;
SELECT * FROM Estoque;
SELECT * FROM Vendas;
SELECT * FROM ItensVenda;
SELECT * FROM PedidosCompra;
SELECT * FROM ItensPedidoCompra;
SELECT * FROM Promocoes;
