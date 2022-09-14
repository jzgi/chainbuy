create schema public;

comment on schema public is 'standard public schema';

alter schema public owner to postgres;

create type purchop_type as
(
    state smallint,
    label varchar(12),
    orgid integer,
    uid integer,
    uname varchar(12),
    utel varchar(11),
    stamp timestamp(0)
);

alter type purchop_type owner to postgres;

create type buyln_type as
(
    stockid integer,
    name varchar(12),
    wareid smallint,
    price money,
    qty smallint,
    qtyre smallint
);

alter type buyln_type owner to postgres;

create table entities
(
    typ smallint not null,
    status smallint default 0 not null,
    name varchar(12) not null,
    tip varchar(30),
    created timestamp(0),
    creator varchar(10),
    adapted timestamp(0),
    adapter varchar(10)
);

alter table entities owner to postgres;

create table users
(
    id serial not null
        constraint users_pk
            primary key,
    tel varchar(11) not null,
    im varchar(28),
    credential varchar(32),
    admly smallint default 0 not null,
    orgid smallint,
    orgly smallint default 0 not null,
    idcard varchar(18),
    icon bytea
)
    inherits (entities);

alter table users owner to postgres;

create index users_admly_idx
    on users (admly)
    where (admly > 0);

create unique index users_im_idx
    on users (im);

create unique index users_tel_idx
    on users (tel);

create index users_orgid_idx
    on users (orgid);

create table regs
(
    id smallint not null
        constraint regs_pk
            primary key,
    idx smallint,
    num smallint
)
    inherits (entities);

comment on column regs.num is 'sub resources';

alter table regs owner to postgres;

create table orgs
(
    id serial not null
        constraint orgs_pk
            primary key,
    fork smallint,
    sprid integer
        constraint orgs_sprid_fk
            references orgs_old,
    license varchar(20),
    trust boolean,
    regid smallint
        constraint orgs_regid_fk
            references regs
            on update cascade,
    addr varchar(30),
    x double precision,
    y double precision,
    tel varchar(11),
    mgrid integer
        constraint orgs_mgrid_fk
            references users,
    ctrid integer,
    icon bytea
)
    inherits (entities);

alter table orgs_old owner to postgres;

alter table users
    add constraint users_orgid_fk
        foreign key (orgid) references orgs_old;

create table dailys
(
    orgid integer,
    dt date,
    itemid smallint,
    count integer,
    amt money,
    qty integer
)
    inherits (entities);

alter table dailys owner to postgres;

create table ledgers_
(
    seq integer,
    acct varchar(20),
    name varchar(12),
    amt integer,
    bal integer,
    cs uuid,
    blockcs uuid,
    stamp timestamp(0)
);

alter table ledgers_ owner to postgres;

create table peerledgs_
(
    peerid smallint
)
    inherits (ledgers_);

alter table peerledgs_ owner to postgres;

create table peers_
(
    id smallint not null
        constraint peers_pk
            primary key,
    weburl varchar(50),
    secret varchar(16)
)
    inherits (entities);

alter table peers_ owner to postgres;

create table accts_
(
    no varchar(20),
    v integer
)
    inherits (entities);

alter table accts_ owner to postgres;

create table notes
(
    id serial not null,
    fromid integer,
    toid integer
)
    inherits (entities);

comment on table events is 'annoucements and notices';

alter table events owner to postgres;

create table buys
(
    id bigserial not null
        constraint buys_pk
            primary key,
    shpid integer not null,
    mrtid integer not null,
    uid integer not null,
    uname varchar(10),
    utel varchar(11),
    uaddr varchar(20),
    uim varchar(28),
    lns buyln_type[],
    pay money,
    payre money
)
    inherits (entities);

comment on table buys is 'customer buys';

alter table buys owner to postgres;

create table clears
(
    id serial not null
        constraint clears_pk
            primary key,
    dt date,
    orgid integer not null,
    sprid integer not null,
    orders integer,
    total money,
    rate money,
    pay integer
)
    inherits (entities);

alter table clears owner to postgres;

create table cats
(
    idx smallint,
    num smallint,
    constraint cats_pk
        primary key (typ)
)
    inherits (entities);

comment on column cats.num is 'sub resources';

alter table cats owner to postgres;

create table products
(
    id serial not null,
    srcid integer,
    store smallint,
    duration smallint,
    agt boolean,
    unit varchar(4),
    unitip varchar(12),
    unitx smallint,
    icon bytea,
    pic bytea,
    mat bytea
)
    inherits (entities);

alter table items owner to postgres;

create table items
(
    id serial not null
        constraint items_pk
            primary key,
    shpid integer,
    productid integer,
    unit varchar(4),
    unitstd varchar(4),
    unitx smallint,
    price money,
    "off" money,
    min smallint,
    max smallint,
    step smallint,
    icon bytea,
    pic bytea
)
    inherits (entities);

alter table wares owner to postgres;

create table lots
(
    id serial not null,
    productid integer,
    srcid integer,
    ctrid integer,
    ctrop varchar(12),
    ctron timestamp(0),
    price money,
    "off" money,
    cap integer,
    remain integer,
    min integer,
    max integer,
    step integer
)
    inherits (entities);

alter table lots owner to postgres;

create table books
(
    id bigserial not null
        constraint books_pk
            primary key,
    shpid integer not null,
    mrtid integer not null,
    ctrid integer not null,
    srcid integer not null,
    prvid integer not null,
    productid integer,
    lotid integer,
    unit varchar(4),
    unitx smallint,
    shipat date,
    price money,
    "off" money,
    qty integer,
    cut integer,
    pay money,
    refund money,
    srcop varchar(12),
    srcon timestamp(0),
    ctrop varchar(12),
    ctron timestamp(0),
    shpop varchar(12),
    shpon timestamp(0)
)
    inherits (entities);

alter table books owner to postgres;

create view users_vw(typ, status, name, tip, created, creator, adapted, adapter, id, tel, im, credential, admly, orgid, orgly, idcard, icon) as
SELECT u.typ,
       u.status,
       u.name,
       u.tip,
       u.created,
       u.creator,
       u.adapted,
       u.adapter,
       u.id,
       u.tel,
       u.im,
       u.credential,
       u.admly,
       u.orgid,
       u.orgly,
       u.idcard,
       u.icon IS NOT NULL AS icon
FROM users u;

alter table users_vw owner to postgres;

create view orgs_vw(typ, status, name, tip, created, creator, adapted, adapter, id, fork, sprid, license, trust, regid, addr, x, y, tel, ctrid, mgrid, mgrname, mgrtel, mgrim, icon) as
SELECT o.typ,
       o.status,
       o.name,
       o.tip,
       o.created,
       o.creator,
       o.adapted,
       o.adapter,
       o.id,
       o.fork,
       o.sprid,
       o.license,
       o.trust,
       o.regid,
       o.addr,
       o.x,
       o.y,
       o.tel,
       o.ctrid,
       o.mgrid,
       m.name             AS mgrname,
       m.tel              AS mgrtel,
       m.im               AS mgrim,
       o.icon IS NOT NULL AS icon
FROM orgs_old o
         LEFT JOIN users m
                   ON o.mgrid =
                      m.id;

alter table orgs_vw owner to postgres;

create function first_agg(anyelement, anyelement) returns anyelement
    immutable
    strict
    parallel safe
    language sql
as $$
SELECT $1
$$;

alter function first_agg(anyelement, anyelement) owner to postgres;

create function last_agg(anyelement, anyelement) returns anyelement
    immutable
    strict
    parallel safe
    language sql
as $$
SELECT $2
$$;

alter function last_agg(anyelement, anyelement) owner to postgres;

create aggregate first(anyelement) (
    sfunc = first_agg,
    stype = anyelement,
    parallel = safe
    );

alter aggregate first(anyelement) owner to postgres;

create aggregate last(anyelement) (
    sfunc = last_agg,
    stype = anyelement,
    parallel = safe
    );

alter aggregate last(anyelement) owner to postgres;

